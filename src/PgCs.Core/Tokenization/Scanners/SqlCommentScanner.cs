namespace PgCs.Core.Tokenization.Scanners;

/// <summary>
/// Сканер SQL комментариев
/// Обрабатывает однострочные (--) и многострочные (/* */) комментарии
/// </summary>
public sealed class SqlCommentScanner
{
    /// <summary>
    /// Сканирует однострочный комментарий: -- comment
    /// Читает до конца строки
    /// </summary>
    public static ScanResult ScanLineComment(TextCursor cursor)
    {
        var start = cursor.Position;

        // Пропускаем второй '-'
        cursor.Advance();

        // Читаем до конца строки
        while (!cursor.IsAtEnd() && cursor.Current != '\n')
        {
            cursor.Advance();
        }

        var length = cursor.Position - start;
        return new ScanResult(TokenType.LineComment, length);
    }

    /// <summary>
    /// Сканирует многострочный комментарий: /* comment */
    /// Поддерживает вложенные комментарии (PostgreSQL расширение)
    /// </summary>
    public static ScanResult ScanBlockComment(TextCursor cursor)
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

        var length = cursor.Position - start;
        return new ScanResult(TokenType.BlockComment, length);
    }
}