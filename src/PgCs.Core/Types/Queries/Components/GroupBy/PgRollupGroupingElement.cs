using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components.GroupBy;

/// <summary>
/// ROLLUP группировка
/// Пример: GROUP BY ROLLUP(year, month, day)
/// </summary>
public sealed record PgRollupGroupingElement : PgGroupingElement
{
    /// <summary>
    /// Выражения для ROLLUP
    /// </summary>
    public required IReadOnlyList<PgExpression> Expressions { get; init; }
}
