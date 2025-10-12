namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация об индексе
/// </summary>
public class IndexInfo
{
    /// <summary>
    /// Имя индекса
    /// </summary>
    public required string IndexName { get; init; }

    /// <summary>
    /// Таблица
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Уникальный индекс
    /// </summary>
    public bool IsUnique { get; init; }

    /// <summary>
    /// Первичный ключ
    /// </summary>
    public bool IsPrimaryKey { get; init; }

    /// <summary>
    /// Колонки в индексе (в порядке)
    /// </summary>
    public required IReadOnlyList<string> Columns { get; init; }

    /// <summary>
    /// Тип индекса (btree, hash, gin, gist, brin)
    /// </summary>
    public string IndexType { get; init; } = "btree";

    /// <summary>
    /// Определение индекса (для выражений)
    /// </summary>
    public string? Definition { get; init; }
}
