namespace PgCs.Common.SchemaAnalyzer.Models.Triggers;

/// <summary>
/// Событие DML операции, вызывающее срабатывание триггера
/// </summary>
public enum TriggerEvent
{
    /// <summary>
    /// INSERT - вставка новой строки
    /// </summary>
    Insert,

    /// <summary>
    /// UPDATE - обновление существующей строки
    /// </summary>
    Update,

    /// <summary>
    /// DELETE - удаление строки
    /// </summary>
    Delete,

    /// <summary>
    /// TRUNCATE - полная очистка таблицы
    /// </summary>
    Truncate
}