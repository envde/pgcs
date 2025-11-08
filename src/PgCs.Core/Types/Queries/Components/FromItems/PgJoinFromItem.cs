namespace PgCs.Core.Types.Queries.Components.FromItems;

/// <summary>
/// JOIN выражение в FROM клаузе
/// </summary>
public sealed record PgJoinFromItem : PgFromItem
{
    /// <summary>
    /// Левая часть JOIN
    /// </summary>
    public required PgFromItem LeftItem { get; init; }

    /// <summary>
    /// JOIN клауза
    /// </summary>
    public required PgJoinClause JoinClause { get; init; }
}
