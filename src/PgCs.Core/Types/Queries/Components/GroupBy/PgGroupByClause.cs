namespace PgCs.Core.Types.Queries.Components.GroupBy;

/// <summary>
/// GROUP BY клауза в SELECT запросе
/// </summary>
public sealed record PgGroupByClause
{
    /// <summary>
    /// Список элементов группировки
    /// </summary>
    public required IReadOnlyList<PgGroupingElement> GroupingElements { get; init; }
}
