using PgCs.Core.Parser.Metadata;
using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Schema;

/// <summary>
/// PostgreSQL trigger definition (CREATE TRIGGER).
/// Automatically executes a function when specific events occur on a table or view.
/// </summary>
public sealed record PgTrigger : PgObject
{
    /// <summary>
    /// Table or view name that the trigger is attached to.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Trigger firing timing (Before, After, InsteadOf).
    /// </summary>
    public required PgTriggerTiming Timing { get; init; }

    /// <summary>
    /// Events that activate the trigger (Insert, Update, Delete, Truncate).
    /// </summary>
    public required IReadOnlyList<PgTriggerEvent> Events { get; init; }

    /// <summary>
    /// Function name to execute when trigger fires.
    /// </summary>
    public required string FunctionName { get; init; }

    /// <summary>
    /// Trigger level (Row or Statement). Default is Row.
    /// </summary>
    public PgTriggerLevel Level { get; init; } = PgTriggerLevel.Row;

    /// <summary>
    /// Optional WHEN condition to limit trigger execution.
    /// Example: "NEW.balance > 0"
    /// </summary>
    public string? WhenCondition { get; init; }

    /// <summary>
    /// Columns for UPDATE OF clause (only for Update triggers).
    /// Trigger fires only when these columns are updated.
    /// </summary>
    public IReadOnlyList<string>? UpdateColumns { get; init; }

    /// <summary>
    /// Arguments passed to the trigger function.
    /// </summary>
    public IReadOnlyList<string>? Arguments { get; init; }

    /// <summary>
    /// Constraint trigger (FOR EACH ROW ... DEFERRABLE).
    /// </summary>
    public bool IsConstraint { get; init; }

    /// <summary>
    /// Can be deferred to transaction end (DEFERRABLE).
    /// </summary>
    public bool IsDeferrable { get; init; }

    /// <summary>
    /// Initially deferred by default (INITIALLY DEFERRED).
    /// </summary>
    public bool IsInitiallyDeferred { get; init; }
}
