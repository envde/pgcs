using System.Text.RegularExpressions;

namespace PgCs.Core.Extraction.Parsing.SqlComment;

/// <summary>
/// Парсер inline-комментариев специального формата для извлечения метаданных колонок
/// <para>
/// Поддерживает два формата:
/// - comment: Описание колонки; type: BIGINT; rename: NewName;
/// - comment(Описание колонки); type(BIGINT); rename(NewName);
/// </para>
/// </summary>
public static partial class SqlInlineCommentParser
{
    /// <summary>
    /// Паттерн для формата: comment: значение;
    /// </summary>
    [GeneratedRegex(@"comment\s*:\s*([^;]+);?", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CommentColonPattern();

    /// <summary>
    /// Паттерн для формата: comment(значение)
    /// </summary>
    [GeneratedRegex(@"comment\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CommentParenPattern();

    /// <summary>
    /// Паттерн для формата: type: значение;
    /// </summary>
    [GeneratedRegex(@"type\s*:\s*([^;]+);?", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeColonPattern();

    /// <summary>
    /// Паттерн для формата: type(значение)
    /// </summary>
    [GeneratedRegex(@"type\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeParenPattern();

    /// <summary>
    /// Паттерн для формата: rename: значение;
    /// </summary>
    [GeneratedRegex(@"rename\s*:\s*([^;]+);?", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex RenameColonPattern();

    /// <summary>
    /// Паттерн для формата: rename(значение)
    /// </summary>
    [GeneratedRegex(@"rename\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex RenameParenPattern();

    /// <summary>
    /// Парсит inline-комментарий и извлекает метаданные
    /// </summary>
    /// <param name="comment">Текст комментария (без префикса --)</param>
    /// <returns>Извлеченные данные или null, если комментарий не содержит метаданных</returns>
    public static SqlComment? Parse(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        // Убираем префикс -- если он есть
        var cleanComment = comment.TrimStart('-').Trim();

        var commentText = ExtractComment(cleanComment);
        var dataType = ExtractDataType(cleanComment);
        var renameTo = ExtractRenameTo(cleanComment);

        // Если ничего не найдено, возвращаем null
        if (commentText is null && dataType is null && renameTo is null)
        {
            return null;
        }

        return new SqlComment
        {
            Comment = commentText,
            DataType = dataType,
            RenameTo = renameTo
        };
    }

    /// <summary>
    /// Извлекает текст комментария из строки
    /// </summary>
    private static string? ExtractComment(string text)
    {
        // Пробуем формат: comment: значение;
        var match = CommentColonPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Пробуем формат: comment(значение)
        match = CommentParenPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }

    /// <summary>
    /// Извлекает тип данных из строки
    /// </summary>
    private static string? ExtractDataType(string text)
    {
        // Пробуем формат: type: значение;
        var match = TypeColonPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Пробуем формат: type(значение)
        match = TypeParenPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }

    /// <summary>
    /// Извлекает переименованное имя из строки
    /// </summary>
    private static string? ExtractRenameTo(string text)
    {
        // Пробуем формат: rename: значение;
        var match = RenameColonPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Пробуем формат: rename(значение)
        match = RenameParenPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }
}
