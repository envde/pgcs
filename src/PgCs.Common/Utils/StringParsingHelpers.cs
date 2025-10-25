using System.Text;
using System.Text.RegularExpressions;

namespace PgCs.Common.Utils;

/// <summary>
/// Утилиты для парсинга и разбора строк SQL и выражений.
/// Содержит общие методы для работы со структурированным текстом.
/// </summary>
public static partial class StringParsingHelpers
{
    /// <summary>
    /// Разбивает строку по запятым с учетом вложенности скобок и кавычек.
    /// Используется для парсинга списков колонок, параметров функций и т.д.
    /// </summary>
    /// <param name="input">Строка для разбора</param>
    /// <param name="respectQuotes">Учитывать ли одинарные кавычки (строковые литералы)</param>
    /// <returns>Список разделенных частей</returns>
    public static IReadOnlyList<string> SplitByCommaRespectingDepth(string input, bool respectQuotes = true)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        var parts = new List<string>();
        var current = new StringBuilder();
        var depth = 0;
        var inQuotes = false;

        foreach (var ch in input)
        {
            // Обработка кавычек (только если respectQuotes = true)
            if (respectQuotes && ch == '\'' && depth == 0)
            {
                inQuotes = !inQuotes;
            }

            // Обработка скобок (только вне кавычек)
            if (!inQuotes)
            {
                if (ch == '(') depth++;
                if (ch == ')') depth--;
            }

            // Разделение по запятой на нулевом уровне вложенности и вне кавычек
            if (ch == ',' && depth == 0 && !inQuotes)
            {
                var trimmed = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    parts.Add(trimmed);
                }
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        // Добавляем последнюю часть
        var lastPart = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(lastPart))
        {
            parts.Add(lastPart);
        }

        return parts;
    }

    /// <summary>
    /// Извлекает имя (алиас) из SQL выражения.
    /// Поддерживает:
    /// - Явные алиасы: "column AS alias" → "alias"
    /// - Неявные алиасы: "COUNT(*) total" → "total"
    /// - Квалифицированные имена: "table.column" → "column"
    /// - Простые имена: "column_name" → "column_name"
    /// </summary>
    /// <param name="expression">SQL выражение</param>
    /// <returns>Извлеченное имя или последнее слово из выражения</returns>
    public static string ExtractNameFromExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return string.Empty;

        var trimmed = expression.Trim();

        // 1. Ищем явный AS алиас
        var asMatch = AsAliasRegex().Match(trimmed);
        if (asMatch.Success)
            return asMatch.Groups[1].Value;

        // 2. Ищем неявный алиас (последнее слово без скобок)
        var tokens = trimmed.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length >= 2 && !tokens[^1].Contains('('))
        {
            return tokens[^1].Trim();
        }

        // 3. Ищем простое имя в конце выражения
        var simpleMatch = SimpleNameRegex().Match(trimmed);
        if (simpleMatch.Success)
        {
            return simpleMatch.Groups[1].Value;
        }

        // 4. Извлекаем из table.column или schema.table.column
        var qualifiedMatch = QualifiedNameRegex().Match(trimmed);
        if (qualifiedMatch.Success)
        {
            return qualifiedMatch.Groups[1].Value;
        }

        // 5. Если ничего не подошло, берем последнее слово
        var implicitMatch = ImplicitAliasRegex().Match(trimmed);
        if (implicitMatch.Success)
            return implicitMatch.Groups[1].Value;

        return trimmed;
    }

    /// <summary>
    /// Извлекает имя схемы из полного имени объекта "schema.object"
    /// </summary>
    /// <param name="fullName">Полное имя в формате schema.object или просто object</param>
    /// <returns>Имя схемы или null если схема не указана</returns>
    public static string? ExtractSchemaName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts.Length > 1 ? parts[0] : null;
    }

    /// <summary>
    /// Извлекает имя объекта из полного имени "schema.object" или "object"
    /// </summary>
    /// <param name="fullName">Полное имя в формате schema.object или просто object</param>
    /// <returns>Имя объекта без схемы</returns>
    public static string ExtractObjectName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts[^1]; // Последний элемент
    }

    /// <summary>
    /// Обрезает строку до указанного количества символов, добавляя многоточие если необходимо.
    /// Используется для создания preview в сообщениях об ошибках.
    /// </summary>
    /// <param name="text">Исходная строка</param>
    /// <param name="maxLength">Максимальная длина (по умолчанию 100)</param>
    /// <param name="ellipsis">Строка-многоточие (по умолчанию "...")</param>
    /// <returns>Обрезанная строка с многоточием или исходная если короче maxLength</returns>
    public static string Truncate(string text, int maxLength = 100, string ellipsis = "...")
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + ellipsis;
    }

    /// <summary>
    /// Разбивает строку по одному разделителю, удаляет пустые элементы и выполняет Trim().
    /// Часто используется для парсинга списков колонок, значений enum и т.д.
    /// </summary>
    /// <param name="text">Строка для разбора</param>
    /// <param name="delimiter">Разделитель (по умолчанию запятая)</param>
    /// <returns>Массив обрезанных непустых строк</returns>
    public static string[] SplitAndTrim(string text, char delimiter = ',')
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        return text
            .Split(delimiter)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    /// <summary>
    /// Разбивает строку по нескольким разделителям, удаляет пустые элементы и выполняет Trim().
    /// Используется для парсинга токенов с множественными разделителями.
    /// </summary>
    /// <param name="text">Строка для разбора</param>
    /// <param name="delimiters">Массив разделителей</param>
    /// <returns>Массив обрезанных непустых строк</returns>
    public static string[] SplitAndTrim(string text, char[] delimiters)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        return text
            .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    #region Regex patterns

    /// <summary>
    /// Regex для поиска явного AS алиаса: "AS alias"
    /// </summary>
    [GeneratedRegex(@"\s+AS\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex AsAliasRegex();

    /// <summary>
    /// Regex для поиска простого имени в конце строки: "column_name"
    /// </summary>
    [GeneratedRegex(@"([a-zA-Z_][a-zA-Z0-9_]*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex SimpleNameRegex();

    /// <summary>
    /// Regex для извлечения имени из квалифицированного выражения: "table.column"
    /// </summary>
    [GeneratedRegex(@"\.([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex QualifiedNameRegex();

    /// <summary>
    /// Regex для поиска неявного алиаса (последнее слово): "word"
    /// </summary>
    [GeneratedRegex(@"(\w+)\s*$", RegexOptions.Compiled)]
    private static partial Regex ImplicitAliasRegex();

    #endregion
}
