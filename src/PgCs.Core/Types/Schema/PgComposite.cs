using PgCs.Core.Parser.Metadata;
using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Schema;

/// <summary>
/// PostgreSQL composite type (CREATE TYPE ... AS).
/// Represents a structure with named fields (like a row type).
/// </summary>
public sealed record PgComposite : PgObject
{
    /// <summary>
    /// Attributes (fields) of the composite type.
    /// </summary>
    public required IReadOnlyList<PgCompositeTypeAttribute> Attributes { get; init; }
}
