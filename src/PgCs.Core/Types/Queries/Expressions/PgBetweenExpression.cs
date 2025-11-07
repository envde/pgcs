using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// BETWEEN выражение в PostgreSQL
/// Примеры: age BETWEEN 18 AND 65, price NOT BETWEEN 100 AND 200
/// </summary>
public sealed record PgBetweenExpression : PgExpression
{
    /// <summary>
    /// Проверяемое выражение
    /// </summary>
    public required PgExpression Expression { get; init; }

    /// <summary>
    /// Нижняя граница диапазона
    /// </summary>
    public required PgExpression LowerBound { get; init; }

    /// <summary>
    /// Верхняя граница диапазона
    /// </summary>
    public required PgExpression UpperBound { get; init; }

    /// <summary>
    /// Инверсия: NOT BETWEEN
    /// </summary>
    public bool IsNot { get; init; }
}
