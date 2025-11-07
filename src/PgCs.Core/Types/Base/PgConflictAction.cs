namespace PgCs.Core.Types.Base;

/// <summary>
/// Действие при конфликте уникальности в INSERT запросе (ON CONFLICT)
/// PostgreSQL поддерживает UPSERT через ON CONFLICT
/// </summary>
public enum PgConflictAction
{
    /// <summary>Не делать ничего при конфликте: ON CONFLICT DO NOTHING</summary>
    Nothing,

    /// <summary>Обновить существующую строку: ON CONFLICT DO UPDATE SET ...</summary>
    Update
}
