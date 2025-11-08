using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components.GroupBy;

/// <summary>
/// GROUPING SETS группировка
/// Пример: GROUP BY GROUPING SETS ((year), (month), ())
/// </summary>
public sealed record PgGroupingSetsElement : PgGroupingElement
{
    /// <summary>
    /// Наборы группировки
    /// </summary>
    public required IReadOnlyList<IReadOnlyList<PgExpression>> GroupingSets { get; init; }
}
