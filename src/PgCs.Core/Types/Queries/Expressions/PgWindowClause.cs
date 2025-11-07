namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// WINDOW клауза для оконных функций в PostgreSQL
/// Пример: OVER (PARTITION BY category ORDER BY price ROWS BETWEEN 1 PRECEDING AND 1 FOLLOWING)
/// </summary>
public sealed record PgWindowClause
{
    /// <summary>
    /// Имя именованного окна (если используется)
    /// Пример: SELECT ... WINDOW w AS (PARTITION BY ...) ... OVER w
    /// </summary>
    public string? WindowName { get; init; }

    /// <summary>
    /// PARTITION BY клауза - группировка для окна
    /// Пример: PARTITION BY category, region
    /// </summary>
    public IReadOnlyList<PgExpression> PartitionBy { get; init; } = [];

    /// <summary>
    /// ORDER BY клауза - сортировка внутри окна
    /// </summary>
    public IReadOnlyList<PgOrderByItem> OrderBy { get; init; } = [];

    /// <summary>
    /// Фрейм окна: ROWS, RANGE, или GROUPS
    /// Пример: ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    /// </summary>
    public PgWindowFrame? Frame { get; init; }
}
