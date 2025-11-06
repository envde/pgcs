using System.Text.RegularExpressions;

namespace PgCs.Core.Parsing.CommentParsing.Metadata;

/// <summary>
/// Экстрактор поля to_type: из inline-комментария
/// Поддерживает два формата:
/// - to_type: VARCHAR(100);
/// - to_type(VARCHAR(100))
/// </summary>
public sealed partial class TypeFieldExtractor
{
    /// <summary>
    /// Паттерн для формата: to_type: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (comment:, to_name:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"to_type\s*:\s*([^;]+?)(?:\s*;\s*(?:comment|to_name)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColonPatternRegex();

    /// <summary>
    /// Паттерн для формата: to_type(значение)
    /// </summary>
    [GeneratedRegex(@"to_type\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ParenPatternRegex();

    /// <summary>
    /// Извлекает значение поля to_type: из текста
    /// </summary>
    /// <param name="text">Текст для парсинга</param>
    /// <returns>Значение to_type или null если не найдено</returns>
    /// <example>
    /// "comment: User ID; to_type: BIGINT;" → "BIGINT"
    /// "comment(User ID); to_type(VARCHAR(100));" → "VARCHAR(100)"
    /// "comment: Simple comment;" → null
    /// </example>
    public string? Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // Пробуем формат: to_type: значение;
        var match = ColonPatternRegex().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Пробуем формат: to_type(значение)
        match = ParenPatternRegex().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }
}
