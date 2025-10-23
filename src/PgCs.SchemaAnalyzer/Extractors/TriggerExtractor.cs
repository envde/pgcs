using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает определения триггеров
/// </summary>
internal sealed partial class TriggerExtractor : BaseExtractor<TriggerDefinition>
{
    [GeneratedRegex(@"CREATE\s+(?:OR\s+REPLACE\s+)?TRIGGER\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+(BEFORE|AFTER|INSTEAD\s+OF)\s+(INSERT|UPDATE|DELETE|TRUNCATE)(?:\s+OR\s+(INSERT|UPDATE|DELETE|TRUNCATE))*\s+ON\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s+FOR\s+EACH\s+(ROW|STATEMENT)\s+(?:WHEN\s*\((.*?)\)\s+)?EXECUTE\s+(?:FUNCTION|PROCEDURE)\s+([a-zA-Z_][a-zA-Z0-9_]*)", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex TriggerPatternRegex();

    protected override Regex Pattern => TriggerPatternRegex();

    protected override TriggerDefinition ParseMatch(Match match, string statement)
    {
        var triggerName = match.Groups[1].Value.Trim();
        var timing = match.Groups[2].Value.Trim();
        var fullTableName = match.Groups[5].Value.Trim();
        var level = match.Groups[6].Value.Trim();
        var whenCondition = match.Groups[7].Value.Trim();
        var functionName = match.Groups[8].Value.Trim();

        var schema = ExtractSchemaName(fullTableName);
        var tableName = ExtractTableName(fullTableName);

        var events = ParseTriggerEvents(statement);
        var triggerTiming = ParseTriggerTiming(timing);
        var triggerLevel = level.Equals("ROW", StringComparison.OrdinalIgnoreCase) 
            ? TriggerLevel.Row 
            : TriggerLevel.Statement;

        // Извлекаем UPDATE columns если есть
        var updateColumns = ExtractUpdateColumns(statement);

        return new TriggerDefinition
        {
            Name = triggerName,
            TableName = tableName,
            Schema = schema,
            Timing = triggerTiming,
            Events = events,
            FunctionName = functionName,
            Level = triggerLevel,
            WhenCondition = !string.IsNullOrWhiteSpace(whenCondition) ? whenCondition : null,
            UpdateColumns = updateColumns,
            RawSql = statement
        };
    }

    private IReadOnlyList<TriggerEvent> ParseTriggerEvents(string statement)
    {
        var events = new List<TriggerEvent>();
        var eventPattern = @"\b(INSERT|UPDATE|DELETE|TRUNCATE)\b";
        var matches = Regex.Matches(statement, eventPattern, RegexOptions.IgnoreCase);

        var seenEvents = new HashSet<TriggerEvent>();

        foreach (Match match in matches)
        {
            var eventType = match.Value.ToUpperInvariant() switch
            {
                "INSERT" => TriggerEvent.Insert,
                "UPDATE" => TriggerEvent.Update,
                "DELETE" => TriggerEvent.Delete,
                "TRUNCATE" => TriggerEvent.Truncate,
                _ => (TriggerEvent?)null
            };

            if (eventType.HasValue && seenEvents.Add(eventType.Value))
            {
                events.Add(eventType.Value);
            }
        }

        return events;
    }

    private static TriggerTiming ParseTriggerTiming(string timing)
    {
        var normalized = timing.Replace(" ", "").ToUpperInvariant();
        return normalized switch
        {
            "BEFORE" => TriggerTiming.Before,
            "AFTER" => TriggerTiming.After,
            "INSTEADOF" => TriggerTiming.InsteadOf,
            _ => TriggerTiming.Before
        };
    }

    private IReadOnlyList<string>? ExtractUpdateColumns(string statement)
    {
        var match = Regex.Match(statement, 
            @"UPDATE\s+OF\s+([a-zA-Z_][a-zA-Z0-9_]*(?:\s*,\s*[a-zA-Z_][a-zA-Z0-9_]*)*)", 
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return null;

        return match.Groups[1].Value
            .Split(',')
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToArray();
    }
}