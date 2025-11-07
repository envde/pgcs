using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Элемент сортировки в ORDER BY клаузе
/// Пример: column_name DESC NULLS LAST
/// </summary>
public sealed record PgOrderByItem
{
    /// <summary>
    /// Выражение для сортировки
    /// </summary>
    public required PgExpression Expression { get; init; }

    /// <summary>
    /// Направление сортировки (ASC/DESC)
    /// </summary>
    public PgOrderDirection Direction { get; init; } = PgOrderDirection.Ascending;

    /// <summary>
    /// Позиция NULL значений (NULLS FIRST/LAST)
    /// </summary>
    public PgNullsOrdering? NullsOrdering { get; init; }

    /// <summary>
    /// USING оператор для кастомной сортировки
    /// Пример: ORDER BY name USING &lt;
    /// </summary>
    public string? UsingOperator { get; init; }
}
