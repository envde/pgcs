namespace PgCs.Common.SchemaAnalyzer.Models.Tables;

/// <summary>
/// Стратегия партиционирования
/// </summary>
public enum PartitionStrategy
{
    Range,
    List,
    Hash
}