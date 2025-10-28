using System.Text.RegularExpressions;

namespace PgCs.Core.Extraction.Block;

/// <summary>
/// Процессор SQL комментариев.
/// Обрабатывает однострочные комментарии PostgreSQL (--).
/// </summary>
public sealed partial class CommentProcessor
{
    [GeneratedRegex(@"^\s*--", RegexOptions.Compiled)]
    private static partial Regex CommentLineRegex();

    [GeneratedRegex(@"--\s*(.*)$", RegexOptions.Compiled)]
    private static partial Regex InlineCommentRegex();

    /// <summary>
    /// Проверяет, начинается ли строка с комментария (--).
    /// </summary>
    /// <param name="line">Строка для проверки</param>
    /// <returns>true, если строка является комментарием</returns>
    /// <example>
    /// "-- This is a comment" → true
    /// "SELECT * FROM users -- comment" → false (не начинается с комментария)
    /// </example>
    public bool IsCommentLine(string line)
    {
        return CommentLineRegex().IsMatch(line);
    }

    /// <summary>
    /// Извлекает текст комментария из строки.
    /// Удаляет префикс "--" и лишние пробелы.
    /// </summary>
    /// <param name="line">Строка с комментарием</param>
    /// <returns>Текст комментария без "--" и пробелов</returns>
    /// <example>
    /// "-- User table" → "User table"
    /// "SELECT * FROM users -- get all" → "get all"
    /// </example>
    public string ExtractCommentText(string line)
    {
        var match = InlineCommentRegex().Match(line);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    /// <summary>
    /// Разделяет строку на SQL код и inline комментарий.
    /// </summary>
    /// <param name="line">Строка для разделения</param>
    /// <returns>
    /// Кортеж: (код до комментария, текст комментария или null)
    /// </returns>
    /// <example>
    /// "SELECT * FROM users -- fetch all" → ("SELECT * FROM users ", "fetch all")
    /// "SELECT * FROM users" → ("SELECT * FROM users", null)
    /// </example>
    public (string CodeBeforeComment, string? Comment) SplitInlineComment(string line)
    {
        var match = InlineCommentRegex().Match(line);
        if (!match.Success)
        {
            return (line, null);
        }

        var codeBeforeComment = line[..match.Index];
        var comment = match.Groups[1].Value.Trim();
        return (codeBeforeComment, comment);
    }
}