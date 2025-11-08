namespace PgCs.Core.Types.Queries.Components.FromItems;

/// <summary>
/// Обычная таблица в FROM клаузе
/// </summary>
public sealed record PgTableFromItem : PgFromItem
{
    /// <summary>
    /// Ссылка на таблицу
    /// </summary>
    public required PgTableReference Table { get; init; }
}
