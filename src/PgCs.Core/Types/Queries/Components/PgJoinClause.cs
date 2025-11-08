using PgCs.Core.Types.Base;
using PgCs.Core.Types.Queries.Components.FromItems;
using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// JOIN клауза в SQL запросе
/// Поддерживает все типы JOIN: INNER, LEFT, RIGHT, FULL, CROSS
/// </summary>
public sealed record PgJoinClause
{
    /// <summary>
    /// Тип JOIN операции
    /// </summary>
    public required PgJoinType JoinType { get; init; }

    /// <summary>
    /// Правая таблица в JOIN
    /// </summary>
    public required PgFromItem RightTable { get; init; }

    /// <summary>
    /// ON условие для JOIN (используется в INNER, LEFT, RIGHT, FULL)
    /// Пример: ON users.id = orders.user_id
    /// </summary>
    public PgExpression? OnCondition { get; init; }

    /// <summary>
    /// USING колонки (альтернатива ON для равенства)
    /// Пример: USING (user_id, order_id)
    /// </summary>
    public IReadOnlyList<string> UsingColumns { get; init; } = [];

    /// <summary>
    /// Является ли это NATURAL JOIN
    /// </summary>
    public bool IsNatural { get; init; }

    /// <summary>
    /// Является ли это LATERAL JOIN
    /// Пример: LEFT JOIN LATERAL get_user_orders(u.id) AS orders ON true
    /// </summary>
    public bool IsLateral { get; init; }
}
