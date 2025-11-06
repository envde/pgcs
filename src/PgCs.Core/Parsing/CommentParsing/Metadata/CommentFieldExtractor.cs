using System.Text.RegularExpressions;

namespace PgCs.Core.Parsing.CommentParsing.Metadata;

/// <summary>
/// Экстрактор поля comment: из inline-комментария
/// Поддерживает два формата:
/// - comment: Описание колонки;
/// - comment(Описание колонки)
/// </summary>
public sealed partial class CommentFieldExtractor
{
    /// <summary>
    /// Паттерн для формата: comment: значение (может быть с или без завершающей ;)
    /// Захватывает текст до следующего ключевого слова (to_type:, to_name:) или до конца строки
    /// </summary>
    [GeneratedRegex(@"comment\s*:\s*([^;]+?)(?:\s*;\s*(?:to_type|to_name)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColonPatternRegex();

    /// <summary>
    /// Паттерн для формата: comment(значение)
    /// </summary>
    [GeneratedRegex(@"comment\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ParenPatternRegex();

    /// <summary>
    /// Извлекает значение поля comment: из текста
    /// </summary>
    /// <param name="text">Текст для парсинга</param>
    /// <returns>Значение comment или null если не найдено</returns>
    /// <example>
    /// "comment: User ID; to_type: BIGINT;" → "User ID"
    /// "comment(User ID); to_type(BIGINT);" → "User ID"
    /// "to_type: BIGINT;" → null
    /// </example>
    public string? Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // Пробуем формат: comment: значение;
        var match = ColonPatternRegex().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Пробуем формат: comment(значение)
        match = ParenPatternRegex().Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }
}
