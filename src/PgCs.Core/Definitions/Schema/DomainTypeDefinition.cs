using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.Definitions.Schema;

/// <summary>
/// Определение Domain типа
/// </summary>
public sealed record DomainTypeDefinition : DefinitionBase
{
    public required DomainTypeInfo DomainInfo { get; init; }
}