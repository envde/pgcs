namespace PgCs.Common.SchemaAnalyzer.Models.Triggers;

/// <summary>
/// Время срабатывания триггера
/// </summary>
public enum TriggerTiming
{
    Before,
    After,
    InsteadOf
}