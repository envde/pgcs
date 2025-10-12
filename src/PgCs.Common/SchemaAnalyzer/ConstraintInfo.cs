namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация об ограничении (constraint)
/// </summary>
public class ConstraintInfo
{
    /// <summary>
    /// Имя constraint
    /// </summary>
    public required string ConstraintName { get; init; }

    /// <summary>
    /// Тип (CHECK, UNIQUE, EXCLUDE)
    /// </summary>
    public required string ConstraintType { get; init; }

    /// <summary>
    /// Определение constraint
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// Колонки, участвующие в constraint
    /// </summary>
    public IReadOnlyList<string> Columns { get; init; } = Array.Empty<string>();
}
