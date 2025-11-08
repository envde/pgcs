namespace PgCs.Core.Types.Queries.Components.FromItems;

/// <summary>
/// Элемент в FROM клаузе
/// Может быть: таблица, подзапрос, функция, VALUES, или JOIN
/// </summary>
public abstract record PgFromItem
{
    /// <summary>
    /// Алиас для элемента FROM (опционально)
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// Алиасы для колонок (опционально)
    /// Пример: FROM users AS u(id, name, email)
    /// </summary>
    public IReadOnlyList<string> ColumnAliases { get; init; } = [];
}
