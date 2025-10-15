namespace PgCs.Common.SchemaAnalyzer;

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