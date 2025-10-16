using PgCs.Common.SchemaAnalyzer.Models.Indexes;

namespace PgCs.Common.SchemaAnalyzer.Models.Indexes;

/// <summary>
/// Определение индекса
/// </summary>
public sealed record IndexDefinition
{
    public required string Name { get; init; }
    public required string TableName { get; init; }
    public string? Schema { get; init; }
    public required IReadOnlyList<string> Columns { get; init; }
    public IndexMethod Method { get; init; } = IndexMethod.BTree;
    public bool IsUnique { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsPartial { get; init; }
    public string? WhereClause { get; init; }
    public string? Comment { get; init; }
    public string? RawSql { get; init; }
}