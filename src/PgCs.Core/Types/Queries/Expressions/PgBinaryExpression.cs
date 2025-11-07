using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Бинарное выражение с двумя операндами и оператором
/// Примеры: a = b, x + y, column1 AND column2, price > 100
/// </summary>
public sealed record PgBinaryExpression : PgExpression
{
    /// <summary>
    /// Левый операнд выражения
    /// </summary>
    public required PgExpression Left { get; init; }

    /// <summary>
    /// Оператор бинарного выражения
    /// </summary>
    public required PgOperatorType Operator { get; init; }

    /// <summary>
    /// Правый операнд выражения
    /// </summary>
    public required PgExpression Right { get; init; }
}
