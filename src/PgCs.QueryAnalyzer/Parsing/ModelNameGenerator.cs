using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.QueryAnalyzer.Parsing;

using PgCs.Common.QueryAnalyzer;

internal static class ModelNameGenerator
{
    /// <summary>
    /// Генерирует имя модели на основе колонок
    /// </summary>
    public static string Generate(IReadOnlyList<ReturnColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        if (columns.Count == 0)
            return "void";

        if (columns.Count == 1)
            return ToPascalCase(columns[0].Name) + "Result";
        
        // Объединяем первые несколько имен колонок
        var nameParts = columns
            .Take(3)
            .Select(c => ToPascalCase(c.Name));
        
        return string.Concat(nameParts) + "Result";
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Обрабатываем snake_case
        if (name.Contains('_'))
        {
            var parts = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(part => 
                char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
        }

        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}