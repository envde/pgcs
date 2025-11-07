using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Вызов функции в SQL выражении
/// Примеры: UPPER(name), COALESCE(price, 0), now(), COUNT(*)
/// </summary>
public sealed record PgFunctionCall : PgExpression
{
    /// <summary>
    /// Имя функции
    /// Примеры: "upper", "coalesce", "now", "json_agg"
    /// </summary>
    public required string FunctionName { get; init; }

    /// <summary>
    /// Схема функции (если указана)
    /// Пример: "pg_catalog", "public"
    /// </summary>
    public string? SchemaName { get; init; }

    /// <summary>
    /// Список аргументов функции
    /// Пустой список для функций без аргументов: now()
    /// </summary>
    public IReadOnlyList<PgExpression> Arguments { get; init; } = [];

    /// <summary>
    /// Квантификатор для агрегатных функций: DISTINCT или ALL
    /// Пример: COUNT(DISTINCT id)
    /// </summary>
    public PgSetQuantifier? SetQuantifier { get; init; }

    /// <summary>
    /// ORDER BY клауза внутри агрегатной функции
    /// Пример: STRING_AGG(name, ',' ORDER BY name)
    /// </summary>
    public IReadOnlyList<PgOrderByItem>? OrderBy { get; init; }

    /// <summary>
    /// FILTER клауза для агрегатных функций
    /// Пример: COUNT(*) FILTER (WHERE status = 'active')
    /// </summary>
    public PgExpression? FilterClause { get; init; }

    /// <summary>
    /// OVER клауза для оконных функций
    /// Пример: ROW_NUMBER() OVER (PARTITION BY category ORDER BY price)
    /// </summary>
    public PgWindowClause? WindowClause { get; init; }

    /// <summary>
    /// Флаг для VARIADIC параметра
    /// Пример: format('%s %s', VARIADIC array['hello', 'world'])
    /// </summary>
    public bool IsVariadic { get; init; }
}
