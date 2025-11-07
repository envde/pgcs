using PgCs.Core.Types.Base;
using PgCs.Core.Types.Queries.Components;
using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries;

/// <summary>
/// UPDATE запрос PostgreSQL
/// Обновление существующих строк в таблице
/// </summary>
public sealed record PgUpdateQuery : PgQuery
{
    /// <summary>
    /// Целевая таблица для обновления
    /// </summary>
    public required PgTableReference Table { get; init; }

    /// <summary>
    /// SET клаузы с присваиваниями новых значений
    /// Пример: SET name = 'John', age = 30
    /// </summary>
    public required IReadOnlyList<PgSetClause> SetClauses { get; init; }

    /// <summary>
    /// FROM клауза для JOIN с другими таблицами
    /// PostgreSQL специфичное расширение
    /// Пример: UPDATE orders SET status = 'shipped' FROM shipping WHERE orders.id = shipping.order_id
    /// </summary>
    public IReadOnlyList<PgFromItem> FromClause { get; init; } = [];

    /// <summary>
    /// WHERE условие для фильтрации обновляемых строк
    /// </summary>
    public PgExpression? WhereClause { get; init; }

    /// <summary>
    /// RETURNING клауза для возврата обновлённых данных
    /// Пример: RETURNING id, updated_at
    /// </summary>
    public IReadOnlyList<PgSelectItem> ReturningClause { get; init; } = [];
}
