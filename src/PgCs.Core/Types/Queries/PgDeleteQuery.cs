using PgCs.Core.Types.Base;
using PgCs.Core.Types.Queries.Components;
using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries;

/// <summary>
/// DELETE запрос PostgreSQL
/// Удаление строк из таблицы
/// </summary>
public sealed record PgDeleteQuery : PgQuery
{
    /// <summary>
    /// Целевая таблица для удаления
    /// </summary>
    public required PgTableReference Table { get; init; }

    /// <summary>
    /// USING клауза для JOIN с другими таблицами
    /// PostgreSQL специфичное расширение
    /// Пример: DELETE FROM orders USING users WHERE orders.user_id = users.id AND users.deleted
    /// </summary>
    public IReadOnlyList<PgFromItem> UsingClause { get; init; } = [];

    /// <summary>
    /// WHERE условие для фильтрации удаляемых строк
    /// </summary>
    public PgExpression? WhereClause { get; init; }

    /// <summary>
    /// RETURNING клауза для возврата удалённых данных
    /// Пример: RETURNING id, name
    /// </summary>
    public IReadOnlyList<PgSelectItem> ReturningClause { get; init; } = [];
}
