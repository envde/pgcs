using PgCs.Core.SchemaAnalyzer.Definitions.Base;

namespace PgCs.Core.SchemaAnalyzer.Definitions;

/// <summary>
/// Определение таблицы базы данных
/// </summary>
public sealed record TableDefinition
{
    /// <summary>
    /// Имя таблицы
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Схема таблицы
    /// </summary>
    public required string Schema { get; init; }
    
    /// <summary>
    /// Список колонок таблицы
    /// </summary>
    public required IReadOnlyList<ColumnDefinition> Columns { get; init; }
    
    /// <summary>
    /// Список ограничений целостности (constraints)
    /// </summary>
    public IReadOnlyList<ConstraintDefinition> Constraints { get; init; } = [];
    
    /// <summary>
    /// Список индексов таблицы
    /// </summary>
    public IReadOnlyList<IndexDefinition> Indexes { get; init; } = [];
    
    /// <summary>
    /// Является ли таблица партиционированной
    /// </summary>
    public bool IsPartitioned { get; init; }
    
    /// <summary>
    /// Информация о партиционировании (если таблица партиционирована)
    /// </summary>
    public PartitionInfo? PartitionInfo { get; init; }
    
    /// <summary>
    /// Комментарий к таблице (COMMENT ON TABLE)
    /// </summary>
    public string? Comment { get; init; }
    
    /// <summary>
    /// Исходный SQL код создания таблицы
    /// </summary>
    public required string RawSql { get; init; }
}