using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// Операция над множествами (SET operation) для объединения результатов запросов
/// Примеры: UNION, INTERSECT, EXCEPT
/// </summary>
public sealed record PgSetOperation
{
    /// <summary>
    /// Тип операции над множествами
    /// </summary>
    public required PgSetOperationType OperationType { get; init; }

    /// <summary>
    /// Правый запрос для операции
    /// </summary>
    public required PgSelectQuery RightQuery { get; init; }
}
