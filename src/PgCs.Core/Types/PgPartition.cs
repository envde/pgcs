using PgCs.Core.Parsing.SqlMetadata;
using PgCs.Core.Types.Base;

namespace PgCs.Core.Types;

/// <summary>
/// PostgreSQL table partition (CREATE TABLE ... PARTITION OF).
/// Represents physical data partitioning.
/// </summary>
public sealed record PgPartition : PgObject
{
    /// <summary>
    /// Parent (partitioned) table name.
    /// </summary>
    public required string ParentTableName { get; init; }

    /// <summary>
    /// Partitioning strategy of the parent table.
    /// </summary>
    public required PgPartitionStrategy Strategy { get; init; }

    /// <summary>
    /// Start value for RANGE partition (FROM).
    /// Example: FROM ('2023-01-01') TO ('2023-02-01')
    /// </summary>
    public string? FromValue { get; init; }

    /// <summary>
    /// End value for RANGE partition (TO).
    /// </summary>
    public string? ToValue { get; init; }

    /// <summary>
    /// Values for LIST partition (IN).
    /// Example: IN ('active', 'pending')
    /// </summary>
    public IReadOnlyList<string>? InValues { get; init; }

    /// <summary>
    /// Modulus for HASH partition (WITH MODULUS).
    /// Defines how many parts the hash space is divided into.
    /// </summary>
    public int? Modulus { get; init; }

    /// <summary>
    /// Remainder for HASH partition (WITH REMAINDER).
    /// Defines which part of hash space goes to this partition.
    /// </summary>
    public int? Remainder { get; init; }

    /// <summary>
    /// Default partition (accepts rows not matching other partitions).
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Tablespace for partition storage.
    /// </summary>
    public string? Tablespace { get; init; }
}
