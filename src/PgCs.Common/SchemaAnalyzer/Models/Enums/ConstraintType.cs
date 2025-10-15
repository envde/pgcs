namespace PgCs.Common.SchemaAnalyzer.Models.Enums;

/// <summary>
/// Тип ограничения
/// </summary>
public enum ConstraintType
{
    PrimaryKey,
    ForeignKey,
    Unique,
    Check,
    Exclude
}