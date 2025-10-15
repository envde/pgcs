namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Стратегия партиционирования
/// </summary>
public enum PartitionStrategy
{
    Range,
    List,
    Hash
}