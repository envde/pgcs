using PgCs.Core.SchemaAnalyzer.Definitions.Base;

namespace PgCs.Core.SchemaAnalyzer.Definitions;

/// <summary>
/// Определение Domain типа
/// </summary>
public sealed record DomainTypeDefinition : DefinitionBase
{
    public required DomainTypeInfo DomainInfo { get; init; }
}