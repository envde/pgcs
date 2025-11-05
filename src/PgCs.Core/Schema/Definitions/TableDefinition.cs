using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение таблицы базы данных (CREATE TABLE)
/// </summary>
public sealed record TableDefinition: DefinitionBase
{
    /// <summary>
    /// Имя таблицы
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Список колонок таблицы с их типами и атрибутами
    /// </summary>
    public required IReadOnlyList<TableColumn> Columns { get; init; }
    
    /// <summary>
    /// Является ли таблица партиционированной (PARTITION BY)
    /// Партиционированные таблицы физически разделены на части
    /// </summary>
    public bool IsPartitioned { get; init; }
    
    /// <summary>
    /// Информация о стратегии партиционирования (если таблица партиционирована)
    /// </summary>
    public PartitionInfo? PartitionInfo { get; init; }
    
    /// <summary>
    /// Является ли таблица партицией другой таблицы (PARTITION OF)
    /// </summary>
    public bool IsPartition { get; init; }
    
    /// <summary>
    /// Имя родительской таблицы (если это партиция)
    /// </summary>
    public string? ParentTableName { get; init; }
    
    /// <summary>
    /// Является ли таблица временной (TEMPORARY/TEMP)
    /// Временные таблицы удаляются после завершения сессии
    /// </summary>
    public bool IsTemporary { get; init; }
    
    /// <summary>
    /// Является ли таблица незалоггированной (UNLOGGED)
    /// Незалоггированные таблицы быстрее, но не crash-safe
    /// </summary>
    public bool IsUnlogged { get; init; }
    
    /// <summary>
    /// Табличное пространство, в котором хранится таблица (TABLESPACE)
    /// </summary>
    public string? Tablespace { get; init; }
    
    /// <summary>
    /// Параметры хранения таблицы (storage parameters)
    /// Например: fillfactor, autovacuum_enabled
    /// </summary>
    public IReadOnlyDictionary<string, string>? StorageParameters { get; init; }
    
    /// <summary>
    /// Список таблиц, от которых наследуется данная таблица (INHERITS)
    /// PostgreSQL поддерживает наследование таблиц
    /// </summary>
    public IReadOnlyList<string>? InheritsFrom { get; init; }
}