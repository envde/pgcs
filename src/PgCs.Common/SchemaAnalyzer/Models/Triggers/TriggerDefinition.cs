using PgCs.Common.SchemaAnalyzer.Models.Triggers;

namespace PgCs.Common.SchemaAnalyzer.Models.Triggers;

/// <summary>
/// Определение триггера
/// </summary>
public sealed record TriggerDefinition
{
    public required string Name { get; init; }
    public required string TableName { get; init; }
    public string? Schema { get; init; }
    public required TriggerTiming Timing { get; init; }
    public required IReadOnlyList<TriggerEvent> Events { get; init; }
    public required string FunctionName { get; init; }
    public TriggerLevel Level { get; init; } = TriggerLevel.Row;
    public string? WhenCondition { get; init; }
    public IReadOnlyList<string>? UpdateColumns { get; init; }
    public string? Comment { get; init; }
    public string? RawSql { get; init; }
}