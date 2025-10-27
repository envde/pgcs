namespace PgCs.Core.Definitions.Schema.Base;

/// <summary>
/// Информация о партиционировании таблицы
/// </summary>
public sealed record PartitionInfo
{
    /// <summary>
    /// Стратегия партиционирования (Range, List, Hash)
    /// </summary>
    public required PartitionStrategy Strategy { get; init; }
    
    /// <summary>
    /// Список колонок, используемых для партиционирования
    /// </summary>
    public required IReadOnlyList<string> PartitionKeys { get; init; }
    
    /// <summary>
    /// Список дочерних партиций
    /// </summary>
    public IReadOnlyList<PartitionDefinition> Partitions { get; init; } = [];
}