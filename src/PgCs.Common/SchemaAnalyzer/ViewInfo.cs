namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация о представлении (view)
/// </summary>
public class ViewInfo
{
    /// <summary>
    /// Имя представления
    /// </summary>
    public required string ViewName { get; init; }

    /// <summary>
    /// Схема
    /// </summary>
    public required string SchemaName { get; init; }

    /// <summary>
    /// SQL определение
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// Колонки представления
    /// </summary>
    public required IReadOnlyList<ColumnInfo> Columns { get; init; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; init; }
}
