namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Действие при нарушении ссылочной целостности
/// </summary>
public enum ReferentialAction
{
    NoAction,
    Restrict,
    Cascade,
    SetNull,
    SetDefault
}