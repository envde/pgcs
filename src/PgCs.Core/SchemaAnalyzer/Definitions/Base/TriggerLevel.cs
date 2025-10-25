namespace PgCs.Core.SchemaAnalyzer.Definitions.Base;

/// <summary>
/// Уровень гранулярности срабатывания триггера
/// </summary>
public enum TriggerLevel
{
    /// <summary>
    /// ROW - триггер срабатывает для каждой затронутой строки
    /// </summary>
    Row,

    /// <summary>
    /// STATEMENT - триггер срабатывает один раз для всей операции
    /// </summary>
    Statement
}