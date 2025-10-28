using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение представления (VIEW)
/// </summary>
public sealed record ViewDefinition: DefinitionBase
{
    /// <summary>
    /// Имя представления
    /// </summary>
    public required string Name { get; init; }
    
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