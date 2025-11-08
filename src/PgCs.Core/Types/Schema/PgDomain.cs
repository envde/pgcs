using PgCs.Core.Parser.Metadata;

namespace PgCs.Core.Types.Schema;

/// <summary>
/// PostgreSQL domain type (CREATE DOMAIN).
/// A base type with additional constraints.
/// </summary>
public sealed record PgDomain : PgObject
{
    /// <summary>
    /// Base PostgreSQL data type (integer, varchar, etc.).
    /// </summary>
    public required string BaseType { get; init; }

    /// <summary>
    /// Default value expression (DEFAULT clause).
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Disallows NULL values (NOT NULL constraint).
    /// </summary>
    public bool IsNotNull { get; init; }

    /// <summary>
    /// CHECK constraints applied to the domain.
    /// Example: "VALUE > 0"
    /// </summary>
    public IReadOnlyList<string> CheckConstraints { get; init; } = [];

    /// <summary>
    /// Maximum length for string domains (VARCHAR(n)).
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Numeric precision for numeric domains (NUMERIC(precision, scale)).
    /// </summary>
    public int? NumericPrecision { get; init; }

    /// <summary>
    /// Numeric scale for numeric domains (NUMERIC(precision, scale)).
    /// </summary>
    public int? NumericScale { get; init; }

    /// <summary>
    /// Collation for string domains.
    /// </summary>
    public string? Collation { get; init; }
}
