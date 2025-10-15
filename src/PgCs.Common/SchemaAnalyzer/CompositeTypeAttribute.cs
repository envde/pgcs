namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Атрибут композитного типа
/// </summary>
public sealed record CompositeTypeAttribute
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
    public int? MaxLength { get; init; }
}