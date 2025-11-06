
using PgCs.Core.Tokenization.Scanners;

namespace PgCs.Core.Tokenization;

/// <summary>
/// Токенизатор SQL на основе конечного автомата (FSM)
/// Преобразует PostgreSQL SQL текст в последовательность токенов
/// Поддерживает PostgreSQL 18 синтаксис
/// </summary>
/// <remarks>
/// Токенизатор поддерживает полный синтаксис PostgreSQL, включая:
/// - Dollar-quoted strings ($tag$...$tag$)
/// - Quoted identifiers ("table_name")
/// - Line (--) и block (/* */) комментарии
/// - Числа (integer, decimal, scientific notation, hex, binary, octal)
/// - Операторы (::, ->, @>, <@, и т.д.)
/// - Ключевые слова PostgreSQL
/// </remarks>
public sealed class SqlTokenizer
{
    private readonly string _sourceText;

    /// <summary>
    /// Создает новый экземпляр токенизатора для указанного SQL текста
    /// </summary>
    /// <param name="sourceText">Исходный SQL текст для токенизации</param>
    /// <exception cref="ArgumentException">Если sourceText null или пустой</exception>
    public SqlTokenizer(string sourceText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);
        _sourceText = sourceText;
    }

    /// <summary>
    /// Токенизирует SQL текст и возвращает список токенов
    /// </summary>
    /// <param name="sql">SQL текст для токенизации</param>
    /// <returns>Список токенов, включая завершающий EOF токен</returns>
    /// <exception cref="ArgumentException">Если sql null или пустой</exception>
    /// <remarks>
    /// Метод всегда возвращает как минимум один токен (EOF).
    /// Все whitespace и комментарии сохраняются как trivia токены для точного воспроизведения оригинального текста.
    /// </remarks>
    public IReadOnlyList<SqlToken> Tokenize(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        var cursor = new TextCursor(sql);
        var tokens = new List<SqlToken>();

        while (!cursor.IsAtEnd())
        {
            var token = ScanToken(cursor, sql);
            if (token.HasValue)
            {
                tokens.Add(token.Value);
            }
        }

        // Добавляем EOF токен
        tokens.Add(CreateEofToken(cursor, sql));

        return tokens;
    }

    /// <summary>
    /// Сканирует следующий токен
    /// </summary>
    private SqlToken? ScanToken(TextCursor cursor, string sourceText)
    {
        var startPos = cursor.Position;
        var startLine = cursor.Line;
        var startColumn = cursor.Column;

        var ch = cursor.Current;
        cursor.Advance();

        var result = ch switch
        {
            // Whitespace
            ' ' or '\t' or '\r' or '\n' => ScanWhitespace(cursor, sourceText, startPos, startLine, startColumn),

            // Comments
            '-' when cursor.Current == '-' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlCommentScanner.ScanLineComment),
            '/' when cursor.Current == '*' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlCommentScanner.ScanBlockComment),

            // Dollar-quoted strings or operator $
            '$' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlStringScanner.ScanDollarQuotedString),

            // String literals
            '\'' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlStringScanner.ScanStringLiteral),

            // Quoted identifiers
            '"' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlStringScanner.ScanQuotedIdentifier),

            // Numbers
            >= '0' and <= '9' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlNumberScanner.ScanNumber),

            // Punctuation
            '(' => CreateToken(TokenType.OpenParen, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            ')' => CreateToken(TokenType.CloseParen, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            '[' => CreateToken(TokenType.OpenBracket, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            ']' => CreateToken(TokenType.CloseBracket, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            ';' => CreateToken(TokenType.Semicolon, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            ',' => CreateToken(TokenType.Comma, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            '.' => CreateToken(TokenType.Dot, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),

            // Operators
            _ when SqlCharClassifier.IsOperatorChar(ch)
                => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlOperatorScanner.ScanOperator),

            // Identifiers & Keywords
            _ when SqlCharClassifier.IsIdentifierStart(ch)
                => ScanIdentifierOrKeyword(cursor, sourceText, startPos, startLine, startColumn),

            _ => CreateToken(TokenType.Unknown, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn)
        };

        return result;
    }

    /// <summary>
    /// Универсальный метод сканирования с откатом курсора.
    /// Устраняет дублирование кода во всех Scan* методах.
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <param name="sourceText">Исходный текст</param>
    /// <param name="start">Начальная позиция</param>
    /// <param name="line">Номер строки</param>
    /// <param name="column">Номер колонки</param>
    /// <param name="scanFunc">Функция сканирования конкретного типа токена</param>
    /// <returns>Отсканированный токен</returns>
    private static SqlToken ScanWithRewind(
        TextCursor cursor,
        string sourceText,
        int start,
        int line,
        int column,
        Func<TextCursor, ScanResult> scanFunc)
    {
        // Откатываем курсор к начальной позиции
        cursor.RestoreSnapshot(new CursorPosition(start, line, column));

        // Сканируем токен
        var result = scanFunc(cursor);

        // Создаем токен с zero-allocation через Memory
        var valueMemory = sourceText.AsMemory(start, result.Length);
        return CreateToken(result.Type, valueMemory, start, line, column);
    }

    private static SqlToken ScanWhitespace(TextCursor cursor, string text, int start, int line, int column)
    {
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsWhitespace(cursor.Current))
        {
            cursor.Advance();
        }

        var length = cursor.Position - start;
        var valueMemory = text.AsMemory(start, length);
        return CreateToken(TokenType.Whitespace, valueMemory, start, line, column);
    }

    private static SqlToken ScanIdentifierOrKeyword(TextCursor cursor, string text, int start, int line, int column)
    {
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsIdentifierPart(cursor.Current))
        {
            cursor.Advance();
        }

        var length = cursor.Position - start;
        var valueMemory = text.AsMemory(start, length);

        // Проверяем ключевое слово используя Span для избежания аллокации
        var valueSpan = valueMemory.Span;
        var type = PostgresKeywords.IsKeyword(valueSpan) ? TokenType.Keyword : TokenType.Identifier;

        return CreateToken(type, valueMemory, start, line, column);
    }

    private static SqlToken CreateToken(TokenType type, ReadOnlyMemory<char> valueMemory, int start, int line, int column)
    {
        return new SqlToken
        {
            Type = type,
            ValueMemory = valueMemory,
            Span = new TextSpan(start, valueMemory.Length),
            Line = line,
            Column = column
        };
    }

    private static SqlToken CreateEofToken(TextCursor cursor, string sourceText)
    {
        return new SqlToken
        {
            Type = TokenType.EndOfFile,
            ValueMemory = ReadOnlyMemory<char>.Empty,
            Span = new TextSpan(cursor.Position, 0),
            Line = cursor.Line,
            Column = cursor.Column
        };
    }
}