using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.Definitions.Schema;

/// <summary>
/// Определение ENUM типа
/// </summary>
public sealed record EnumTypeDefinition : DefinitionBase
{
    public required IReadOnlyList<string> Values { get; init; }
}