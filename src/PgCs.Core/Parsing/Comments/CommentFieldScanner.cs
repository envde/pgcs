using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PgCs.Core.Parsing.Comments;

/// <summary>
/// Сканер служебных полей в комментариях
/// Парсит строки вида "comment: value" или "comment(value)"
/// </summary>
/// <remarks>
/// Поддерживает два формата записи полей:
/// 1. С двоеточием: comment: значение; to_type: тип; to_name: имя;
/// 2. Со скобками: comment(значение); to_type(тип); to_name(имя);
/// </remarks>
public sealed partial class CommentFieldScanner
{
    /// <summary>
    /// Паттерн для формата: field: значение
    /// Захватывает текст до следующего semicolon или конца строки
    /// </summary>
    [GeneratedRegex(@"(\w+)\s*:\s*([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColonFormatRegex();

    /// <summary>
    /// Паттерн для формата: field(значение)
    /// </summary>
    [GeneratedRegex(@"(\w+)\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ParenFormatRegex();

    /// <summary>
    /// Пытается извлечь значение конкретного поля из текста
    /// </summary>
    /// <param name="text">Текст комментария</param>
    /// <param name="fieldName">Имя поля (например, "comment", "to_type", "to_name")</param>
    /// <param name="value">Извлеченное значение</param>
    /// <returns>true если поле найдено</returns>
    public bool TryExtractField(string text, string fieldName, [NotNullWhen(true)] out string? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        // Пробуем формат с двоеточием: field: value
        var colonMatches = ColonFormatRegex().Matches(text);
        foreach (Match match in colonMatches)
        {
            if (match.Groups[1].Value.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                value = match.Groups[2].Value.Trim();
                return !string.IsNullOrEmpty(value);
            }
        }

        // Пробуем формат со скобками: field(value)
        var parenMatches = ParenFormatRegex().Matches(text);
        foreach (Match match in parenMatches)
        {
            if (match.Groups[1].Value.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                value = match.Groups[2].Value.Trim();
                return !string.IsNullOrEmpty(value);
            }
        }

        return false;
    }

    /// <summary>
    /// Проверяет, содержит ли текст служебные поля
    /// </summary>
    public bool HasFields(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return ColonFormatRegex().IsMatch(text) || ParenFormatRegex().IsMatch(text);
    }
}
