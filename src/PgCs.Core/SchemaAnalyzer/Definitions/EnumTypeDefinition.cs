using PgCs.Core.SchemaAnalyzer.Definitions.Base;

namespace PgCs.Core.SchemaAnalyzer.Definitions;

/// <summary>
/// Определение ENUM типа
/// </summary>
public sealed record EnumTypeDefinition : DefinitionBase
{
    public required IReadOnlyList<string> Values { get; init; }
}