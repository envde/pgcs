namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация о таблице
/// </summary>
public class TableInfo
{
    /// <summary>
    /// Имя схемы
    /// </summary>
    public required string SchemaName { get; init; }

    /// <summary>
    /// Имя таблицы
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Комментарий к таблице
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Колонки таблицы
    /// </summary>
    public required IReadOnlyList<ColumnInfo> Columns { get; init; }

    /// <summary>
    /// Первичный ключ
    /// </summary>
    public PrimaryKeyInfo? PrimaryKey { get; init; }

    /// <summary>
    /// Индексы таблицы
    /// </summary>
    public IReadOnlyList<IndexInfo> Indexes { get; init; } = Array.Empty<IndexInfo>();

    /// <summary>
    /// Ограничения (constraints)
    /// </summary>
    public IReadOnlyList<ConstraintInfo> Constraints { get; init; } = Array.Empty<ConstraintInfo>();

    /// <summary>
    /// Foreign keys из этой таблицы
    /// </summary>
    public IReadOnlyList<ForeignKeyInfo> ForeignKeys { get; init; } = Array.Empty<ForeignKeyInfo>();
}
