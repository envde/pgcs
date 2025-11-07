namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Выражение с скобками (для группировки)
/// Пример: (a + b) * c
/// </summary>
public sealed record PgParenthesizedExpression : PgExpression
{
    /// <summary>
    /// Выражение внутри скобок
    /// </summary>
    public required PgExpression Expression { get; init; }
}
