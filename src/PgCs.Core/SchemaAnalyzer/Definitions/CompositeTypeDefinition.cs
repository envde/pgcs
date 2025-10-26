using PgCs.Core.SchemaAnalyzer.Definitions.Base;

namespace PgCs.Core.SchemaAnalyzer.Definitions;

/// <summary>
/// Определение Composite типа
/// </summary>
public sealed record CompositeTypeDefinition : DefinitionBase
{
    public required IReadOnlyList<CompositeTypeAttribute> Attributes { get; init; }
}