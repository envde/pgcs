namespace PgCs.Core.Types.Base;

/// <summary>
/// Событие DML операции, вызывающее срабатывание триггера
/// </summary>
public enum PgTriggerEvent
{
    /// <summary>
    /// INSERT - вставка новой строки
    /// </summary>
    /// <remarks>
    /// Доступна переменная NEW с новыми значениями.
    /// OLD недоступна.
    /// </remarks>
    Insert,

    /// <summary>
    /// UPDATE - обновление существующей строки
    /// </summary>
    /// <remarks>
    /// Доступны обе переменные: NEW (новые значения) и OLD (старые значения).
    /// Можно сравнивать изменения.
    /// </remarks>
    Update,

    /// <summary>
    /// DELETE - удаление строки
    /// </summary>
    /// <remarks>
    /// Доступна переменная OLD со старыми значениями.
    /// NEW недоступна.
    /// </remarks>
    Delete,

    /// <summary>
    /// TRUNCATE - полная очистка таблицы
    /// </summary>
    /// <remarks>
    /// Не работает на уровне ROW (только STATEMENT).
    /// OLD и NEW недоступны.
    /// </remarks>
    Truncate
}
