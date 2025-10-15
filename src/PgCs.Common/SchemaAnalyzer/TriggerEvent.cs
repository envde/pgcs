namespace PgCs.Common.SchemaAnalyzer;

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