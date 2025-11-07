namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Выражение массива в PostgreSQL
/// Примеры: ARRAY[1, 2, 3], ARRAY(SELECT id FROM users), '{1,2,3}'::integer[]
/// </summary>
public sealed record PgArrayExpression : PgExpression
{
    /// <summary>
    /// Элементы массива (если массив создаётся через конструктор ARRAY[...])
    /// </summary>
    public IReadOnlyList<PgExpression>? Elements { get; init; }

    /// <summary>
    /// Подзапрос для создания массива (если используется ARRAY(SELECT ...))
    /// </summary>
    public PgSelectQuery? Query { get; init; }

    /// <summary>
    /// Тип элементов массива (если указан явно)
    /// Примеры: "integer", "text"
    /// </summary>
    public string? ElementType { get; init; }

    /// <summary>
    /// Размерность массива
    /// 1 для одномерного [1,2,3], 2 для двумерного [[1,2],[3,4]]
    /// </summary>
    public int Dimension { get; init; } = 1;
}
