namespace PgCs.Core.SchemaAnalyzer.Definitions.Base;

/// <summary>
/// Время срабатывания триггера относительно операции DML
/// </summary>
public enum TriggerTiming
{
    /// <summary>
    /// BEFORE - триггер срабатывает до выполнения операции (можно изменить или отменить операцию)
    /// </summary>
    Before,

    /// <summary>
    /// AFTER - триггер срабатывает после выполнения операции
    /// </summary>
    After,

    /// <summary>
    /// INSTEAD OF - триггер заменяет собой операцию (используется для представлений)
    /// </summary>
    InsteadOf
}