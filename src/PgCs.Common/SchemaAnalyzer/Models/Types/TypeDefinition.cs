using PgCs.Common.SchemaAnalyzer.Models.Types;

namespace PgCs.Common.SchemaAnalyzer.Models.Types;

/// <summary>
/// Определение пользовательского типа данных
/// </summary>
public sealed record TypeDefinition
{
    public required string Name { get; init; }
    public string? Schema { get; init; }
    public required TypeKind Kind { get; init; }
    public IReadOnlyList<string> EnumValues { get; init; } = [];
    public IReadOnlyList<CompositeTypeAttribute> CompositeAttributes { get; init; } = [];
    public DomainTypeInfo? DomainInfo { get; init; }
    public string? Comment { get; init; }
    public string? RawSql { get; init; }
}