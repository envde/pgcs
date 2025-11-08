using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components.FromItems;

/// <summary>
/// Вызов функции в FROM клаузе
/// Пример: FROM generate_series(1, 10) AS numbers(n)
/// </summary>
public sealed record PgFunctionFromItem : PgFromItem
{
    /// <summary>
    /// Вызов функции
    /// </summary>
    public required PgFunctionCall Function { get; init; }

    /// <summary>
    /// WITH ORDINALITY - добавляет порядковый номер строки
    /// </summary>
    public bool WithOrdinality { get; init; }
}
