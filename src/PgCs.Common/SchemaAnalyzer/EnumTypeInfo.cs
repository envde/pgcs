namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация об ENUM типе
/// </summary>
public class EnumTypeInfo
{
    /// <summary>
    /// Имя enum
    /// </summary>
    public required string EnumName { get; init; }

    /// <summary>
    /// Схема
    /// </summary>
    public required string SchemaName { get; init; }

    /// <summary>
    /// Значения enum (в порядке)
    /// </summary>
    public required IReadOnlyList<string> Values { get; init; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; init; }
}
