namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Определение партиции
/// </summary>
public sealed record PartitionDefinition
{
    public required string Name { get; init; }
    public string? FromValue { get; init; }
    public string? ToValue { get; init; }
    public IReadOnlyList<string>? InValues { get; init; }
    public int? Modulus { get; init; }
    public int? Remainder { get; init; }
}