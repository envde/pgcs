using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components.GroupBy;

/// <summary>
/// Простое выражение группировки
/// Пример: GROUP BY column1, column2
/// </summary>
public sealed record PgSimpleGroupingElement : PgGroupingElement
{
    /// <summary>
    /// Выражение для группировки
    /// </summary>
    public required PgExpression Expression { get; init; }
}
