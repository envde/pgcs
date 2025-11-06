using PgCs.Core.Tokenization.Scanners;

namespace PgCs.Core.Tokenization;

/// <summary>
/// Токенизатор SQL на основе конечного автомата (FSM)
/// Преобразует PostgreSQL SQL текст в последовательность токенов
/// Поддерживает PostgreSQL 18 синтаксис
/// </summary>
public sealed class SqlTokenizer
{
    /// <summary>
    /// Токенизирует SQL текст
    /// </summary>
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
        tokens.Add(CreateEofToken(cursor));

        return tokens;
    }

    /// <summary>
    /// Сканирует следующий токен
    /// </summary>
    private static SqlToken? ScanToken(TextCursor cursor, string sourceText)
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
            '-' when cursor.Current == '-' => ScanLineComment(cursor, sourceText, startPos, startLine, startColumn),
            '/' when cursor.Current == '*' => ScanBlockComment(cursor, sourceText, startPos, startLine, startColumn),

            // Dollar-quoted strings or operator $
            '$' => ScanDollarQuotedOrOperator(cursor, sourceText, startPos, startLine, startColumn),

            // String literals
            '\'' => ScanStringLiteral(cursor, sourceText, startPos, startLine, startColumn),

            // Quoted identifiers
            '"' => ScanQuotedIdentifier(cursor, sourceText, startPos, startLine, startColumn),

            // Numbers
            >= '0' and <= '9' => ScanNumber(cursor, sourceText, startPos, startLine, startColumn),

            // Punctuation
            '(' => CreateToken(TokenType.OpenParen, "(", startPos, startLine, startColumn),
            ')' => CreateToken(TokenType.CloseParen, ")", startPos, startLine, startColumn),
            '[' => CreateToken(TokenType.OpenBracket, "[", startPos, startLine, startColumn),
            ']' => CreateToken(TokenType.CloseBracket, "]", startPos, startLine, startColumn),
            ';' => CreateToken(TokenType.Semicolon, ";", startPos, startLine, startColumn),
            ',' => CreateToken(TokenType.Comma, ",", startPos, startLine, startColumn),
            '.' => CreateToken(TokenType.Dot, ".", startPos, startLine, startColumn),

            // Operators
            _ when SqlCharClassifier.IsOperatorChar(ch)
                => ScanOperator(cursor, sourceText, startPos, startLine, startColumn),

            // Identifiers & Keywords
            _ when SqlCharClassifier.IsIdentifierStart(ch)
                => ScanIdentifierOrKeyword(cursor, sourceText, startPos, startLine, startColumn),

            _ => CreateToken(TokenType.Unknown, ch.ToString(), startPos, startLine, startColumn)
        };

        return result;
    }

    private static SqlToken ScanWhitespace(TextCursor cursor, string text, int start, int line, int column)
    {
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsWhitespace(cursor.Current))
        {
            cursor.Advance();
        }

        var length = cursor.Position - start;
        var value = text.Substring(start, length);
        return CreateToken(TokenType.Whitespace, value, start, line, column);
    }

    private static SqlToken ScanLineComment(TextCursor cursor, string text, int start, int line, int column)
    {
        var result = SqlCommentScanner.ScanLineComment(cursor);
        // result.Length is from cursor position (after first '-') to end of line
        // but start is at the first '-', so we need to add 1
        var totalLength = result.Length + 1;
        var value = text.Substring(start, totalLength);
        return CreateToken(result.Type, value, start, line, column);
    }

    private static SqlToken ScanBlockComment(TextCursor cursor, string text, int start, int line, int column)
    {
        var result = SqlCommentScanner.ScanBlockComment(cursor);
        // result.Length is from cursor position (after '/') to end of comment
        // but start is at the '/', so we need to add 1
        var totalLength = result.Length + 1;
        var value = text.Substring(start, totalLength);
        return CreateToken(result.Type, value, start, line, column);
    }

    private static SqlToken ScanDollarQuotedOrOperator(TextCursor cursor, string text, int start, int line, int column)
    {
        // Откатываем на 1 символ назад (на $)
        var snapshot = cursor.CreateSnapshot();
        cursor.RestoreSnapshot(new CursorPosition(start, line, column));

        var result = SqlStringScanner.ScanDollarQuotedString(cursor);
        var value = text.Substring(start, result.Length);
        return CreateToken(result.Type, value, start, line, column);
    }

    private static SqlToken ScanStringLiteral(TextCursor cursor, string text, int start, int line, int column)
    {
        // Откатываем на 1 символ назад (на ')
        var snapshot = cursor.CreateSnapshot();
        cursor.RestoreSnapshot(new CursorPosition(start, line, column));

        var result = SqlStringScanner.ScanStringLiteral(cursor);
        var value = text.Substring(start, result.Length);
        return CreateToken(result.Type, value, start, line, column);
    }

    private static SqlToken ScanQuotedIdentifier(TextCursor cursor, string text, int start, int line, int column)
    {
        // Откатываем на 1 символ назад (на ")
        var snapshot = cursor.CreateSnapshot();
        cursor.RestoreSnapshot(new CursorPosition(start, line, column));

        var result = SqlStringScanner.ScanQuotedIdentifier(cursor);
        var value = text.Substring(start, result.Length);
        return CreateToken(result.Type, value, start, line, column);
    }

    private static SqlToken ScanNumber(TextCursor cursor, string text, int start, int line, int column)
    {
        // Откатываем на 1 символ назад (на цифру)
        var snapshot = cursor.CreateSnapshot();
        cursor.RestoreSnapshot(new CursorPosition(start, line, column));

        var result = SqlNumberScanner.ScanNumber(cursor);
        var value = text.Substring(start, result.Length);
        return CreateToken(result.Type, value, start, line, column);
    }

    private static SqlToken ScanOperator(TextCursor cursor, string text, int start, int line, int column)
    {
        // Откатываем на 1 символ назад (на оператор)
        cursor.RestoreSnapshot(new CursorPosition(start, line, column));

        var result = SqlOperatorScanner.ScanOperator(cursor);
        var value = text.Substring(start, result.Length);
        return CreateToken(result.Type, value, start, line, column);
    }

    private static SqlToken ScanIdentifierOrKeyword(TextCursor cursor, string text, int start, int line, int column)
    {
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsIdentifierPart(cursor.Current))
        {
            cursor.Advance();
        }

        var length = cursor.Position - start;
        var value = text.Substring(start, length);
        var type = PostgresKeywords.IsKeyword(value) ? TokenType.Keyword : TokenType.Identifier;

        return CreateToken(type, value, start, line, column);
    }

    private static SqlToken CreateToken(TokenType type, string value, int start, int line, int column)
    {
        return new SqlToken
        {
            Type = type,
            Value = value,
            Span = new TextSpan(start, value.Length),
            Line = line,
            Column = column
        };
    }

    private static SqlToken CreateEofToken(TextCursor cursor)
    {
        return new SqlToken
        {
            Type = TokenType.EndOfFile,
            Value = string.Empty,
            Span = new TextSpan(cursor.Position, 0),
            Line = cursor.Line,
            Column = cursor.Column
        };
    }
}