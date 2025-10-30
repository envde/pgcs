using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Извлекает определения представлений (VIEW) из SQL блоков
/// <para>
/// Поддерживает:
/// - Обычные представления (CREATE VIEW)
/// - Материализованные представления (CREATE MATERIALIZED VIEW)
/// - WITH CHECK OPTION
/// - SECURITY BARRIER
/// - OR REPLACE
/// </para>
/// </summary>
public sealed partial class ViewExtractor : IViewExtractor
{
    // ============================================================================
    // Regex Patterns
    // ============================================================================

    /// <summary>
    /// Основной паттерн для извлечения CREATE VIEW
    /// Группы:
    /// - orReplace: OR REPLACE флаг
    /// - materialized: MATERIALIZED флаг
    /// - schema: имя схемы (опционально)
    /// - name: имя представления
    /// </summary>
    [GeneratedRegex(
        @"^\s*CREATE\s+(?:(OR\s+REPLACE)\s+)?(?:(MATERIALIZED)\s+)?VIEW\s+(?:(\w+)\.)?(\w+)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex CreateViewPattern();

    /// <summary>
    /// Паттерн для извлечения SELECT запроса (включая WITH CTEs)
    /// Группа: query - весь SELECT/WITH запрос до конца блока
    /// </summary>
    [GeneratedRegex(
        @"AS\s+((?:WITH\s+.+?\s+)?SELECT\s+.+?)(?:;|\s*$)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex SelectQueryPattern();

    /// <summary>
    /// Паттерн для извлечения WITH CHECK OPTION
    /// </summary>
    [GeneratedRegex(
        @"WITH\s+CHECK\s+OPTION",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex WithCheckOptionPattern();

    /// <summary>
    /// Паттерн для извлечения SECURITY BARRIER
    /// </summary>
    [GeneratedRegex(
        @"WITH\s+\(\s*security_barrier\s*=\s*(true|false|on|off)\s*\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex SecurityBarrierPattern();

    /// <summary>
    /// Паттерн для извлечения списка колонок после имени представления
    /// Группа: columns - список колонок в скобках
    /// </summary>
    [GeneratedRegex(
        @"VIEW\s+(?:\w+\.)?(\w+)\s*\(([^)]+)\)\s+AS",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColumnListPattern();

    /// <summary>
    /// Паттерн для извлечения колонок из SELECT части запроса
    /// Извлекает список колонок между SELECT и FROM
    /// </summary>
    [GeneratedRegex(
        @"SELECT\s+(.*?)\s+FROM",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex SelectColumnsPattern();

    // ============================================================================
    // Public Methods
    // ============================================================================

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        return CreateViewPattern().IsMatch(block.Content);
    }

    /// <inheritdoc />
    public ViewDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (!CanExtract(block))
        {
            return null;
        }

        var match = CreateViewPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups[3].Success ? match.Groups[3].Value : null;
        var name = match.Groups[4].Value;
        var isMaterialized = match.Groups[2].Success;

        // Извлечение SELECT запроса
        var query = ExtractSelectQuery(block.Content);
        if (string.IsNullOrWhiteSpace(query))
        {
            // Если не удалось извлечь запрос, возвращаем null
            return null;
        }

        // Извлечение списка колонок (если указан явно)
        var columnNames = ExtractColumnNames(block.Content);

        // Если колонки не указаны явно, пытаемся извлечь их из SELECT запроса
        var columns = columnNames is not null 
            ? CreateColumnsFromNames(columnNames) 
            : ExtractColumnsFromSelect(query);

        // Извлечение WITH CHECK OPTION
        var withCheckOption = WithCheckOptionPattern().IsMatch(block.Content);

        // Извлечение SECURITY BARRIER
        var isSecurityBarrier = ExtractSecurityBarrier(block.Content);

        return new ViewDefinition
        {
            Name = name,
            Schema = schema,
            Query = query.Trim(),
            IsMaterialized = isMaterialized,
            Columns = columns,
            Indexes = [],
            WithCheckOption = withCheckOption,
            IsSecurityBarrier = isSecurityBarrier,
            SqlComment = block.HeaderComment,
            RawSql = block.Content
        };
    }

    // ============================================================================
    // Private Helper Methods
    // ============================================================================

    /// <summary>
    /// Извлекает SELECT запрос из определения VIEW
    /// </summary>
    private static string? ExtractSelectQuery(string sql)
    {
        var match = SelectQueryPattern().Match(sql);
        if (!match.Success)
        {
            return null;
        }

        var query = match.Groups[1].Value;

        // Убираем WITH CHECK OPTION и другие опции после SELECT
        var withCheckIndex = query.LastIndexOf("WITH CHECK OPTION", StringComparison.OrdinalIgnoreCase);
        if (withCheckIndex > 0)
        {
            query = query[..withCheckIndex];
        }

        return query.Trim();
    }

    /// <summary>
    /// Извлекает список имен колонок, если они указаны явно
    /// Например: CREATE VIEW my_view (col1, col2, col3) AS SELECT ...
    /// </summary>
    private static IReadOnlyList<string>? ExtractColumnNames(string sql)
    {
        var match = ColumnListPattern().Match(sql);
        if (!match.Success)
        {
            return null;
        }

        var columnsStr = match.Groups[2].Value;
        var columns = columnsStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => c.Trim())
            .ToList();

        return columns.Count > 0 ? columns : null;
    }

    /// <summary>
    /// Извлекает флаг SECURITY BARRIER
    /// </summary>
    private static bool ExtractSecurityBarrier(string sql)
    {
        var match = SecurityBarrierPattern().Match(sql);
        if (!match.Success)
        {
            return false;
        }

        var value = match.Groups[1].Value.ToLowerInvariant();
        return value is "true" or "on";
    }

    /// <summary>
    /// Создает список колонок из явно указанных имен
    /// </summary>
    private static IReadOnlyList<TableColumn> CreateColumnsFromNames(IReadOnlyList<string> columnNames)
    {
        var columns = new List<TableColumn>();
        var position = 1;

        foreach (var name in columnNames)
        {
            columns.Add(new TableColumn
            {
                Name = name,
                DataType = "unknown", // Тип неизвестен для явно указанных колонок
                IsNullable = true,
                OrdinalPosition = position++
            });
        }

        return columns;
    }

    /// <summary>
    /// Извлекает колонки из SELECT запроса
    /// </summary>
    private static IReadOnlyList<TableColumn> ExtractColumnsFromSelect(string query)
    {
        var match = SelectColumnsPattern().Match(query);
        if (!match.Success)
        {
            return [];
        }

        var columnsStr = match.Groups[1].Value;

        // Обработка SELECT *
        if (columnsStr.Trim() == "*")
        {
            return [];
        }

        // Парсинг списка колонок
        var columns = new List<TableColumn>();
        var position = 1;

        // Простой парсер: разделяем по запятым (не учитывая запятые внутри функций)
        var columnParts = SplitColumns(columnsStr);

        foreach (var part in columnParts)
        {
            var columnName = ExtractColumnName(part.Trim());
            if (!string.IsNullOrWhiteSpace(columnName))
            {
                columns.Add(new TableColumn
                {
                    Name = columnName,
                    DataType = "unknown", // Тип определяется из базовой таблицы
                    IsNullable = true,
                    OrdinalPosition = position++
                });
            }
        }

        return columns;
    }

    /// <summary>
    /// Разделяет строку колонок по запятым, учитывая скобки функций
    /// </summary>
    private static List<string> SplitColumns(string columnsStr)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;

        foreach (var ch in columnsStr)
        {
            if (ch == '(')
            {
                depth++;
                current.Append(ch);
            }
            else if (ch == ')')
            {
                depth--;
                current.Append(ch);
            }
            else if (ch == ',' && depth == 0)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }

    /// <summary>
    /// Извлекает имя колонки из выражения SELECT (учитывает AS алиасы)
    /// </summary>
    private static string ExtractColumnName(string columnExpression)
    {
        // Ищем AS alias
        var asIndex = columnExpression.LastIndexOf(" AS ", StringComparison.OrdinalIgnoreCase);
        if (asIndex > 0)
        {
            var alias = columnExpression[(asIndex + 4)..].Trim();
            return CleanColumnName(alias);
        }

        // Ищем пробел перед алиасом (без AS)
        var parts = columnExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1)
        {
            // Последняя часть может быть алиасом
            var lastPart = parts[^1].Trim();
            // Проверяем, что это не ключевое слово
            if (!IsKeyword(lastPart))
            {
                return CleanColumnName(lastPart);
            }
        }

        // Иначе берем само выражение (например, просто имя колонки)
        var name = parts.Length > 0 ? parts[^1] : columnExpression;
        return CleanColumnName(name);
    }

    /// <summary>
    /// Очищает имя колонки от кавычек и других символов
    /// </summary>
    private static string CleanColumnName(string name)
    {
        return name.Trim().Trim('"', '\'', '`', '[', ']');
    }

    /// <summary>
    /// Проверяет, является ли слово SQL ключевым словом
    /// </summary>
    private static bool IsKeyword(string word)
    {
        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FROM", "WHERE", "GROUP", "HAVING", "ORDER", "LIMIT", "OFFSET",
            "UNION", "INTERSECT", "EXCEPT", "JOIN", "INNER", "LEFT", "RIGHT",
            "FULL", "CROSS", "ON", "USING", "AND", "OR", "NOT", "IN", "EXISTS"
        };

        return keywords.Contains(word);
    }
}
