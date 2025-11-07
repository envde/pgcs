namespace PgCs.Core.Types.Base;

/// <summary>
/// Типы блокировки строк в SELECT запросе (FOR UPDATE/SHARE)
/// PostgreSQL поддерживает различные уровни блокировки для конкурентного доступа
/// </summary>
public enum PgLockStrength
{
    /// <summary>FOR UPDATE - самая строгая блокировка</summary>
    Update,

    /// <summary>FOR NO KEY UPDATE - блокировка без блокировки ключевых полей</summary>
    NoKeyUpdate,

    /// <summary>FOR SHARE - разделяемая блокировка</summary>
    Share,

    /// <summary>FOR KEY SHARE - разделяемая блокировка только ключевых полей</summary>
    KeyShare
}
