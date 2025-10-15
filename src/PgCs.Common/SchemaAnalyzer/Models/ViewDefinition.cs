namespace PgCs.Common.SchemaAnalyzer.Models;

/// <summary>
/// Определение представления (VIEW)
/// </summary>
public sealed record ViewDefinition
{
    public required string Name { get; init; }
    public string? Schema { get; init; }
    public required string Query { get; init; }
    public bool IsMaterialized { get; init; }
    public IReadOnlyList<ColumnDefinition> Columns { get; init; } = [];
    public IReadOnlyList<IndexDefinition> Indexes { get; init; } = [];
    public string? Comment { get; init; }
    public string? RawSql { get; init; }
}