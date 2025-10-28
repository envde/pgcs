using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение Composite типа
/// </summary>
public sealed record CompositeTypeDefinition : DefinitionBase
{
    public required string Name { get; init; }
    public required IReadOnlyList<CompositeTypeAttribute> Attributes { get; init; }
}