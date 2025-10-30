using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение композитного типа PostgreSQL (CREATE TYPE ... AS)
/// Композитный тип представляет собой структуру с именованными полями
/// </summary>
public sealed record CompositeTypeDefinition : DefinitionBase
{
    /// <summary>
    /// Имя композитного типа
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Список атрибутов (полей) композитного типа
    /// </summary>
    public required IReadOnlyList<CompositeTypeAttribute> Attributes { get; init; }
}