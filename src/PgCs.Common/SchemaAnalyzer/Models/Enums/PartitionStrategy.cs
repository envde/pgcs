namespace PgCs.Common.SchemaAnalyzer.Models.Enums;

/// <summary>
/// Стратегия партиционирования
/// </summary>
public enum PartitionStrategy
{
    Range,
    List,
    Hash
}