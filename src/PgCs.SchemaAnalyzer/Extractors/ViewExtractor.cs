using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.Utils;

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

    protected override ViewDefinition ParseMatch(Match match, string statement)
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
        // Используем общий метод с учетом кавычек и скобок
        var parts = StringParsingHelpers.SplitByCommaRespectingDepth(columnsText, respectQuotes: true);
        return parts.ToList();
    }

    private static string ExtractColumnName(string columnExpression)
    {
        return StringParsingHelpers.ExtractNameFromExpression(columnExpression);
    }
}