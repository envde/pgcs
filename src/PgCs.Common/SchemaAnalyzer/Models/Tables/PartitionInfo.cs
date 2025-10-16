using PgCs.Common.SchemaAnalyzer.Models.Tables;

namespace PgCs.Common.SchemaAnalyzer.Models.Tables;

/// <summary>
/// Информация о партиционировании таблицы
/// </summary>
public sealed record PartitionInfo
{
    public required PartitionStrategy Strategy { get; init; }
    public required IReadOnlyList<string> PartitionKeys { get; init; }
    public IReadOnlyList<PartitionDefinition> Partitions { get; init; } = [];
}