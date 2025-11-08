using PgCs.Core.Types.Base;
using PgCs.Core.Types.Queries.Components;
using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries;

/// <summary>
/// Запрос с операциями над множествами (UNION, INTERSECT, EXCEPT)
/// Объединяет результаты двух или более SELECT запросов
/// ORDER BY, LIMIT, OFFSET применяются ко всему результату операции
/// </summary>
public sealed record PgSetOperationQuery : PgQuery
{
    /// <summary>
    /// Левый SELECT запрос
    /// </summary>
    public required PgSelectQuery LeftQuery { get; init; }

    /// <summary>
    /// Тип операции над множествами (UNION, INTERSECT, EXCEPT)
    /// </summary>
    public required PgSetOperationType OperationType { get; init; }

    /// <summary>
    /// Модификатор ALL для сохранения дубликатов
    /// По умолчанию операции удаляют дубликаты (UNION = UNION DISTINCT)
    /// С ALL дубликаты сохраняются (UNION ALL)
    /// </summary>
    public bool IsAll { get; init; }

    /// <summary>
    /// Правый SELECT запрос
    /// </summary>
    public required PgSelectQuery RightQuery { get; init; }

    /// <summary>
    /// Дополнительные операции для цепочек
    /// Пример: SELECT ... UNION SELECT ... INTERSECT SELECT ...
    /// </summary>
    public IReadOnlyList<PgChainedSetOperation> ChainedOperations { get; init; } = [];

    /// <summary>
    /// ORDER BY клауза для всего результата
    /// Применяется после выполнения всех операций над множествами
    /// </summary>
    public IReadOnlyList<PgOrderByItem> OrderByClause { get; init; } = [];

    /// <summary>
    /// LIMIT количество возвращаемых строк (применяется к финальному результату)
    /// </summary>
    public PgExpression? LimitClause { get; init; }

    /// <summary>
    /// OFFSET смещение для пропуска строк (применяется к финальному результату)
    /// </summary>
    public PgExpression? OffsetClause { get; init; }

    /// <summary>
    /// FETCH FIRST/NEXT клауза (SQL стандартная альтернатива LIMIT)
    /// Применяется к финальному результату
    /// </summary>
    public PgFetchClause? FetchClause { get; init; }

    /// <summary>
    /// FOR UPDATE/SHARE клаузы блокировки
    /// Применяются к финальному результату (редко используется с SET operations)
    /// </summary>
    public IReadOnlyList<PgLockingClause> LockingClauses { get; init; } = [];
}

/// <summary>
/// Дополнительная операция в цепочке SET операций
/// Пример: query1 UNION query2 INTERSECT query3
/// </summary>
public sealed record PgChainedSetOperation
{
    /// <summary>
    /// Тип операции над множествами
    /// </summary>
    public required PgSetOperationType OperationType { get; init; }

    /// <summary>
    /// Модификатор ALL
    /// </summary>
    public bool IsAll { get; init; }

    /// <summary>
    /// Следующий SELECT запрос в цепочке
    /// </summary>
    public required PgSelectQuery Query { get; init; }
}
