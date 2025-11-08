using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components.GroupBy;

/// <summary>
/// CUBE группировка
/// Пример: GROUP BY CUBE(year, month)
/// </summary>
public sealed record PgCubeGroupingElement : PgGroupingElement
{
    /// <summary>
    /// Выражения для CUBE
    /// </summary>
    public required IReadOnlyList<PgExpression> Expressions { get; init; }
}
