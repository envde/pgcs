namespace PgCs.Core.Lexer;

/// <summary>
/// Сканеры для различных типов SQL токенов PostgreSQL
/// Все сканеры возвращают (TokenKind, Length) tuple
/// </summary>
public static class SqlScanners
{
    // ==================== КОММЕНТАРИИ ====================

    /// <summary>
    /// Сканирует однострочный комментарий: -- comment
    /// Читает до конца строки
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <returns>(TokenKind.LineComment, длина токена)</returns>
    public static (TokenKind Type, int Length) ScanLineComment(TextCursor cursor)
    {
        var start = cursor.Position;

        // Пропускаем второй '-'
        cursor.Advance();

        // Читаем до конца строки
        while (!cursor.IsAtEnd() && cursor.Current != '\n')
        {
            cursor.Advance();
        }

        return (TokenKind.LineComment, cursor.Position - start);
    }

    /// <summary>
    /// Сканирует многострочный комментарий: /* comment */
    /// Поддерживает вложенные комментарии (PostgreSQL расширение)
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <returns>(TokenKind.BlockComment, длина токена)</returns>
    public static (TokenKind Type, int Length) ScanBlockComment(TextCursor cursor)
    {
        var start = cursor.Position;
        var nestLevel = 1; // Уровень вложенности

        // Пропускаем '*' после '/'
        cursor.Advance();

        while (!cursor.IsAtEnd() && nestLevel > 0)
        {
            // Проверяем начало вложенного комментария /*
            if (cursor.Current == '/' && cursor.Peek() == '*')
            {
                nestLevel++;
                cursor.Advance(); // /
                cursor.Advance(); // *
                continue;
            }

            // Проверяем конец комментария */
            if (cursor.Current == '*' && cursor.Peek() == '/')
            {
                nestLevel--;
                cursor.Advance(); // *
                cursor.Advance(); // /
                continue;
            }

            cursor.Advance();
        }

        return (TokenKind.BlockComment, cursor.Position - start);
    }

    // ==================== ЧИСЛА ====================

    /// <summary>
    /// Сканирует числовой литерал
    /// Форматы: 123, 123.456, 1.23e10, 0x1A (hex), 0b101 (binary), 0o17 (octal)
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <returns>(TokenKind.NumericLiteral, длина токена)</returns>
    public static (TokenKind Type, int Length) ScanNumber(TextCursor cursor)
    {
        var start = cursor.Position;

        // Проверяем специальные префиксы (PostgreSQL расширения)
        if (cursor.Current == '0' && !cursor.IsAtEnd())
        {
            var next = cursor.Peek();

            // Hexadecimal: 0x1A
            if (next is 'x' or 'X')
            {
                cursor.Advance(); // 0
                cursor.Advance(); // x

                while (!cursor.IsAtEnd() && IsHexDigit(cursor.Current))
                {
                    cursor.Advance();
                }

                return (TokenKind.NumericLiteral, cursor.Position - start);
            }

            // Binary: 0b101
            if (next is 'b' or 'B')
            {
                cursor.Advance(); // 0
                cursor.Advance(); // b

                while (!cursor.IsAtEnd() && IsBinaryDigit(cursor.Current))
                {
                    cursor.Advance();
                }

                return (TokenKind.NumericLiteral, cursor.Position - start);
            }

            // Octal: 0o17
            if (next is 'o' or 'O')
            {
                cursor.Advance(); // 0
                cursor.Advance(); // o

                while (!cursor.IsAtEnd() && IsOctalDigit(cursor.Current))
                {
                    cursor.Advance();
                }

                return (TokenKind.NumericLiteral, cursor.Position - start);
            }
        }

        // Целая часть
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsDigit(cursor.Current))
        {
            cursor.Advance();
        }

        // Десятичная часть
        if (cursor.Current == '.' && !cursor.IsAtEnd() && SqlCharClassifier.IsDigit(cursor.Peek()))
        {
            cursor.Advance(); // .

            while (!cursor.IsAtEnd() && SqlCharClassifier.IsDigit(cursor.Current))
            {
                cursor.Advance();
            }
        }

        // Экспонента
        if (cursor.Current is 'e' or 'E')
        {
            cursor.Advance(); // e

            // Опциональный знак
            if (cursor.Current is '+' or '-')
            {
                cursor.Advance();
            }

            while (!cursor.IsAtEnd() && SqlCharClassifier.IsDigit(cursor.Current))
            {
                cursor.Advance();
            }
        }

        return (TokenKind.NumericLiteral, cursor.Position - start);
    }

    private static bool IsHexDigit(char ch) =>
        SqlCharClassifier.IsDigit(ch) || ch is >= 'A' and <= 'F' or >= 'a' and <= 'f';

    private static bool IsBinaryDigit(char ch) =>
        ch is '0' or '1';

    private static bool IsOctalDigit(char ch) =>
        ch is >= '0' and <= '7';

    // ==================== ОПЕРАТОРЫ ====================

    /// <summary>
    /// Сканирует оператор (простой или составной)
    /// PostgreSQL поддерживает многосимвольные операторы: <=, >=, <>, !=, ||, &&, etc.
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <returns>(TokenKind.Operator, длина токена)</returns>
    public static (TokenKind Type, int Length) ScanOperator(TextCursor cursor)
    {
        var first = cursor.Current;
        cursor.Advance();

        if (cursor.IsAtEnd())
        {
            return (TokenKind.Operator, 1);
        }

        var second = cursor.Current;

        // Двухсимвольные операторы
        var twoCharOp = $"{first}{second}";
        if (IsTwoCharOperator(twoCharOp))
        {
            cursor.Advance();
            return (TokenKind.Operator, 2);
        }

        // Проверяем трёхсимвольные операторы
        if (!cursor.IsAtEnd())
        {
            var third = cursor.Peek();
            var threeCharOp = $"{first}{second}{third}";

            if (IsThreeCharOperator(threeCharOp))
            {
                cursor.Advance();
                cursor.Advance();
                return (TokenKind.Operator, 3);
            }
        }

        return (TokenKind.Operator, 1);
    }

    private static bool IsTwoCharOperator(string op) => op switch
    {
        "<=" or ">=" or "<>" or "!=" or "||" or "&&" or
        "::" or "->" or "->>" or "#>" or "#>>" or
        "@>" or "<@" or "?|" or "?&" or "~*" or "!~" or
        "!~*" or "@@" or "##" or "<->" or "<<" or ">>" or
        "&<" or "&>" or "<<|" or "|>>" or "&<|" or "|&>" => true,
        _ => false
    };

    private static bool IsThreeCharOperator(string op) => op switch
    {
        "!~~" or "~~*" or "!~~*" => true,
        _ => false
    };

    // ==================== СТРОКИ ====================

    /// <summary>
    /// Сканирует строковый литерал с одинарными кавычками: 'text'
    /// Поддерживает экранирование через двойные кавычки: 'don''t'
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <returns>(TokenKind.StringLiteral, длина токена)</returns>
    public static (TokenKind Type, int Length) ScanStringLiteral(TextCursor cursor)
    {
        var start = cursor.Position;

        // Пропускаем открывающую кавычку
        cursor.Advance();

        while (!cursor.IsAtEnd())
        {
            if (cursor.Current == '\'')
            {
                cursor.Advance();

                // Проверяем двойную кавычку '' (экранирование)
                if (cursor.Current == '\'')
                {
                    cursor.Advance(); // Пропускаем вторую кавычку
                    continue;
                }

                // Конец строки
                break;
            }

            cursor.Advance();
        }

        return (TokenKind.StringLiteral, cursor.Position - start);
    }

    /// <summary>
    /// Сканирует dollar-quoted строку: $$text$$ или $tag$text$tag$
    /// PostgreSQL поддерживает произвольные теги между $
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <returns>(TokenKind.DollarQuotedString или TokenKind.Operator, длина токена)</returns>
    public static (TokenKind Type, int Length) ScanDollarQuotedString(TextCursor cursor)
    {
        var start = cursor.Position;

        // Читаем открывающий тег: $ + [optional_tag] + $
        cursor.Advance(); // Первый $

        var tagStart = cursor.Position;

        // Читаем тег (может быть пустым)
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsIdentifierPart(cursor.Current))
        {
            cursor.Advance();
        }

        var tagEnd = cursor.Position;

        // Должен быть закрывающий $ тега
        if (cursor.Current != '$')
        {
            // Это не dollar-quoted string, а просто оператор $
            return (TokenKind.Operator, 1);
        }

        cursor.Advance(); // Закрывающий $ тега

        // Получаем тег как Span (zero-allocation)
        ReadOnlySpan<char> tag = cursor.GetTextSpan(tagStart, tagEnd - tagStart);

        // Создаём закрывающий тег: $tag$
        // Используем stackalloc для небольших строк (до 128 символов)
        Span<char> closingTag = tag.Length <= 126
            ? stackalloc char[tag.Length + 2]
            : new char[tag.Length + 2];

        closingTag[0] = '$';
        tag.CopyTo(closingTag[1..]);
        closingTag[^1] = '$';

        // Ищем закрывающий тег
        while (!cursor.IsAtEnd())
        {
            if (cursor.MatchSequence(closingTag))
            {
                // Пропускаем закрывающий тег
                for (var i = 0; i < closingTag.Length; i++)
                {
                    cursor.Advance();
                }
                break;
            }

            cursor.Advance();
        }

        return (TokenKind.DollarQuotedString, cursor.Position - start);
    }

    /// <summary>
    /// Сканирует quoted идентификатор: "Table Name"
    /// PostgreSQL не поддерживает экранирование внутри quoted identifiers
    /// </summary>
    /// <param name="cursor">Курсор текста</param>
    /// <returns>(TokenKind.QuotedIdentifier, длина токена)</returns>
    public static (TokenKind Type, int Length) ScanQuotedIdentifier(TextCursor cursor)
    {
        var start = cursor.Position;

        // Пропускаем открывающую кавычку
        cursor.Advance();

        while (!cursor.IsAtEnd() && cursor.Current != '"')
        {
            cursor.Advance();
        }

        if (!cursor.IsAtEnd())
        {
            cursor.Advance(); // Закрывающая кавычка
        }

        return (TokenKind.QuotedIdentifier, cursor.Position - start);
    }
}
