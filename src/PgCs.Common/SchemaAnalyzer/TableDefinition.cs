namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Определение таблицы базы данных
/// </summary>
public sealed record TableDefinition
{
    public required string Name { get; init; }
    public string? Schema { get; init; }
    public required IReadOnlyList<ColumnDefinition> Columns { get; init; }
    public IReadOnlyList<ConstraintDefinition> Constraints { get; init; } = [];
    public IReadOnlyList<IndexDefinition> Indexes { get; init; } = [];
    public bool IsPartitioned { get; init; }
    public PartitionInfo? PartitionInfo { get; init; }
    public string? Comment { get; init; }
    public string? RawSql { get; init; }
}