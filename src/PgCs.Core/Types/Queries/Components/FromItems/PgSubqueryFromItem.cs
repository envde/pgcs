namespace PgCs.Core.Types.Queries.Components.FromItems;

/// <summary>
/// Подзапрос в FROM клаузе (derived table)
/// Пример: FROM (SELECT * FROM users WHERE active) AS u
/// </summary>
public sealed record PgSubqueryFromItem : PgFromItem
{
    /// <summary>
    /// Подзапрос
    /// </summary>
    public required PgSelectQuery Query { get; init; }

    /// <summary>
    /// Является ли это LATERAL подзапрос
    /// </summary>
    public bool IsLateral { get; init; }
}
