using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// Присваивание значения колонке в UPDATE запросе
/// Пример: SET column = value
/// </summary>
public sealed record PgSetClause
{
    /// <summary>
    /// Имя обновляемой колонки
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// Новое значение для колонки
    /// </summary>
    public required PgExpression Value { get; init; }
}
