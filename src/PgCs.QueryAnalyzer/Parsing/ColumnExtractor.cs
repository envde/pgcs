using System.Text;
using System.Text.RegularExpressions;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.Services;
using PgCs.Common.Utils;

namespace PgCs.QueryAnalyzer.Parsing;

/// <summary>
/// Извлекатель информации о колонках из SQL запросов (SELECT и RETURNING)
/// </summary>
internal static partial class ColumnExtractor
{
    private static readonly Regex SelectRegex = GenerateSelectRegex();
    private static readonly Regex ReturningRegex = GenerateReturningRegex();
    private static readonly ITypeMapper TypeMapper = new PostgreSqlTypeMapper();

    /// <summary>
    /// Извлекает список колонок из SELECT или RETURNING части SQL запроса
    /// </summary>
    /// <param name="sqlQuery">SQL запрос для анализа</param>
    /// <param name="schemaMetadata">Метаданные схемы для определения типов (опционально)</param>
    /// <returns>Список колонок с информацией о типах</returns>
    public static IReadOnlyList<ReturnColumn> Extract(string sqlQuery, SchemaMetadata? schemaMetadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlQuery);

        // Извлекаем ВСЕ таблицы и алиасы из запроса
        var tableAliases = ExtractTableAliases(sqlQuery);

        // Пробуем найти SELECT
        var selectMatch = SelectRegex.Match(sqlQuery);
        if (selectMatch.Success)
        {
            return ParseColumnList(selectMatch.Groups[1].Value, tableAliases, schemaMetadata);
        }

        // Пробуем найти RETURNING
        var returningMatch = ReturningRegex.Match(sqlQuery);
        if (returningMatch.Success)
        {
            return ParseColumnList(returningMatch.Groups[1].Value, tableAliases, schemaMetadata);
        }

        return [];
    }

    /// <summary>
    /// Парсит список колонок из строки вида "col1, col2 AS alias, func(col3)"
    /// </summary>
    private static IReadOnlyList<ReturnColumn> ParseColumnList(
        string columnList, 
        Dictionary<string, string> tableAliases,
        SchemaMetadata? schemaMetadata = null)
    {
        var trimmed = columnList.Trim();
        
        if (trimmed == "*")
        {
            // Если есть схема и только одна таблица, возвращаем все колонки из схемы
            if (schemaMetadata != null && tableAliases.Count == 1)
            {
                var tableName = tableAliases.Values.First();
                var table = schemaMetadata.Tables.FirstOrDefault(t => 
                    t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                    
                if (table != null)
                {
                    return table.Columns.Select(c => new ReturnColumn
                    {
                        Name = c.Name,
                        PostgresType = c.DataType,
                        CSharpType = TypeMapper.MapType(c.DataType, c.IsNullable, c.IsArray),
                        IsNullable = c.IsNullable
                    }).ToList();
                }
            }
            
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
            
            // Пытаемся определить тип из схемы
            var (postgresType, csharpType, isNullable) = TryResolveTypeFromSchema(
                columnName, tableAliases, columnTrimmed, schemaMetadata);
            
            // Если не нашли в схеме, используем type inference
            if (postgresType == null)
            {
                var (infPostgresType, infCsharpType) = TypeInference.InferColumnType(columnTrimmed);
                postgresType = infPostgresType;
                csharpType = infCsharpType;
            }
            
            columns.Add(new ReturnColumn
            {
                Name = columnName,
                PostgresType = postgresType ?? "text",
                CSharpType = csharpType ?? "string",
                IsNullable = isNullable
            });
        }

        return columns;
    }

    /// <summary>
    /// Извлекает имя таблицы из FROM clause
    /// </summary>
    private static string? ExtractTableName(string sqlQuery)
    {
        // Ищем FROM table_name (простой случай)
        var fromMatch = Regex.Match(sqlQuery, @"\bFROM\s+(\w+)", RegexOptions.IgnoreCase);
        if (fromMatch.Success)
        {
            return fromMatch.Groups[1].Value;
        }

        // Ищем INSERT INTO table_name или UPDATE table_name
        var insertMatch = Regex.Match(sqlQuery, @"\b(?:INSERT\s+INTO|UPDATE)\s+(\w+)", RegexOptions.IgnoreCase);
        if (insertMatch.Success)
        {
            return insertMatch.Groups[1].Value;
        }

        return null;
    }

    /// <summary>
    /// Извлекает все таблицы и их алиасы из запроса (FROM + JOINs)
    /// Возвращает словарь: alias -> table_name
    /// </summary>
    private static Dictionary<string, string> ExtractTableAliases(string sqlQuery)
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // FROM table_name [AS] alias
        var fromMatches = Regex.Matches(sqlQuery, 
            @"\bFROM\s+(\w+)(?:\s+(?:AS\s+)?(\w+))?", 
            RegexOptions.IgnoreCase);
        
        foreach (Match match in fromMatches)
        {
            var tableName = match.Groups[1].Value;
            var alias = match.Groups[2].Success ? match.Groups[2].Value : tableName;
            aliases[alias] = tableName;
        }

        // JOIN table_name [AS] alias
        var joinMatches = Regex.Matches(sqlQuery,
            @"\b(?:INNER|LEFT|RIGHT|FULL|CROSS)?\s*JOIN\s+(\w+)(?:\s+(?:AS\s+)?(\w+))?",
            RegexOptions.IgnoreCase);

        foreach (Match match in joinMatches)
        {
            var tableName = match.Groups[1].Value;
            var alias = match.Groups[2].Success ? match.Groups[2].Value : tableName;
            aliases[alias] = tableName;
        }

        return aliases;
    }

    /// <summary>
    /// Пытается определить тип колонки из схемы базы данных
    /// </summary>
    private static (string? PostgresType, string? CSharpType, bool IsNullable) TryResolveTypeFromSchema(
        string columnName,
        Dictionary<string, string> tableAliases,
        string columnExpression,
        SchemaMetadata? schemaMetadata)
    {
        if (schemaMetadata == null || tableAliases.Count == 0)
            return (null, null, true);

        string? targetTableName = null;
        string? cleanColumnName = columnName;

        // Если колонка с префиксом (например, "o.id"), разбираем
        if (columnName.Contains('.'))
        {
            var parts = columnName.Split('.');
            var alias = parts[0];
            cleanColumnName = parts[1];

            // Ищем таблицу по алиасу
            if (tableAliases.TryGetValue(alias, out var foundTable))
            {
                targetTableName = foundTable;
            }
        }
        else if (tableAliases.Count == 1)
        {
            // Если только одна таблица, используем её
            targetTableName = tableAliases.Values.First();
        }
        else
        {
            // Несколько таблиц без префикса - пробуем найти колонку во всех
            foreach (var tableName in tableAliases.Values.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var table = schemaMetadata.Tables.FirstOrDefault(t => 
                    t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                
                if (table != null)
                {
                    var column = table.Columns.FirstOrDefault(c => 
                        c.Name.Equals(cleanColumnName, StringComparison.OrdinalIgnoreCase));
                    
                    if (column != null)
                    {
                        // Используем общий TypeMapper для конвертации типов
                        var isArray = column.IsArray;
                        var csharpType = TypeMapper.MapType(column.DataType, column.IsNullable, isArray);
                        return (column.DataType, csharpType, column.IsNullable);
                    }
                }
            }
            
            return (null, null, true);
        }

        // Ищем таблицу в схеме
        if (string.IsNullOrEmpty(targetTableName))
            return (null, null, true);

        var targetTable = schemaMetadata.Tables.FirstOrDefault(t => 
            t.Name.Equals(targetTableName, StringComparison.OrdinalIgnoreCase));

        if (targetTable == null)
            return (null, null, true);

        var targetColumn = targetTable.Columns.FirstOrDefault(c => 
            c.Name.Equals(cleanColumnName, StringComparison.OrdinalIgnoreCase));

        if (targetColumn == null)
            return (null, null, true);

        // Используем общий TypeMapper для конвертации типов
        var targetIsArray = targetColumn.IsArray;
        var targetCsharpType = TypeMapper.MapType(targetColumn.DataType, targetColumn.IsNullable, targetIsArray);
        return (targetColumn.DataType, targetCsharpType, targetColumn.IsNullable);
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