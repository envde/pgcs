namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Определение ограничения (constraint)
/// </summary>
public sealed record ConstraintDefinition
{
    public required string Name { get; init; }
    public required string TableName { get; init; }
    public string? Schema { get; init; }
    public required ConstraintType Type { get; init; }
    public IReadOnlyList<string> Columns { get; init; } = [];
    public string? ReferencedTable { get; init; }
    public IReadOnlyList<string>? ReferencedColumns { get; init; }
    public ReferentialAction? OnDelete { get; init; }
    public ReferentialAction? OnUpdate { get; init; }
    public string? CheckExpression { get; init; }
    public bool IsDeferrable { get; init; }
    public bool IsInitiallyDeferred { get; init; }
    public string? RawSql { get; init; }
}