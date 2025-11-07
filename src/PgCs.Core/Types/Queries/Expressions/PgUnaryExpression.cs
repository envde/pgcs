using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Унарное выражение с одним операндом
/// Примеры: NOT condition, -value, +value
/// </summary>
public sealed record PgUnaryExpression : PgExpression
{
    /// <summary>
    /// Оператор унарного выражения
    /// </summary>
    public required PgOperatorType Operator { get; init; }

    /// <summary>
    /// Операнд выражения
    /// </summary>
    public required PgExpression Operand { get; init; }

    /// <summary>
    /// Префиксная (true) или постфиксная (false) форма оператора
    /// Примеры: NOT x (prefix), x IS NULL (postfix)
    /// </summary>
    public bool IsPrefix { get; init; } = true;
}
