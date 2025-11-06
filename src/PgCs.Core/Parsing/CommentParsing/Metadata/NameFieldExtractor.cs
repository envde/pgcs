using System.Text.RegularExpressions;

namespace PgCs.Core.Parsing.CommentParsing.Metadata;

/// <summary>
/// Экстрактор поля to_name: из inline-комментария
/// Поддерживает два формата:
/// - to_name: NewFieldName;
/// - to_name(NewFieldName)
/// </summary>
public sealed partial class NameFieldExtractor
{
    /// <summary>
    /// Паттерн для формата: to_name: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (comment:, to_type:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"to_name\s*:\s*([^;]+?)(?:\s*;\s*(?:comment|to_type)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColonPatternRegex();

    /// <summary>
    /// Паттерн для формата: to_name(значение)
    /// </summary>
    [GeneratedRegex(@"to_name\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ParenPatternRegex();

    /// <summary>
    /// Извлекает значение поля to_name: из текста
    /// </summary>
    /// <param name="text">Текст для парсинга</param>
    /// <returns>Значение to_name или null если не найдено</returns>
    /// <example>
    /// "comment: User ID; to_name: UserId;" → "UserId"
    /// "comment(User ID); to_name(UserId);" → "UserId"
    /// "comment: Simple comment;" → null
    /// </example>
    public string? Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // Пробуем формат: to_name: значение;
        var match = ColonPatternRegex().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Пробуем формат: to_name(значение)
        match = ParenPatternRegex().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }
}
