using PgCs.Core.Types.Base;
using PgCs.Core.Types.Queries.Components;
using PgCs.Core.Types.Queries.Components.FromItems;
using PgCs.Core.Types.Queries.Components.GroupBy;
using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries;

/// <summary>
/// SELECT запрос PostgreSQL
/// Самый сложный и универсальный тип запроса
/// </summary>
public sealed record PgSelectQuery : PgQuery
{
    /// <summary>
    /// Квантификатор множества (ALL, DISTINCT, DISTINCT ON)
    /// </summary>
    public PgSetQuantifier SetQuantifier { get; init; } = PgSetQuantifier.All;

    /// <summary>
    /// Выражения для DISTINCT ON (если используется)
    /// Пример: SELECT DISTINCT ON (category) *
    /// </summary>
    public IReadOnlyList<PgExpression> DistinctOnExpressions { get; init; } = [];

    /// <summary>
    /// Список выбираемых колонок/выражений в SELECT клаузе
    /// </summary>
    public required IReadOnlyList<PgSelectItem> SelectList { get; init; }

    /// <summary>
    /// FROM клауза с источниками данных
    /// </summary>
    public IReadOnlyList<PgFromItem> FromClause { get; init; } = [];

    /// <summary>
    /// WHERE условие фильтрации
    /// </summary>
    public PgExpression? WhereClause { get; init; }

    /// <summary>
    /// GROUP BY клауза группировки
    /// </summary>
    public PgGroupByClause? GroupByClause { get; init; }

    /// <summary>
    /// HAVING условие для агрегатных функций
    /// </summary>
    public PgExpression? HavingClause { get; init; }

    /// <summary>
    /// WINDOW определения именованных окон
    /// Пример: WINDOW w AS (PARTITION BY category ORDER BY price)
    /// </summary>
    public IReadOnlyList<PgNamedWindowClause> WindowClauses { get; init; } = [];

    /// <summary>
    /// ORDER BY клауза сортировки результата
    /// Для операций над множествами (UNION/INTERSECT/EXCEPT) используйте PgSetOperationQuery
    /// </summary>
    public IReadOnlyList<PgOrderByItem> OrderByClause { get; init; } = [];

    /// <summary>
    /// LIMIT количество возвращаемых строк
    /// </summary>
    public PgExpression? LimitClause { get; init; }

    /// <summary>
    /// OFFSET смещение для пропуска строк
    /// </summary>
    public PgExpression? OffsetClause { get; init; }

    /// <summary>
    /// FETCH FIRST/NEXT клауза (SQL стандартная альтернатива LIMIT)
    /// Пример: FETCH FIRST 10 ROWS ONLY
    /// </summary>
    public PgFetchClause? FetchClause { get; init; }

    /// <summary>
    /// FOR UPDATE/SHARE клаузы блокировки
    /// </summary>
    public IReadOnlyList<PgLockingClause> LockingClauses { get; init; } = [];
}

/// <summary>
/// Именованное окно в WINDOW клаузе
/// </summary>
public sealed record PgNamedWindowClause
{
    /// <summary>
    /// Имя окна
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Определение окна
    /// </summary>
    public required PgWindowClause WindowDefinition { get; init; }
}

/// <summary>
/// FETCH клауза (SQL стандарт)
/// </summary>
public sealed record PgFetchClause
{
    /// <summary>
    /// Количество строк для выборки
    /// </summary>
    public required PgExpression Count { get; init; }

    /// <summary>
    /// FIRST или NEXT
    /// </summary>
    public bool IsFirst { get; init; } = true;

    /// <summary>
    /// WITH TIES - включить строки с одинаковым значением ORDER BY
    /// </summary>
    public bool WithTies { get; init; }
}
