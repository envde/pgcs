using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение таблицы базы данных
/// </summary>
public sealed record TableDefinition: DefinitionBase
{
    /// <summary>
    /// Имя таблицы
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Список колонок таблицы
    /// </summary>
    public required IReadOnlyList<TableColumn> Columns { get; init; }
    
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
}