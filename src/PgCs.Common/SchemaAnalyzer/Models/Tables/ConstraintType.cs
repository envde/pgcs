namespace PgCs.Common.SchemaAnalyzer.Models.Tables;

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