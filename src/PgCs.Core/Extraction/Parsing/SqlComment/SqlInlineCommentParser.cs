using System.Text.RegularExpressions;

namespace PgCs.Core.Extraction.Parsing.SqlComment;

/// <summary>
/// Парсер inline-комментариев специального формата для извлечения метаданных колонок
/// <para>
/// Поддерживает два формата:
/// - comment: Описание колонки; to_type: BIGINT; to_name: NewName;
/// - comment(Описание колонки); to_type(BIGINT); to_name(NewName);
/// </para>
/// </summary>
public static partial class SqlInlineCommentParser
{
    /// <summary>
    /// Паттерн для формата: comment: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (to_type:, to_name:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"comment\s*:\s*([^;]+?)(?:\s*;\s*(?:to_type|to_name)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CommentColonPattern();

    /// <summary>
    /// Паттерн для формата: comment(значение)
    /// </summary>
    [GeneratedRegex(@"comment\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CommentParenPattern();

    /// <summary>
    /// Паттерн для формата: to_type: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (comment:, to_name:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"to_type\s*:\s*([^;]+?)(?:\s*;\s*(?:comment|to_name)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeColonPattern();

    /// <summary>
    /// Паттерн для формата: to_type(значение)
    /// </summary>
    [GeneratedRegex(@"to_type\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeParenPattern();

    /// <summary>
    /// Паттерн для формата: to_name: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (comment:, to_type:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"to_name\s*:\s*([^;]+?)(?:\s*;\s*(?:comment|to_type)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex RenameColonPattern();

    /// <summary>
    /// Паттерн для формата: to_name(значение)
    /// </summary>
    [GeneratedRegex(@"to_name\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex RenameParenPattern();

    /// <summary>
    /// Парсит inline-комментарий и извлекает метаданные
    /// </summary>
    /// <param name="comment">Текст комментария (без префикса --)</param>
    /// <returns>Извлеченные данные или null, если комментарий пустой</returns>
    /// <remarks>
    /// Поддерживает:
    /// - Комментарии со служебными словами: comment: ...; to_type: ...; to_name: ...
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
                ToDateType = null,
                ToName = null
            };
        }

        return new SqlInlineComment
        {
            Comment = commentText,
            ToDateType = dataType,
            ToName = renameTo
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
        // Пробуем формат: to_type: значение;
        var match = TypeColonPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Пробуем формат: to_type(значение)
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
        // Пробуем формат: to_name: значение;
        var match = RenameColonPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Пробуем формат: to_name(значение)
        match = RenameParenPattern().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }
}
