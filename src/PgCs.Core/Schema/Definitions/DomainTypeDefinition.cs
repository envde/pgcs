using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение Domain типа
/// </summary>
public sealed record DomainTypeDefinition : DefinitionBase
{
    public required DomainTypeInfo DomainInfo { get; init; }
}