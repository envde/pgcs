namespace PgCs.Common.SchemaAnalyzer.Models.Enums;

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