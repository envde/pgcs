namespace PgCs.Core.Lexer;

/// <summary>
/// Лексер SQL на основе конечного автомата (FSM)
/// Преобразует PostgreSQL SQL текст в последовательность токенов
/// Поддерживает PostgreSQL 18 синтаксис
/// </summary>
/// <remarks>
/// Лексер поддерживает полный синтаксис PostgreSQL, включая:
/// - Dollar-quoted strings ($tag$...$tag$)
/// - Quoted identifiers ("table_name")
/// - Line (--) и block (/* */) комментарии
/// - Числа (integer, decimal, scientific notation, hex, binary, octal)
/// - Операторы (::, ->, @>, <@, и т.д.)
/// - Ключевые слова PostgreSQL
/// </remarks>
public sealed class SqlLexer
{
    private readonly string _sourceText;

    /// <summary>
    /// Создает новый экземпляр лексера для указанного SQL текста
    /// </summary>
    /// <param name="sourceText">Исходный SQL текст для лексического анализа</param>
    /// <exception cref="ArgumentNullException">Если sourceText null</exception>
    public SqlLexer(string sourceText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        _sourceText = sourceText;
    }

    /// <summary>
    /// Выполняет лексический анализ SQL текста и возвращает список токенов
    /// </summary>
    /// <param name="sql">SQL текст для анализа</param>
    /// <returns>Список токенов, включая завершающий EOF токен</returns>
    /// <exception cref="ArgumentNullException">Если sql null</exception>
    /// <remarks>
    /// Метод всегда возвращает как минимум один токен (EOF).
    /// Все whitespace и комментарии сохраняются как trivia токены для точного воспроизведения оригинального текста.
    /// </remarks>
    public IReadOnlyList<Token> Tokenize(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);

        var cursor = new TextCursor(sql);
        var tokens = new List<Token>();

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
    private Token? ScanToken(TextCursor cursor, string sourceText)
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
            '-' when cursor.Current == '-' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlScanners.ScanLineComment),
            '/' when cursor.Current == '*' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlScanners.ScanBlockComment),

            // Dollar-quoted strings or operator $
            '$' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlScanners.ScanDollarQuotedString),

            // String literals
            '\'' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlScanners.ScanStringLiteral),

            // Quoted identifiers
            '"' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlScanners.ScanQuotedIdentifier),

            // Numbers
            >= '0' and <= '9' => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlScanners.ScanNumber),

            // Punctuation
            '(' => CreateToken(TokenKind.OpenParen, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            ')' => CreateToken(TokenKind.CloseParen, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            '[' => CreateToken(TokenKind.OpenBracket, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            ']' => CreateToken(TokenKind.CloseBracket, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            ';' => CreateToken(TokenKind.Semicolon, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            ',' => CreateToken(TokenKind.Comma, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),
            '.' => CreateToken(TokenKind.Dot, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn),

            // Operators
            _ when SqlCharClassifier.IsOperatorChar(ch)
                => ScanWithRewind(cursor, sourceText, startPos, startLine, startColumn, SqlScanners.ScanOperator),

            // Identifiers & Keywords
            _ when SqlCharClassifier.IsIdentifierStart(ch)
                => ScanIdentifierOrKeyword(cursor, sourceText, startPos, startLine, startColumn),

            _ => CreateToken(TokenKind.Unknown, sourceText.AsMemory(startPos, 1), startPos, startLine, startColumn)
        };

        return result;
    }

    /// <summary>
    /// Универсальный метод сканирования с откатом курсора
    /// Устраняет дублирование кода во всех Scan* методах
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <param name="sourceText">Исходный текст</param>
    /// <param name="start">Начальная позиция</param>
    /// <param name="line">Номер строки</param>
    /// <param name="column">Номер колонки</param>
    /// <param name="scanFunc">Функция сканирования конкретного типа токена</param>
    /// <returns>Отсканированный токен</returns>
    private static Token ScanWithRewind(
        TextCursor cursor,
        string sourceText,
        int start,
        int line,
        int column,
        Func<TextCursor, (TokenKind Type, int Length)> scanFunc)
    {
        // Откатываем курсор к начальной позиции
        cursor.RestoreSnapshot(new CursorPosition(start, line, column));

        // Сканируем токен
        var (type, length) = scanFunc(cursor);

        // Создаем токен с zero-allocation через Memory
        var valueMemory = sourceText.AsMemory(start, length);
        return CreateToken(type, valueMemory, start, line, column);
    }

    private static Token ScanWhitespace(TextCursor cursor, string text, int start, int line, int column)
    {
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsWhitespace(cursor.Current))
        {
            cursor.Advance();
        }

        var length = cursor.Position - start;
        var valueMemory = text.AsMemory(start, length);
        return CreateToken(TokenKind.Whitespace, valueMemory, start, line, column);
    }

    private static Token ScanIdentifierOrKeyword(TextCursor cursor, string text, int start, int line, int column)
    {
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsIdentifierPart(cursor.Current))
        {
            cursor.Advance();
        }

        var length = cursor.Position - start;
        var valueMemory = text.AsMemory(start, length);

        // Проверяем ключевое слово используя Span для избежания аллокации
        var valueSpan = valueMemory.Span;
        var type = PostgresKeywords.IsKeyword(valueSpan) ? TokenKind.Keyword : TokenKind.Identifier;

        return CreateToken(type, valueMemory, start, line, column);
    }

    private static Token CreateToken(TokenKind type, ReadOnlyMemory<char> valueMemory, int start, int line, int column)
    {
        return new Token
        {
            Kind = type,
            ValueMemory = valueMemory,
            Span = new Token.TextSpan(start, valueMemory.Length),
            Line = line,
            Column = column
        };
    }

    private static Token CreateEofToken(TextCursor cursor, string sourceText)
    {
        return new Token
        {
            Kind = TokenKind.EndOfFile,
            ValueMemory = ReadOnlyMemory<char>.Empty,
            Span = new Token.TextSpan(cursor.Position, 0),
            Line = cursor.Line,
            Column = cursor.Column
        };
    }
}