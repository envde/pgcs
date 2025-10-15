using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer.Models;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает определения представлений (VIEW и MATERIALIZED VIEW)
/// </summary>
internal sealed partial class ViewExtractor : BaseExtractor<ViewDefinition>
{
    [GeneratedRegex(@"CREATE\s+(?:OR\s+REPLACE\s+)?(MATERIALIZED\s+)?VIEW\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s+AS\s+(.*)", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex ViewPatternRegex();

    protected override Regex Pattern => ViewPatternRegex();

    protected override ViewDefinition? ParseMatch(Match match, string statement)
    {
        var isMaterialized = !string.IsNullOrWhiteSpace(match.Groups[1].Value);
        var fullViewName = match.Groups[2].Value.Trim();
        var query = match.Groups[3].Value.Trim();

        var schema = ExtractSchemaName(fullViewName);
        var viewName = ExtractTableName(fullViewName);

        // Удаляем возможный trailing semicolon из query
        query = query.TrimEnd(';').Trim();

        var columns = ExtractColumnsFromQuery(query);

        return new ViewDefinition
        {
            Name = viewName,
            Schema = schema,
            Query = query,
            IsMaterialized = isMaterialized,
            Columns = columns,
            RawSql = statement
        };
    }

    private IReadOnlyList<ColumnDefinition> ExtractColumnsFromQuery(string query)
    {
        // Простой парсинг SELECT columns
        var selectMatch = Regex.Match(query, @"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!selectMatch.Success)
            return [];

        var columnsText = selectMatch.Groups[1].Value;
        var columns = new List<ColumnDefinition>();

        // Разбиваем по запятым (упрощенная версия, без учета вложенных подзапросов)
        var columnParts = SplitSelectColumns(columnsText);

        foreach (var part in columnParts)
        {
            var columnName = ExtractColumnName(part);
            if (!string.IsNullOrWhiteSpace(columnName))
            {
                columns.Add(new ColumnDefinition
                {
                    Name = columnName,
                    DataType = "unknown", // Тип неизвестен без анализа базы
                    IsNullable = true
                });
            }
        }

        return columns;
    }

    private static List<string> SplitSelectColumns(string columnsText)
    {
        var columns = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;
        var inString = false;

        foreach (var ch in columnsText)
        {
            if (ch == '\'' && depth == 0)
            {
                inString = !inString;
            }

            if (!inString)
            {
                if (ch == '(') depth++;
                if (ch == ')') depth--;
            }

            if (ch == ',' && depth == 0 && !inString)
            {
                columns.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        if (current.Length > 0)
        {
            columns.Add(current.ToString().Trim());
        }

        return columns;
    }

    private static string ExtractColumnName(string columnExpression)
    {
        // Ищем AS alias
        var asMatch = Regex.Match(columnExpression, @"\s+AS\s+([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase);
        if (asMatch.Success)
        {
            return asMatch.Groups[1].Value;
        }

        // Ищем implicit alias (column_name без AS)
        var tokens = columnExpression.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length >= 2 && !tokens[^1].Contains('('))
        {
            return tokens[^1].Trim();
        }

        // Берем имя колонки напрямую
        var simpleMatch = Regex.Match(columnExpression, @"([a-zA-Z_][a-zA-Z0-9_]*)\s*$", RegexOptions.IgnoreCase);
        if (simpleMatch.Success)
        {
            return simpleMatch.Groups[1].Value;
        }

        // Извлекаем из table.column
        var qualifiedMatch = Regex.Match(columnExpression, @"\.([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase);
        if (qualifiedMatch.Success)
        {
            return qualifiedMatch.Groups[1].Value;
        }

        return columnExpression.Trim();
    }
}