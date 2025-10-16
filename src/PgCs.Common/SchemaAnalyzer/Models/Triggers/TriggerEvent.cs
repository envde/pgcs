namespace PgCs.Common.SchemaAnalyzer.Models.Triggers;

/// <summary>
/// Событие, вызывающее триггер
/// </summary>
public enum TriggerEvent
{
    Insert,
    Update,
    Delete,
    Truncate
}