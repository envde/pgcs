using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение перечислимого типа PostgreSQL (CREATE TYPE ... AS ENUM)
/// ENUM тип содержит фиксированный набор текстовых значений
/// </summary>
public sealed record EnumTypeDefinition : DefinitionBase
{
    /// <summary>
    /// Имя ENUM типа
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Упорядоченный список возможных значений перечисления
    /// </summary>
    public required IReadOnlyList<string> Values { get; init; }
}