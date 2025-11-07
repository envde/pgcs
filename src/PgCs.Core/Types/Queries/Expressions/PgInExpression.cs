namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// IN выражение в PostgreSQL
/// Примеры: status IN ('active', 'pending'), id NOT IN (SELECT user_id FROM banned)
/// </summary>
public sealed record PgInExpression : PgExpression
{
    /// <summary>
    /// Проверяемое выражение
    /// </summary>
    public required PgExpression Expression { get; init; }

    /// <summary>
    /// Список значений для проверки (если используется список литералов)
    /// Пример: IN (1, 2, 3)
    /// </summary>
    public IReadOnlyList<PgExpression>? ValueList { get; init; }

    /// <summary>
    /// Подзапрос для проверки (если используется subquery)
    /// Пример: IN (SELECT id FROM users)
    /// </summary>
    public PgSelectQuery? Subquery { get; init; }

    /// <summary>
    /// Инверсия: NOT IN
    /// </summary>
    public bool IsNot { get; init; }
}
