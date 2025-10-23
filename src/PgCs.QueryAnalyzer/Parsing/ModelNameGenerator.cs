using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.QueryAnalyzer.Parsing;

/// <summary>
/// Генератор имен для C# моделей результатов запросов на основе колонок
/// </summary>
internal static class ModelNameGenerator
{
    /// <summary>
    /// Генерирует имя класса модели на основе имен возвращаемых колонок
    /// </summary>
    /// <param name="columns">Список колонок результата запроса</param>
    /// <returns>Имя модели в PascalCase (например, "UserEmailResult")</returns>
    public static string Generate(IReadOnlyList<ReturnColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        if (columns.Count == 0)
            return "void";

        // Для одной колонки используем ее имя + Result
        if (columns.Count == 1)
            return ToPascalCase(columns[0].Name) + "Result";
        
        // Объединяем первые несколько имен колонок для составного имени
        var nameParts = columns
            .Take(3)
            .Select(c => ToPascalCase(c.Name));
        
        return string.Concat(nameParts) + "Result";
    }

    /// <summary>
    /// Преобразует имя в PascalCase (snake_case → PascalCase)
    /// </summary>
    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Обрабатываем snake_case (user_name → UserName)
        if (name.Contains('_'))
        {
            var parts = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(part => 
                char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
        }

        // Простое имя - делаем первую букву заглавной
        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}