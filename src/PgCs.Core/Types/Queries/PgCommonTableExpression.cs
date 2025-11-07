namespace PgCs.Core.Types.Queries;

/// <summary>
/// Common Table Expression (CTE) - подзапрос в WITH клаузе
/// Пример: WITH active_users AS (SELECT * FROM users WHERE active) SELECT * FROM active_users
/// </summary>
public sealed record PgCommonTableExpression
{
    /// <summary>
    /// Имя CTE (обязательно)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Список колонок CTE (опционально)
    /// Пример: WITH cte(id, name) AS ...
    /// </summary>
    public IReadOnlyList<string> ColumnNames { get; init; } = [];

    /// <summary>
    /// Запрос CTE
    /// </summary>
    public required PgSelectQuery Query { get; init; }

    /// <summary>
    /// Материализация CTE: MATERIALIZED или NOT MATERIALIZED
    /// null означает решение оптимизатором
    /// </summary>
    public bool? IsMaterialized { get; init; }
}
