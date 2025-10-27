using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.Definitions.Schema;

/// <summary>
/// Определение представления (VIEW)
/// </summary>
public sealed record ViewDefinition: DefinitionBase
{
    
    /// <summary>
    /// SQL запрос, определяющий представление
    /// </summary>
    public required string Query { get; init; }
    
    /// <summary>
    /// Является ли представление материализованным (MATERIALIZED VIEW)
    /// </summary>
    public bool IsMaterialized { get; init; }
    
    /// <summary>
    /// Список колонок представления
    /// </summary>
    public IReadOnlyList<TableColumn> Columns { get; init; } = [];
    
    /// <summary>
    /// Список индексов (для материализованных представлений)
    /// </summary>
    public IReadOnlyList<IndexDefinition> Indexes { get; init; } = [];
}