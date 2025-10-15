namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Определение колонки таблицы
/// </summary>
public sealed record ColumnDefinition
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
    public bool IsNullable { get; init; }
    public bool IsPrimaryKey { get; init; }
    public bool IsUnique { get; init; }
    public bool IsArray { get; init; }
    public string? DefaultValue { get; init; }
    public int? MaxLength { get; init; }
    public int? NumericPrecision { get; init; }
    public int? NumericScale { get; init; }
    public string? Comment { get; init; }
    public IReadOnlyList<string> CheckConstraints { get; init; } = [];
}