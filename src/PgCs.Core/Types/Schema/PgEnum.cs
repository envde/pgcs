using PgCs.Core.Parser.Metadata;

namespace PgCs.Core.Types.Schema;

/// <summary>
/// PostgreSQL enumeration type (CREATE TYPE ... AS ENUM).
/// Contains a fixed set of ordered text values.
/// </summary>
public sealed record PgEnum : PgObject
{
    /// <summary>
    /// Ordered list of enumeration values.
    /// Order matters in PostgreSQL ENUMs (used for comparisons).
    /// </summary>
    public required IReadOnlyList<string> Values { get; init; }
}
