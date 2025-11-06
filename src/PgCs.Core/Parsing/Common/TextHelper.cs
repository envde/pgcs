namespace PgCs.Core.Parsing.Common;

/// <summary>
/// Вспомогательный класс для работы с текстом в парсере блоков
/// </summary>
public static class TextHelper
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
    public static string ExtractCommentText(string commentToken)
    {
        return commentToken.TrimStart('-', ' ', '\t').TrimEnd('\r', '\n');
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
    public static bool ContainsEmptyLine(string whitespace)
    {
        return whitespace.Contains("\n\n") || whitespace.Contains("\r\n\r\n");
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
    public static string UnquoteIdentifier(string quotedIdentifier)
    {
        return quotedIdentifier.Trim('"');
    }
}
