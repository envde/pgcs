namespace PgCs.Core.Schema.Common;

/// <summary>
/// Информация о партиционировании таблицы PostgreSQL
/// Описывает стратегию и ключи разделения данных
/// </summary>
public sealed record PartitionInfo
{
    /// <summary>
    /// Стратегия партиционирования: RANGE (по диапазонам), LIST (по списку), HASH (по хэшу)
    /// </summary>
    public required PartitionStrategy Strategy { get; init; }
    
    /// <summary>
    /// Список колонок или выражений, используемых для партиционирования
    /// Например: ["created_at"] для RANGE, ["region_id"] для LIST
    /// </summary>
    public required IReadOnlyList<string> PartitionKeys { get; init; }
    
    /// <summary>
    /// Выражение партиционирования для сложных случаев
    /// Например: "EXTRACT(YEAR FROM created_at)"
    /// </summary>
    public string? PartitionExpression { get; init; }
}