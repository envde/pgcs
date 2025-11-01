using System.Text.RegularExpressions;
using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Extraction.Parsing.SqlComment;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;

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
public sealed partial class ViewExtractor : IExtractor<ViewDefinition>
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
        @"AS\s+((?:WITH\s+[\s\S]+?\s+)?SELECT[\s\S]+?)(?:;|\s*$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
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
        @"SELECT\s+([\s\S]*?)\s+FROM",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex SelectColumnsPattern();

    // ============================================================================
    // Public Methods
    // ============================================================================

    /// <inheritdoc />
    public bool CanExtract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (blocks.Count == 0)
        {
            return false;
        }

        // Ищем блок с VIEW в любой позиции списка
        return blocks.Any(block => CreateViewPattern().IsMatch(block.Content));
    }

    /// <inheritdoc />
    public ExtractionResult<ViewDefinition> Extract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        var issues = new List<ValidationIssue>();

        if (blocks.Count == 0)
        {
            return ExtractionResult<ViewDefinition>.NotApplicable();
        }

        // Ищем блок с VIEW в любой позиции
        var viewBlock = blocks.FirstOrDefault(block => CreateViewPattern().IsMatch(block.Content));
        if (viewBlock is null)
        {
            return ExtractionResult<ViewDefinition>.NotApplicable();
        }

        var match = CreateViewPattern().Match(viewBlock.Content);
        if (!match.Success)
        {
            issues.Add(ValidationIssue.Error(
                ValidationIssue.ValidationDefinitionType.View,
                "VIEW_PARSE_ERROR",
                "Failed to parse CREATE VIEW statement",
                new ValidationIssue.ValidationLocation
                {
                    Segment = viewBlock.Content.Length > 100 
                        ? viewBlock.Content[..100] + "..." 
                        : viewBlock.Content,
                    Line = viewBlock.StartLine,
                    Column = 1
                }
            ));
            return ExtractionResult<ViewDefinition>.Failure(issues);
        }

        var schema = match.Groups[3].Success ? match.Groups[3].Value : null;
        var name = match.Groups[4].Value;
        var isMaterialized = match.Groups[2].Success;

        // Извлечение SELECT запроса
        var query = ExtractSelectQuery(viewBlock.Content);
        if (string.IsNullOrWhiteSpace(query))
        {
            issues.Add(ValidationIssue.Error(
                ValidationIssue.ValidationDefinitionType.View,
                "VIEW_NO_QUERY",
                $"No SELECT query found in VIEW definition for '{name}'",
                new ValidationIssue.ValidationLocation
                {
                    Segment = viewBlock.Content.Length > 100 
                        ? viewBlock.Content[..100] + "..." 
                        : viewBlock.Content,
                    Line = viewBlock.StartLine,
                    Column = 1
                }
            ));
            return ExtractionResult<ViewDefinition>.Failure(issues);
        }

        // Извлечение списка колонок (если указан явно)
        var columnNames = ExtractColumnNames(viewBlock.Content);

        // Если колонки не указаны явно, пытаемся извлечь их из SELECT запроса с использованием inline-комментариев
        var columns = columnNames is not null 
            ? CreateColumnsFromNames(columnNames) 
            : ExtractColumnsFromSelect(query, viewBlock, blocks, issues);

        // Извлечение WITH CHECK OPTION
        var withCheckOption = WithCheckOptionPattern().IsMatch(viewBlock.Content);

        // Извлечение SECURITY BARRIER
        var isSecurityBarrier = ExtractSecurityBarrier(viewBlock.Content);

        var definition = new ViewDefinition
        {
            Name = name,
            Schema = schema,
            Query = query.Trim(),
            IsMaterialized = isMaterialized,
            Columns = columns,
            Indexes = [],
            WithCheckOption = withCheckOption,
            IsSecurityBarrier = isSecurityBarrier,
            SqlComment = viewBlock.HeaderComment,
            RawSql = viewBlock.Content
        };

        return ExtractionResult<ViewDefinition>.Success(definition, issues);
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
    /// Извлекает колонки из SELECT запроса с учетом inline-комментариев и типов из таблиц
    /// </summary>
    private static IReadOnlyList<TableColumn> ExtractColumnsFromSelect(
        string query, 
        SqlBlock viewBlock, 
        IReadOnlyList<SqlBlock> allBlocks,
        List<ValidationIssue> issues)
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

        // Извлекаем таблицы из всех блоков, кроме самого VIEW
        var tableExtractor = new TableExtractor();
        var availableTables = new Dictionary<string, TableDefinition>();
        
        foreach (var block in allBlocks)
        {
            // Пропускаем сам блок с VIEW
            if (block == viewBlock)
            {
                continue;
            }
            
            if (tableExtractor.CanExtract([block]))
            {
                var result = tableExtractor.Extract([block]);
                if (result.IsSuccess && result.Definition is not null)
                {
                    availableTables[result.Definition.Name.ToLowerInvariant()] = result.Definition;
                }
            }
        }

        foreach (var part in columnParts)
        {
            var rawColumnExpression = part.Trim();
            var columnName = ExtractColumnName(rawColumnExpression);
            if (string.IsNullOrWhiteSpace(columnName))
            {
                continue;
            }

            // Пытаемся найти inline-комментарий для этой колонки
            // Нужно попробовать найти по полному выражению и по короткому имени
            var inlineComment = FindInlineCommentForColumnExpression(viewBlock, rawColumnExpression, columnName);
            var parsedComment = InlineCommentParser.Parse(inlineComment?.Comment);

            string dataType = "unknown";
            string? renameTo = null;
            string? comment = null;
            int? maxLength = null;
            int? numericPrecision = null;
            int? numericScale = null;
            bool isArray = false;

            if (parsedComment is not null)
            {
                // Если есть распарсенный комментарий с метаданными, используем его данные
                dataType = parsedComment.DataType ?? "unknown";
                renameTo = parsedComment.RenameTo;
                comment = parsedComment.Comment;
            }
            else if (inlineComment is not null)
            {
                // Если inline-комментарий есть, но не содержит метаданных,
                // используем его как обычный комментарий
                comment = inlineComment.Comment;
                // Пытаемся найти тип из таблицы
                var sourceColumn = FindColumnFromTables(columnName, availableTables);
                if (sourceColumn is not null)
                {
                    dataType = sourceColumn.DataType;
                    maxLength = sourceColumn.MaxLength;
                    numericPrecision = sourceColumn.NumericPrecision;
                    numericScale = sourceColumn.NumericScale;
                    isArray = sourceColumn.IsArray;
                }
            }
            else
            {
                // Если комментария вообще нет, пытаемся найти тип из таблицы
                var sourceColumn = FindColumnFromTables(columnName, availableTables);
                if (sourceColumn is not null)
                {
                    dataType = sourceColumn.DataType;
                    maxLength = sourceColumn.MaxLength;
                    numericPrecision = sourceColumn.NumericPrecision;
                    numericScale = sourceColumn.NumericScale;
                    isArray = sourceColumn.IsArray;
                }
            }

            columns.Add(new TableColumn
            {
                Name = columnName,
                DataType = dataType,
                ReName = renameTo,
                Comment = comment,
                IsNullable = true,
                OrdinalPosition = position++,
                MaxLength = maxLength,
                NumericPrecision = numericPrecision,
                NumericScale = numericScale,
                IsArray = isArray
            });
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
    /// Также удаляет префикс таблицы/алиаса (например, "u.id" -> "id")
    /// </summary>
    private static string CleanColumnName(string name)
    {
        // Удаляем кавычки и скобки
        var cleaned = name.Trim().Trim('"', '\'', '`', '[', ']');
        
        // Удаляем префикс таблицы/алиаса (все до последней точки)
        var dotIndex = cleaned.LastIndexOf('.');
        if (dotIndex > 0 && dotIndex < cleaned.Length - 1)
        {
            cleaned = cleaned.Substring(dotIndex + 1);
        }
        
        return cleaned;
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

    /// <summary>
    /// Ищет inline-комментарий для выражения колонки в блоке VIEW
    /// Пытается найти комментарий по полному выражению (например, "u.id") или по короткому имени ("id")
    /// </summary>
    private static InlineComment? FindInlineCommentForColumnExpression(
        SqlBlock viewBlock, 
        string rawExpression, 
        string cleanedColumnName)
    {
        if (viewBlock.InlineComments is null || viewBlock.InlineComments.Count == 0)
        {
            return null;
        }

        // Сначала пытаемся найти по полному выражению из кода (например, "u.id")
        // Извлекаем последний идентификатор из rawExpression (это то, что BlockAccumulator использует как Key)
        var key = ExtractKeyFromExpression(rawExpression);
        
        var comment = viewBlock.InlineComments
            .FirstOrDefault(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        
        if (comment is not null)
        {
            return comment;
        }

        // Если не нашли, пробуем по очищенному имени колонки
        return viewBlock.InlineComments
            .FirstOrDefault(c => c.Key.Equals(cleanedColumnName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Извлекает ключ из выражения колонки (последний идентификатор)
    /// Аналогично BlockAccumulator.ExtractKeyFromCode
    /// </summary>
    private static string ExtractKeyFromExpression(string expression)
    {
        var trimmed = expression.Trim().TrimEnd(',', ' ', '\t');
        var parts = trimmed.Split([' ', '\t', '\n', '\r', ',', '(', ')'], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[^1] : trimmed;
    }

    /// <summary>
    /// Ищет колонку в доступных таблицах
    /// </summary>
    private static TableColumn? FindColumnFromTables(
        string columnName, 
        Dictionary<string, TableDefinition> availableTables)
    {
        foreach (var table in availableTables.Values)
        {
            var column = table.Columns.FirstOrDefault(c => 
                c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            
            if (column is not null)
            {
                return column;
            }
        }

        return null;
    }
}
