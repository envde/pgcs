using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// Клауза блокировки строк в SELECT запросе
/// Примеры: FOR UPDATE, FOR SHARE, FOR NO KEY UPDATE, FOR KEY SHARE
/// </summary>
public sealed record PgLockingClause
{
    /// <summary>
    /// Уровень блокировки
    /// </summary>
    public required PgLockStrength Strength { get; init; }

    /// <summary>
    /// Список таблиц для блокировки (опционально)
    /// Если пусто, блокируются все таблицы в FROM
    /// Пример: FOR UPDATE OF users, orders
    /// </summary>
    public IReadOnlyList<string> OfTables { get; init; } = [];

    /// <summary>
    /// Поведение при невозможности получить блокировку
    /// </summary>
    public PgLockWaitPolicy WaitPolicy { get; init; } = PgLockWaitPolicy.Wait;
}

/// <summary>
/// Политика ожидания блокировки
/// </summary>
public enum PgLockWaitPolicy
{
    /// <summary>Ждать получения блокировки (по умолчанию)</summary>
    Wait,

    /// <summary>Не ждать, пропустить заблокированные строки: SKIP LOCKED</summary>
    SkipLocked,

    /// <summary>Не ждать, вернуть ошибку: NOWAIT</summary>
    NoWait
}
