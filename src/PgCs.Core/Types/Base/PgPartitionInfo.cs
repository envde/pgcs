namespace PgCs.Core.Types.Base;

/// <summary>
/// Информация о партиционировании таблицы PostgreSQL
/// </summary>
public sealed record PgPartitionInfo
{
    /// <summary>
    /// Стратегия партиционирования: RANGE, LIST или HASH
    /// </summary>
    public required PgPartitionStrategy Strategy { get; init; }

    /// <summary>
    /// Список колонок или выражений, используемых для партиционирования
    /// </summary>
    /// <example>
    /// ["created_at"] для RANGE по дате
    /// ["region_id"] для LIST по регионам  
    /// ["user_id"] для HASH по пользователям
    /// </example>
    public required IReadOnlyList<string> PartitionKeys { get; init; }

    /// <summary>
    /// Выражение партиционирования для сложных случаев
    /// </summary>
    /// <example>
    /// "EXTRACT(YEAR FROM created_at)"
    /// "date_trunc('month', timestamp_column)"
    /// </example>
    public string? PartitionExpression { get; init; }
}
