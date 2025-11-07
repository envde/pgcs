using PgCs.Core.Parsing.SqlMetadata;
using PgCs.Core.Types.Base;

namespace PgCs.Core.Types;

/// <summary>
/// PostgreSQL index definition (CREATE INDEX).
/// Accelerates data retrieval from tables.
/// </summary>
public sealed record PgIndex : PgObject
{
    /// <summary>
    /// Table name that this index is built on.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Index columns or expressions.
    /// Can be column names or expressions like "LOWER(email)".
    /// </summary>
    public required IReadOnlyList<string> Columns { get; init; }

    /// <summary>
    /// Index method: BTree (default), Hash, GiST, GIN, SpGist, Brin, Bloom.
    /// </summary>
    public PgIndexMethod Method { get; init; } = PgIndexMethod.BTree;

    /// <summary>
    /// Unique index (CREATE UNIQUE INDEX).
    /// Ensures uniqueness of indexed values.
    /// </summary>
    public bool IsUnique { get; init; }

    /// <summary>
    /// Primary key index (created via PRIMARY KEY constraint).
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    /// Partial index with WHERE clause.
    /// Indexes only rows satisfying the condition.
    /// </summary>
    public bool IsPartial { get; init; }

    /// <summary>
    /// WHERE clause for partial index.
    /// Example: "WHERE status = 'active'"
    /// </summary>
    public string? WhereClause { get; init; }

    /// <summary>
    /// INCLUDE columns (stored but not indexed).
    /// Useful for covering indexes.
    /// </summary>
    public IReadOnlyList<string>? IncludeColumns { get; init; }

    /// <summary>
    /// Tablespace where index is stored.
    /// </summary>
    public string? Tablespace { get; init; }

    /// <summary>
    /// Storage parameters (e.g., fillfactor).
    /// </summary>
    public IReadOnlyDictionary<string, string>? StorageParameters { get; init; }

    /// <summary>
    /// Concurrent index build (CREATE INDEX CONCURRENTLY).
    /// </summary>
    public bool IsConcurrent { get; init; }
}
