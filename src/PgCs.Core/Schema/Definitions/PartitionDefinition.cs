using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение партиции таблицы (CREATE TABLE ... PARTITION OF)
/// Партиция представляет собой физическое разделение данных таблицы
/// </summary>
public sealed record PartitionDefinition: DefinitionBase
{
    /// <summary>
    /// Имя партиции
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Имя родительской (партиционированной) таблицы
    /// </summary>
    public required string ParentTableName { get; init; }
    
    /// <summary>
    /// Стратегия партиционирования родительской таблицы
    /// </summary>
    public required PartitionStrategy Strategy { get; init; }
    
    /// <summary>
    /// Начальное значение диапазона для RANGE партиции (FROM)
    /// Например: FROM ('2023-01-01') TO ('2023-02-01')
    /// </summary>
    public string? FromValue { get; init; }
    
    /// <summary>
    /// Конечное значение диапазона для RANGE партиции (TO)
    /// </summary>
    public string? ToValue { get; init; }
    
    /// <summary>
    /// Список значений для LIST партиции (IN)
    /// Например: IN ('active', 'pending')
    /// </summary>
    public IReadOnlyList<string>? InValues { get; init; }
    
    /// <summary>
    /// Модуль для HASH партиции (WITH MODULUS)
    /// Определяет на сколько частей разделяется хэш-пространство
    /// </summary>
    public int? Modulus { get; init; }
    
    /// <summary>
    /// Остаток для HASH партиции (WITH REMAINDER)
    /// Определяет, какая часть хэш-пространства попадает в эту партицию
    /// </summary>
    public int? Remainder { get; init; }
    
    /// <summary>
    /// Является ли это партицией по умолчанию (DEFAULT PARTITION)
    /// Принимает все строки, не подходящие под другие партиции
    /// </summary>
    public bool IsDefault { get; init; }
    
    /// <summary>
    /// Табличное пространство партиции (TABLESPACE)
    /// </summary>
    public string? Tablespace { get; init; }
}