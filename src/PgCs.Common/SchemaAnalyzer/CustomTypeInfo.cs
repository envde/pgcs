namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация о пользовательском типе
/// </summary>
public class CustomTypeInfo
{
    /// <summary>
    /// Имя типа
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Схема
    /// </summary>
    public required string SchemaName { get; init; }

    /// <summary>
    /// Категория типа (composite, enum, range, base)
    /// </summary>
    public required string TypeCategory { get; init; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; init; }
}
