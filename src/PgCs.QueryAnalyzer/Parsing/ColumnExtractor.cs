using System.Text;
using System.Text.RegularExpressions;
using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.QueryAnalyzer.Parsing;

/// <summary>
/// Извлекатель информации о колонках из SQL запросов (SELECT и RETURNING)
/// </summary>
internal static partial class ColumnExtractor
{
    private static readonly Regex SelectRegex = GenerateSelectRegex();
    private static readonly Regex ReturningRegex = GenerateReturningRegex();

    /// <summary>
    /// Извлекает список колонок из SELECT или RETURNING части SQL запроса
    /// </summary>
    /// <param name="sqlQuery">SQL запрос для анализа</param>
    /// <returns>Список колонок с информацией о типах</returns>
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

    /// <summary>
    /// Парсит список колонок из строки вида "col1, col2 AS alias, func(col3)"
    /// </summary>
    private static IReadOnlyList<ReturnColumn> ParseColumnList(string columnList)
    {
        var trimmed = columnList.Trim();
        
        if (trimmed == "*")
        {
            // NOTE: Static analysis limitation - requires database connection to resolve SELECT * columns
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
                // NOTE: Static analysis limitation - nullability requires database schema metadata
                IsNullable = true
            });
        }

        return columns;
    }

    /// <summary>
    /// Разбивает список колонок по запятым, учитывая вложенность скобок
    /// </summary>
    private static List<string> SplitColumns(string columnList)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
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

    /// <summary>
    /// Извлекает имя колонки из выражения (учитывает AS алиасы)
    /// </summary>
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

    /// <summary>
    /// Regex для поиска SELECT части запроса
    /// </summary>
    [GeneratedRegex(@"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex GenerateSelectRegex();

    /// <summary>
    /// Regex для поиска RETURNING части запроса
    /// </summary>
    [GeneratedRegex(@"RETURNING\s+(.*?)(?:;|\s*$)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex GenerateReturningRegex();
}