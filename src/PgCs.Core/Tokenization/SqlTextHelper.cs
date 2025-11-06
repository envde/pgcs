namespace PgCs.Core.Tokenization;

/// <summary>
/// Вспомогательный класс для работы с текстовым содержимым SQL токенов
/// Использует ReadOnlySpan для zero-allocation операций
/// </summary>
public static class SqlTextHelper
{
    /// <summary>
    /// Извлекает текст комментария (убирает префикс --)
    /// Также убирает trailing newlines (которые включаются в LineComment токен)
    /// </summary>
    /// <param name="commentToken">Токен комментария с префиксом --</param>
    /// <returns>Текст комментария без префикса, пробелов и newlines</returns>
    /// <example>
    /// "-- User table" → "User table"
    /// "-- comment: Description;" → "comment: Description;"
    /// </example>
    public static ReadOnlySpan<char> ExtractCommentText(ReadOnlySpan<char> commentToken)
    {
        var span = commentToken;

        // Trim start: убираем '-', ' ', '\t'
        while (span.Length > 0 && (span[0] == '-' || span[0] == ' ' || span[0] == '\t'))
        {
            span = span[1..];
        }

        // Trim end: убираем '\r', '\n'
        while (span.Length > 0 && (span[^1] == '\r' || span[^1] == '\n'))
        {
            span = span[..^1];
        }

        return span;
    }

    /// <summary>
    /// Проверяет, содержит ли whitespace пустую строку (два переноса подряд)
    /// Пустая строка используется для разделения header комментариев
    /// </summary>
    /// <param name="whitespace">Whitespace текст</param>
    /// <returns>true если содержит пустую строку</returns>
    /// <example>
    /// "\n\n" → true
    /// "\r\n\r\n" → true
    /// "  \n  " → false
    /// </example>
    public static bool ContainsEmptyLine(ReadOnlySpan<char> whitespace)
    {
        // Проверяем наличие "\n\n"
        for (int i = 0; i < whitespace.Length - 1; i++)
        {
            if (whitespace[i] == '\n' && whitespace[i + 1] == '\n')
            {
                return true;
            }
        }

        // Проверяем наличие "\r\n\r\n"
        for (int i = 0; i < whitespace.Length - 3; i++)
        {
            if (whitespace[i] == '\r' && whitespace[i + 1] == '\n' &&
                whitespace[i + 2] == '\r' && whitespace[i + 3] == '\n')
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Извлекает идентификатор из quoted строки
    /// </summary>
    /// <param name="quotedIdentifier">Идентификатор в кавычках</param>
    /// <returns>Идентификатор без кавычек</returns>
    /// <example>
    /// "\"Table Name\"" → "Table Name"
    /// "username" → "username"
    /// </example>
    public static ReadOnlySpan<char> UnquoteIdentifier(ReadOnlySpan<char> quotedIdentifier)
    {
        return quotedIdentifier.Trim('"');
    }
}
