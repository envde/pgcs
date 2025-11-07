using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// GROUP BY клауза в SELECT запросе
/// </summary>
public sealed record PgGroupByClause
{
    /// <summary>
    /// Список выражений для группировки
    /// </summary>
    public required IReadOnlyList<PgGroupingElement> GroupingElements { get; init; }
}

/// <summary>
/// Элемент группировки в GROUP BY
/// </summary>
public abstract record PgGroupingElement;

/// <summary>
/// Простое выражение группировки
/// Пример: GROUP BY column1, column2
/// </summary>
public sealed record PgSimpleGroupingElement : PgGroupingElement
{
    /// <summary>
    /// Выражение для группировки
    /// </summary>
    public required PgExpression Expression { get; init; }
}

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
