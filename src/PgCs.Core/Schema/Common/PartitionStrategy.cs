namespace PgCs.Core.Schema.Common;

/// <summary>
/// Стратегия партиционирования таблицы для разделения данных на части
/// </summary>
public enum PartitionStrategy
{
    /// <summary>
    /// RANGE - партиционирование по диапазонам значений (например, по датам)
    /// </summary>
    Range,

    /// <summary>
    /// LIST - партиционирование по списку конкретных значений
    /// </summary>
    List,

    /// <summary>
    /// HASH - партиционирование по хэшу значения для равномерного распределения
    /// </summary>
    Hash
}