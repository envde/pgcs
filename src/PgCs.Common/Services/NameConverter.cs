using System.Globalization;
using System.Text;
using Humanizer;

namespace PgCs.Common.Services;

/// <summary>
/// Конвертер имен из PostgreSQL конвенций в C# конвенции
/// Использует библиотеку Humanizer для плюрализации
/// </summary>
public sealed class NameConverter : INameConverter
{
    public string ToClassName(string tableName)
    {
        // Преобразуем snake_case в PascalCase и делаем singular
        var pascalCase = ToPascalCase(tableName);
        
        // Используем Humanizer для singularization
        return pascalCase.Singularize(inputIsKnownToBePlural: false);
    }

    public string ToPropertyName(string columnName)
    {
        return ToPascalCase(columnName);
    }

    public string ToEnumMemberName(string enumValue)
    {
        // Обрабатываем UPPER_SNAKE_CASE, snake_case, kebab-case
        return ToPascalCase(enumValue);
    }

    public string ToMethodName(string functionName)
    {
        return ToPascalCase(functionName);
    }

    public string ToParameterName(string parameterName)
    {
        return ToCamelCase(parameterName);
    }

    /// <summary>
    /// Преобразует строку в PascalCase
    /// Например: user_profile -> UserProfile, ACTIVE_USER -> ActiveUser
    /// </summary>
    private static string ToPascalCase(string input)
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
    /// Преобразует строку в camelCase
    /// Например: user_id -> userId
    /// </summary>
    private static string ToCamelCase(string input)
    {
        var pascalCase = ToPascalCase(input);
        if (string.IsNullOrEmpty(pascalCase))
            return pascalCase;

        return char.ToLower(pascalCase[0], CultureInfo.InvariantCulture) + pascalCase[1..];
    }

    /// <summary>
    /// Разделяет имя на части по различным разделителям
    /// Поддерживает: snake_case, kebab-case, UPPER_SNAKE_CASE
    /// </summary>
    private static IEnumerable<string> SplitName(string input)
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
