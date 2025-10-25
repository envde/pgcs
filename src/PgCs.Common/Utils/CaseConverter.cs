using System.Globalization;
using System.Text;

namespace PgCs.Common.Utils;

/// <summary>
/// Утилиты для конвертации строк между различными стилями написания.
/// Поддерживает snake_case, kebab-case, PascalCase, camelCase.
/// </summary>
public static class CaseConverter
{
    /// <summary>
    /// Преобразует строку в PascalCase.
    /// Примеры: user_profile → UserProfile, ACTIVE_USER → ActiveUser, my-var → MyVar
    /// </summary>
    /// <param name="input">Входная строка в любом формате</param>
    /// <returns>Строка в PascalCase</returns>
    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var parts = SplitName(input);
        var sb = new StringBuilder();

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            // Капитализируем первую букву, остальные lowercase
            sb.Append(char.ToUpper(part[0], CultureInfo.InvariantCulture));
            if (part.Length > 1)
            {
                sb.Append(part[1..].ToLower(CultureInfo.InvariantCulture));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Преобразует строку в camelCase.
    /// Примеры: user_id → userId, MY_VAR → myVar, my-var → myVar
    /// </summary>
    /// <param name="input">Входная строка в любом формате</param>
    /// <returns>Строка в camelCase</returns>
    public static string ToCamelCase(string input)
    {
        var pascalCase = ToPascalCase(input);
        if (string.IsNullOrEmpty(pascalCase))
            return pascalCase;

        return char.ToLower(pascalCase[0], CultureInfo.InvariantCulture) + pascalCase[1..];
    }

    /// <summary>
    /// Разделяет имя на части по различным разделителям.
    /// Поддерживает: snake_case, kebab-case, UPPER_SNAKE_CASE, пробелы
    /// </summary>
    /// <param name="input">Строка для разделения</param>
    /// <returns>Последовательность частей имени</returns>
    public static IEnumerable<string> SplitName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            yield break;

        var currentPart = new StringBuilder();

        foreach (var c in input)
        {
            // Разделители: underscore, dash, space
            if (c == '_' || c == '-' || char.IsWhiteSpace(c))
            {
                if (currentPart.Length > 0)
                {
                    yield return currentPart.ToString();
                    currentPart.Clear();
                }
            }
            else
            {
                currentPart.Append(c);
            }
        }

        if (currentPart.Length > 0)
        {
            yield return currentPart.ToString();
        }
    }
}
