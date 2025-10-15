using PgCs.Common.QueryAnalyzer.Models;

namespace PgCs.QueryAnalyzer.Parsing;

using System.Text.RegularExpressions;
using PgCs.Common.QueryAnalyzer;

internal static partial class ColumnExtractor
{
    private static readonly Regex SelectRegex = GenerateSelectRegex();
    private static readonly Regex ReturningRegex = GenerateReturningRegex();

    /// <summary>
    /// Извлекает колонки из SELECT или RETURNING
    /// </summary>
    public static IReadOnlyList<ReturnColumn> Extract(string sqlQuery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlQuery);

        // Пробуем найти SELECT
        var selectMatch = SelectRegex.Match(sqlQuery);
        if (selectMatch.Success)
        {
            return ParseColumnList(selectMatch.Groups[1].Value);
        }

        // Пробуем найти RETURNING
        var returningMatch = ReturningRegex.Match(sqlQuery);
        if (returningMatch.Success)
        {
            return ParseColumnList(returningMatch.Groups[1].Value);
        }

        return [];
    }

    private static IReadOnlyList<ReturnColumn> ParseColumnList(string columnList)
    {
        var trimmed = columnList.Trim();
        
        if (trimmed == "*")
        {
            // TODO: требуется подключение к БД для получения схемы
            return [];
        }

        var columns = new List<ReturnColumn>();
        var parts = SplitColumns(trimmed);
        
        foreach (var part in parts)
        {
            var columnTrimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(columnTrimmed))
                continue;
            
            var columnName = ExtractColumnName(columnTrimmed);
            var (postgresType, csharpType) = TypeInference.InferColumnType(columnTrimmed);
            
            columns.Add(new ReturnColumn
            {
                Name = columnName,
                PostgresType = postgresType,
                CSharpType = csharpType,
                IsNullable = true // TODO: определить из схемы БД
            });
        }

        return columns;
    }

    private static List<string> SplitColumns(string columnList)
    {
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;

        foreach (var ch in columnList)
        {
            switch (ch)
            {
                case '(':
                    depth++;
                    current.Append(ch);
                    break;
                case ')':
                    depth--;
                    current.Append(ch);
                    break;
                case ',' when depth == 0:
                    parts.Add(current.ToString());
                    current.Clear();
                    break;
                default:
                    current.Append(ch);
                    break;
            }
        }

        if (current.Length > 0)
            parts.Add(current.ToString());

        return parts;
    }

    private static string ExtractColumnName(string columnExpression)
    {
        // Ищем AS alias
        var asMatch = Regex.Match(columnExpression, @"\s+AS\s+(\w+)\s*$", RegexOptions.IgnoreCase);
        if (asMatch.Success)
            return asMatch.Groups[1].Value;

        // Ищем неявный алиас (последнее слово)
        var implicitMatch = Regex.Match(columnExpression, @"(\w+)\s*$");
        if (implicitMatch.Success)
            return implicitMatch.Groups[1].Value;

        return "column";
    }

    [GeneratedRegex(@"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex GenerateSelectRegex();

    [GeneratedRegex(@"RETURNING\s+(.*?)(?:;|\s*$)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex GenerateReturningRegex();
}