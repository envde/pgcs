using System.Text;
using System.Text.RegularExpressions;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Formatting;

/// <summary>
/// Утилита для преобразования имён в различные стили именования
/// </summary>
internal static partial class NamingHelper
{
    /// <summary>
    /// Преобразует имя в соответствии с заданной стратегией
    /// </summary>
    public static string ConvertName(string name, NamingStrategy strategy, string? prefix = null, string? suffix = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var converted = strategy switch
        {
            NamingStrategy.PascalCase => ToPascalCase(name),
            NamingStrategy.CamelCase => ToCamelCase(name),
            NamingStrategy.SnakeCase => ToSnakeCase(name),
            NamingStrategy.AsIs => name,
            _ => ToPascalCase(name)
        };

        // Добавляем префикс и суффикс
        if (!string.IsNullOrEmpty(prefix))
        {
            converted = prefix + converted;
        }

        if (!string.IsNullOrEmpty(suffix))
        {
            converted += suffix;
        }

        return converted;
    }

    /// <summary>
    /// Преобразует имя в PascalCase
    /// </summary>
    public static string ToPascalCase(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // Разделяем по подчёркиваниям, дефисам и пробелам
        var words = SplitWords(name);
        var result = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                result.Append(char.ToUpperInvariant(word[0]));
                if (word.Length > 1)
                {
                    result.Append(word[1..].ToLowerInvariant());
                }
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Преобразует имя в camelCase
    /// </summary>
    public static string ToCamelCase(string name)
    {
        var pascalCase = ToPascalCase(name);
        
        if (pascalCase.Length == 0)
        {
            return pascalCase;
        }

        return char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
    }

    /// <summary>
    /// Преобразует имя в snake_case
    /// </summary>
    public static string ToSnakeCase(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // Если уже в snake_case, возвращаем как есть
        if (SnakeCaseRegex().IsMatch(name))
        {
            return name.ToLowerInvariant();
        }

        var words = SplitWords(name);
        return string.Join("_", words.Select(w => w.ToLowerInvariant()));
    }

    /// <summary>
    /// Разбивает строку на слова
    /// </summary>
    private static string[] SplitWords(string name)
    {
        // Сначала заменяем разделители на пробелы
        var normalized = name.Replace('_', ' ')
                            .Replace('-', ' ')
                            .Trim();

        // Затем разбиваем по camelCase/PascalCase
        normalized = CamelCaseRegex().Replace(normalized, " $1");

        // Разделяем по пробелам и фильтруем пустые
        return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Делает первую букву строки заглавной
    /// </summary>
    public static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// Проверяет, является ли имя зарезервированным ключевым словом C#
    /// </summary>
    public static bool IsReservedKeyword(string name)
    {
        return ReservedKeywords.Contains(name);
    }

    /// <summary>
    /// Экранирует имя, если оно является зарезервированным ключевым словом
    /// </summary>
    public static string EscapeIfKeyword(string name)
    {
        return IsReservedKeyword(name) ? $"@{name}" : name;
    }

    /// <summary>
    /// Делает имя множественным (простая реализация)
    /// </summary>
    public static string Pluralize(string name)
    {
        if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && 
            !name.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
            !name.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
            !name.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
            !name.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
        {
            return name[..^1] + "ies";
        }

        if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            return name + "es";
        }

        return name + "s";
    }

    /// <summary>
    /// Regex для определения snake_case
    /// </summary>
    [GeneratedRegex(@"^[a-z][a-z0-9_]*$")]
    private static partial Regex SnakeCaseRegex();

    /// <summary>
    /// Regex для разбиения camelCase/PascalCase
    /// </summary>
    [GeneratedRegex(@"([A-Z][a-z]+)")]
    private static partial Regex CamelCaseRegex();

    /// <summary>
    /// Зарезервированные ключевые слова C#
    /// </summary>
    private static readonly HashSet<string> ReservedKeywords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
        "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
        "virtual", "void", "volatile", "while", "required", "init", "record", "and", "or", "not"
    ];
}
