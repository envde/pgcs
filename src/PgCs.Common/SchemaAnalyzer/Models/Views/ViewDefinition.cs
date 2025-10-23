using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;

namespace PgCs.Common.SchemaAnalyzer.Models.Views;

/// <summary>
/// Определение представления (VIEW)
/// </summary>
public sealed record ViewDefinition
{
    /// <summary>
    /// Имя представления
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Схема представления (если отличается от public)
    /// </summary>
    public string? Schema { get; init; }
    
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
    public IReadOnlyList<ColumnDefinition> Columns { get; init; } = [];
    
    /// <summary>
    /// Список индексов (для материализованных представлений)
    /// </summary>
    public IReadOnlyList<IndexDefinition> Indexes { get; init; } = [];
    
    /// <summary>
    /// Комментарий к представлению (COMMENT ON VIEW)
    /// </summary>
    public string? Comment { get; init; }
    
    /// <summary>
    /// Исходный SQL код создания представления
    /// </summary>
    public string? RawSql { get; init; }
}