using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение ENUM типа
/// </summary>
public sealed record EnumTypeDefinition : DefinitionBase
{
    public required IReadOnlyList<string> Values { get; init; }
}