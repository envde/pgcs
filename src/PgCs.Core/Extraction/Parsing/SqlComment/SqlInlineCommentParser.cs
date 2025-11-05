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
    /// Паттерн для формата: comment: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (type:, rename:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"comment\s*:\s*([^;]+?)(?:\s*;\s*(?:type|rename)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CommentColonPattern();

    /// <summary>
    /// Паттерн для формата: comment(значение)
    /// </summary>
    [GeneratedRegex(@"comment\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CommentParenPattern();

    /// <summary>
    /// Паттерн для формата: type: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (comment:, rename:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"type\s*:\s*([^;]+?)(?:\s*;\s*(?:comment|rename)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeColonPattern();

    /// <summary>
    /// Паттерн для формата: type(значение)
    /// </summary>
    [GeneratedRegex(@"type\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeParenPattern();

    /// <summary>
    /// Паттерн для формата: rename: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (comment:, type:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"rename\s*:\s*([^;]+?)(?:\s*;\s*(?:comment|type)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
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
    /// <returns>Извлеченные данные или null, если комментарий пустой</returns>
    /// <remarks>
    /// Поддерживает:
    /// - Комментарии со служебными словами: comment: ...; type: ...; rename: ...
    /// - Комментарии со служебными словами в любом порядке
    /// - Комментарии с частичным набором служебных слов
    /// - Простые комментарии без служебных слов
    /// </remarks>
    public static SqlInlineComment? Parse(string? comment)
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

        // Если ничего не найдено через ключевые слова, считаем весь текст комментарием
        if (commentText is null && dataType is null && renameTo is null)
        {
            // Просто комментарий без служебных слов
            return new SqlInlineComment
            {
                Comment = cleanComment,
                DataType = null,
                RenameTo = null
            };
        }

        return new SqlInlineComment
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
