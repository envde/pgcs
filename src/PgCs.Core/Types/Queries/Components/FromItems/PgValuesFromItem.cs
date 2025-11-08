using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components.FromItems;

/// <summary>
/// VALUES конструктор в FROM клаузе
/// Пример: FROM (VALUES (1, 'a'), (2, 'b')) AS t(id, name)
/// </summary>
public sealed record PgValuesFromItem : PgFromItem
{
    /// <summary>
    /// Строки значений
    /// </summary>
    public required IReadOnlyList<IReadOnlyList<PgExpression>> Rows { get; init; }
}
