using PgCs.Core.Parser.Metadata;
using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Schema;

/// <summary>
/// PostgreSQL table constraint (PRIMARY KEY, FOREIGN KEY, UNIQUE, CHECK, EXCLUDE).
/// Enforces data integrity rules.
/// </summary>
public sealed record PgConstraint : PgObject
{
    /// <summary>
    /// Table name that this constraint applies to.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Constraint type.
    /// </summary>
    public required PgConstraintType Type { get; init; }

    /// <summary>
    /// Columns involved in the constraint.
    /// </summary>
    public IReadOnlyList<string> Columns { get; init; } = [];

    /// <summary>
    /// Referenced table for ForeignKey constraints.
    /// </summary>
    public string? ReferencedTable { get; init; }

    /// <summary>
    /// Referenced columns for ForeignKey constraints.
    /// </summary>
    public IReadOnlyList<string>? ReferencedColumns { get; init; }

    /// <summary>
    /// ON DELETE action for ForeignKey.
    /// </summary>
    public PgReferentialAction? OnDelete { get; init; }

    /// <summary>
    /// ON UPDATE action for ForeignKey.
    /// </summary>
    public PgReferentialAction? OnUpdate { get; init; }

    /// <summary>
    /// CHECK constraint expression.
    /// Example: "price > 0"
    /// </summary>
    public string? CheckExpression { get; init; }

    /// <summary>
    /// EXCLUDE constraint using clause.
    /// Example: "USING gist (daterange WITH &&)"
    /// </summary>
    public string? ExcludeUsing { get; init; }

    /// <summary>
    /// Can be deferred (DEFERRABLE).
    /// </summary>
    public bool IsDeferrable { get; init; }

    /// <summary>
    /// Initially deferred by default (INITIALLY DEFERRED).
    /// </summary>
    public bool IsInitiallyDeferred { get; init; }

    /// <summary>
    /// NOT VALID constraint (not checked for existing rows).
    /// </summary>
    public bool IsNotValid { get; init; }
}
