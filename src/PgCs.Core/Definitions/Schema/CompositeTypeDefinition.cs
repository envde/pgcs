using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.Definitions.Schema;

/// <summary>
/// Определение Composite типа
/// </summary>
public sealed record CompositeTypeDefinition : DefinitionBase
{
    public required IReadOnlyList<CompositeTypeAttribute> Attributes { get; init; }
}