using System.Text.RegularExpressions;
using PgCs.Common.CodeGeneration;
using PgCs.Common.Utils;
using PgCs.SchemaAnalyzer.Utils;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Базовый класс для всех экстракторов
/// </summary>
internal abstract class BaseExtractor<T>
{
    protected abstract Regex Pattern { get; }
    
    /// <summary>
    /// Warnings и errors собранные во время parsing
    /// </summary>
    public List<ValidationIssue> Issues { get; } = new();
    
    /// <summary>
    /// Возвращает true если этот extractor должен обрабатывать данный CREATE statement
    /// По умолчанию проверяет только Pattern, но может быть переопределен
    /// </summary>
    protected virtual bool ShouldProcess(string statement)
    {
        return Pattern.IsMatch(statement);
    }

    public virtual IReadOnlyList<T> Extract(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            return [];

        Issues.Clear(); // Очищаем перед новым extraction
        var statements = SqlStatementSplitter.Split(sqlScript);
        var results = new List<T>();

        foreach (var statement in statements)
        {
            // Пропускаем пустые statements
            if (string.IsNullOrWhiteSpace(statement))
                continue;

            try
            {
                var match = Pattern.Match(statement);
                if (match.Success)
                {
                    var item = ParseMatch(match, statement);
                    if (item is not null)
                    {
                        results.Add(item);
                    }
                    else
                    {
                        // ParseMatch вернул null - возможно statement был пропущен
                        // Логируем warning если statement выглядит как CREATE но был пропущен
                        if (statement.TrimStart().StartsWith("CREATE", StringComparison.OrdinalIgnoreCase))
                        {
                            var preview = StringParsingHelpers.Truncate(statement);
                            
                            Issues.Add(ValidationIssue.Warning(
                                "PARSE_SKIPPED",
                                "Statement matched pattern but was skipped during parsing",
                                preview,
                                new Dictionary<string, string>
                                {
                                    ["StatementPreview"] = preview
                                }));
                        }
                    }
                }
                else
                {
                    // Pattern не совпал - проверим, это CREATE statement который НЕ для этого extractor?
                    var trimmed = statement.TrimStart();
                    if (trimmed.StartsWith("CREATE", StringComparison.OrdinalIgnoreCase))
                    {
                        // Проверяем, относится ли этот CREATE statement к другому extractor
                        var isForDifferentExtractor = 
                            (trimmed.Contains("CREATE TABLE", StringComparison.OrdinalIgnoreCase) && GetType().Name != "TableExtractor") ||
                            (trimmed.Contains("CREATE INDEX", StringComparison.OrdinalIgnoreCase) && GetType().Name != "IndexExtractor") ||
                            (trimmed.Contains("CREATE UNIQUE INDEX", StringComparison.OrdinalIgnoreCase) && GetType().Name != "IndexExtractor") ||
                            (trimmed.Contains("CREATE TYPE", StringComparison.OrdinalIgnoreCase) && GetType().Name != "TypeExtractor") ||
                            (trimmed.Contains("CREATE DOMAIN", StringComparison.OrdinalIgnoreCase) && GetType().Name != "TypeExtractor") ||
                            (trimmed.Contains("CREATE FUNCTION", StringComparison.OrdinalIgnoreCase) && GetType().Name != "FunctionExtractor") ||
                            (trimmed.Contains("CREATE OR REPLACE FUNCTION", StringComparison.OrdinalIgnoreCase) && GetType().Name != "FunctionExtractor") ||
                            (trimmed.Contains("CREATE TRIGGER", StringComparison.OrdinalIgnoreCase) && GetType().Name != "TriggerExtractor") ||
                            (trimmed.Contains("CREATE VIEW", StringComparison.OrdinalIgnoreCase) && GetType().Name != "ViewExtractor") ||
                            (trimmed.Contains("CREATE MATERIALIZED VIEW", StringComparison.OrdinalIgnoreCase) && GetType().Name != "ViewExtractor") ||
                            trimmed.Contains("PARTITION OF", StringComparison.OrdinalIgnoreCase);
                        
                        // Только логируем warning если это statement должен обрабатываться ЭТИМ extractor
                        if (!isForDifferentExtractor)
                        {
                            var preview = StringParsingHelpers.Truncate(statement);
                            
                            Issues.Add(ValidationIssue.Warning(
                                "PARSE_NO_MATCH",
                                "CREATE statement did not match expected pattern - possibly malformed SQL",
                                preview,
                                new Dictionary<string, string>
                                {
                                    ["StatementPreview"] = preview,
                                    ["ExtractorType"] = GetType().Name
                                }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку парсинга
                var preview = StringParsingHelpers.Truncate(statement);
                
                Issues.Add(ValidationIssue.Error(
                    "PARSE_ERROR",
                    $"Failed to parse statement: {ex.Message}",
                    preview,
                    new Dictionary<string, string>
                    {
                        ["Exception"] = ex.GetType().Name,
                        ["StatementPreview"] = preview
                    }));
            }
        }

        return results;
    }

    protected abstract T? ParseMatch(Match match, string statement);

    protected static string? ExtractSchemaName(string fullName)
    {
        var schema = StringParsingHelpers.ExtractSchemaName(fullName);
        // Убираем двойные кавычки если есть
        return schema?.Trim('"');
    }

    protected static string ExtractTableName(string fullName)
    {
        var objectName = StringParsingHelpers.ExtractObjectName(fullName);
        // Убираем двойные кавычки если есть
        return objectName.Trim('"');
    }
}