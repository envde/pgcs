using PgCs.Core.Extraction;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор для извлечения определений триггеров из SQL-скриптов
/// </summary>
public sealed class TriggerExtractor : IExtractor<TriggerDefinition>
{
    /// <inheritdoc />
    public bool CanExtract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        
        if (blocks.Count == 0)
        {
            return false;
        }

        var block = blocks[0];
        var content = block.Content;
        
        // Быстрая проверка на наличие CREATE и TRIGGER
        return content.Contains("CREATE", StringComparison.OrdinalIgnoreCase) &&
               content.Contains("TRIGGER", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ExtractionResult<TriggerDefinition> Extract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        var issues = new List<ValidationIssue>();

        if (blocks.Count == 0)
        {
            return ExtractionResult<TriggerDefinition>.NotApplicable();
        }

        var block = blocks[0];
        
        if (!CanExtract(blocks))
        {
            return ExtractionResult<TriggerDefinition>.NotApplicable();
        }

        var content = block.Content.Trim();
        
        // Парсинг триггера вручную
        try
        {
            // Пример: CREATE TRIGGER trigger_name BEFORE INSERT OR UPDATE ON table_name FOR EACH ROW EXECUTE FUNCTION func_name();
            
            // Находим CREATE TRIGGER
            var createIndex = content.IndexOf("CREATE", StringComparison.OrdinalIgnoreCase);
            if (createIndex < 0)
            {
                issues.Add(ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Trigger,
                    "TRIGGER_PARSE_ERROR",
                    "Failed to parse CREATE TRIGGER statement",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = content.Length > 100 ? content[..100] + "..." : content,
                        Line = block.StartLine,
                        Column = 1
                    }
                ));
                return ExtractionResult<TriggerDefinition>.Failure(issues);
            }

            var afterCreate = content[createIndex..];
            var triggerIndex = afterCreate.IndexOf("TRIGGER", StringComparison.OrdinalIgnoreCase);
            if (triggerIndex < 0)
            {
                issues.Add(ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Trigger,
                    "TRIGGER_PARSE_ERROR",
                    "TRIGGER keyword not found after CREATE",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = content.Length > 100 ? content[..100] + "..." : content,
                        Line = block.StartLine,
                        Column = 1
                    }
                ));
                return ExtractionResult<TriggerDefinition>.Failure(issues);
            }

            var afterTrigger = afterCreate[(triggerIndex + 7)..].TrimStart();
            
            // Извлекаем имя триггера
            var tokens = afterTrigger.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1)
            {
                issues.Add(ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Trigger,
                    "TRIGGER_PARSE_ERROR",
                    "Trigger name not found",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = content.Length > 100 ? content[..100] + "..." : content,
                        Line = block.StartLine,
                        Column = 1
                    }
                ));
                return ExtractionResult<TriggerDefinition>.Failure(issues);
            }

            var triggerName = tokens[0];
            
            // Извлекаем схему если есть
            string? schema = null;
            if (triggerName.Contains('.'))
            {
                var parts = triggerName.Split('.');
                schema = parts[0];
                triggerName = parts[1];
            }
            
            // Находим timing (BEFORE/AFTER/INSTEAD OF)
            var timing = ExtractTiming(content);
            if (timing is null)
            {
                issues.Add(ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Trigger,
                    "TRIGGER_NO_TIMING",
                    $"Trigger timing (BEFORE/AFTER/INSTEAD OF) not found for trigger '{triggerName}'",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = content.Length > 100 ? content[..100] + "..." : content,
                        Line = block.StartLine,
                        Column = 1
                    }
                ));
                return ExtractionResult<TriggerDefinition>.Failure(issues);
            }
            
            // Находим события (INSERT/UPDATE/DELETE/TRUNCATE)
            var events = ExtractEvents(content);
            if (events.Count == 0)
            {
                issues.Add(ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Trigger,
                    "TRIGGER_NO_EVENTS",
                    $"No trigger events (INSERT/UPDATE/DELETE/TRUNCATE) found for trigger '{triggerName}'",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = content.Length > 100 ? content[..100] + "..." : content,
                        Line = block.StartLine,
                        Column = 1
                    }
                ));
                return ExtractionResult<TriggerDefinition>.Failure(issues);
            }
            
            // Находим таблицу (ON table_name)
            var tableName = ExtractTableName(content);
            if (string.IsNullOrWhiteSpace(tableName))
            {
                issues.Add(ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Trigger,
                    "TRIGGER_NO_TABLE",
                    $"Table name not found for trigger '{triggerName}'",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = content.Length > 100 ? content[..100] + "..." : content,
                        Line = block.StartLine,
                        Column = 1
                    }
                ));
                return ExtractionResult<TriggerDefinition>.Failure(issues);
            }
            
            // Извлекаем схему таблицы если есть
            string? tableSchema = null;
            if (tableName.Contains('.'))
            {
                var parts = tableName.Split('.');
                tableSchema = parts[0];
                tableName = parts[1];
            }
            
            // Находим уровень (FOR EACH ROW/STATEMENT)
            var level = ExtractLevel(content);
            
            // Находим функцию (EXECUTE FUNCTION/PROCEDURE func_name)
            var functionName = ExtractFunctionName(content);
            if (string.IsNullOrWhiteSpace(functionName))
            {
                issues.Add(ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Trigger,
                    "TRIGGER_NO_FUNCTION",
                    $"Function name not found for trigger '{triggerName}'",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = content.Length > 100 ? content[..100] + "..." : content,
                        Line = block.StartLine,
                        Column = 1
                    }
                ));
                return ExtractionResult<TriggerDefinition>.Failure(issues);
            }
            
            // Извлекаем WHEN условие если есть
            var whenCondition = ExtractWhenCondition(content);
            
            // Извлекаем UPDATE OF колонки если есть
            var updateColumns = ExtractUpdateColumns(content);

            var definition = new TriggerDefinition
            {
                Name = triggerName,
                TableName = tableName,
                Timing = timing.Value,
                Events = events,
                FunctionName = functionName,
                Level = level,
                WhenCondition = whenCondition,
                UpdateColumns = updateColumns,
                Schema = tableSchema ?? schema,
                SqlComment = block.HeaderComment,
                RawSql = block.RawContent
            };

            return ExtractionResult<TriggerDefinition>.Success(definition, issues);
        }
        catch (Exception ex)
        {
            issues.Add(ValidationIssue.Error(
                ValidationIssue.ValidationDefinitionType.Trigger,
                "TRIGGER_PARSE_ERROR",
                $"Unexpected error parsing trigger: {ex.Message}",
                new ValidationIssue.ValidationLocation
                {
                    Segment = content.Length > 100 ? content[..100] + "..." : content,
                    Line = block.StartLine,
                    Column = 1
                }
            ));
            return ExtractionResult<TriggerDefinition>.Failure(issues);
        }
    }

    /// <summary>
    /// Извлекает timing триггера (BEFORE/AFTER/INSTEAD OF)
    /// </summary>
    private static TriggerTiming? ExtractTiming(string content)
    {
        var upper = content.ToUpperInvariant();
        
        if (upper.Contains("INSTEAD OF"))
        {
            return TriggerTiming.InsteadOf;
        }
        
        if (upper.Contains("BEFORE"))
        {
            return TriggerTiming.Before;
        }
        
        if (upper.Contains("AFTER"))
        {
            return TriggerTiming.After;
        }
        
        return null;
    }

    /// <summary>
    /// Извлекает события триггера (INSERT/UPDATE/DELETE/TRUNCATE)
    /// </summary>
    private static IReadOnlyList<TriggerEvent> ExtractEvents(string content)
    {
        var events = new List<TriggerEvent>();
        var upper = content.ToUpperInvariant();
        
        if (upper.Contains("INSERT"))
        {
            events.Add(TriggerEvent.Insert);
        }
        
        if (upper.Contains("UPDATE"))
        {
            events.Add(TriggerEvent.Update);
        }
        
        if (upper.Contains("DELETE"))
        {
            events.Add(TriggerEvent.Delete);
        }
        
        if (upper.Contains("TRUNCATE"))
        {
            events.Add(TriggerEvent.Truncate);
        }
        
        return events;
    }

    /// <summary>
    /// Извлекает имя таблицы (ON table_name)
    /// </summary>
    private static string? ExtractTableName(string content)
    {
        var upper = content.ToUpperInvariant();
        var onIndex = upper.IndexOf(" ON ", StringComparison.Ordinal);
        if (onIndex < 0)
        {
            return null;
        }
        
        var afterOn = content[(onIndex + 4)..].TrimStart();
        var tokens = afterOn.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        return tokens.Length > 0 ? tokens[0] : null;
    }

    /// <summary>
    /// Извлекает уровень триггера (FOR EACH ROW/STATEMENT)
    /// </summary>
    private static TriggerLevel ExtractLevel(string content)
    {
        var upper = content.ToUpperInvariant();
        
        if (upper.Contains("FOR EACH ROW"))
        {
            return TriggerLevel.Row;
        }
        
        if (upper.Contains("FOR EACH STATEMENT"))
        {
            return TriggerLevel.Statement;
        }
        
        // По умолчанию ROW
        return TriggerLevel.Row;
    }

    /// <summary>
    /// Извлекает имя функции (EXECUTE FUNCTION/PROCEDURE func_name)
    /// </summary>
    private static string? ExtractFunctionName(string content)
    {
        var upper = content.ToUpperInvariant();
        
        // EXECUTE FUNCTION (новый синтаксис)
        var execFuncIndex = upper.IndexOf("EXECUTE FUNCTION", StringComparison.Ordinal);
        if (execFuncIndex >= 0)
        {
            var afterExec = content[(execFuncIndex + 16)..].TrimStart();
            return ExtractFunctionNameFromText(afterExec);
        }
        
        // EXECUTE PROCEDURE (старый синтаксис)
        var execProcIndex = upper.IndexOf("EXECUTE PROCEDURE", StringComparison.Ordinal);
        if (execProcIndex >= 0)
        {
            var afterExec = content[(execProcIndex + 17)..].TrimStart();
            return ExtractFunctionNameFromText(afterExec);
        }
        
        return null;
    }

    /// <summary>
    /// Извлекает имя функции из текста, удаляя скобки и параметры
    /// </summary>
    private static string ExtractFunctionNameFromText(string text)
    {
        // Находим первый токен до скобки или точки с запятой
        var parenIndex = text.IndexOf('(');
        var semicolonIndex = text.IndexOf(';');
        
        var endIndex = -1;
        if (parenIndex >= 0 && semicolonIndex >= 0)
        {
            endIndex = Math.Min(parenIndex, semicolonIndex);
        }
        else if (parenIndex >= 0)
        {
            endIndex = parenIndex;
        }
        else if (semicolonIndex >= 0)
        {
            endIndex = semicolonIndex;
        }
        
        var functionText = endIndex >= 0 ? text[..endIndex] : text;
        var tokens = functionText.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        return tokens.Length > 0 ? tokens[0] : string.Empty;
    }

    /// <summary>
    /// Извлекает WHEN условие если есть
    /// </summary>
    private static string? ExtractWhenCondition(string content)
    {
        var upper = content.ToUpperInvariant();
        var whenIndex = upper.IndexOf(" WHEN ", StringComparison.Ordinal);
        if (whenIndex < 0)
        {
            return null;
        }
        
        var afterWhen = content[(whenIndex + 6)..].TrimStart();
        
        // Условие заканчивается на EXECUTE
        var executeIndex = afterWhen.IndexOf("EXECUTE", StringComparison.OrdinalIgnoreCase);
        if (executeIndex < 0)
        {
            return null;
        }
        
        var condition = afterWhen[..executeIndex].Trim();
        
        // Удаляем скобки если есть
        if (condition.StartsWith('(') && condition.EndsWith(')'))
        {
            condition = condition[1..^1].Trim();
        }
        
        return string.IsNullOrWhiteSpace(condition) ? null : condition;
    }

    /// <summary>
    /// Извлекает колонки для UPDATE OF если есть
    /// </summary>
    private static IReadOnlyList<string>? ExtractUpdateColumns(string content)
    {
        var upper = content.ToUpperInvariant();
        var updateOfIndex = upper.IndexOf("UPDATE OF", StringComparison.Ordinal);
        if (updateOfIndex < 0)
        {
            return null;
        }
        
        var afterUpdateOf = content[(updateOfIndex + 9)..].TrimStart();
        
        // Колонки заканчиваются на ON
        var onIndex = afterUpdateOf.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
        if (onIndex < 0)
        {
            return null;
        }
        
        var columnsText = afterUpdateOf[..onIndex].Trim();
        
        // Разделяем по запятым
        var columns = columnsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();
        
        return columns.Count > 0 ? columns : null;
    }
}
