namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация о внешнем ключе
/// </summary>
public class ForeignKeyInfo
{
    /// <summary>
    /// Имя constraint
    /// </summary>
    public required string ConstraintName { get; init; }

    /// <summary>
    /// Таблица, содержащая FK
    /// </summary>
    public required string FromTable { get; init; }

    /// <summary>
    /// Колонки в таблице FROM
    /// </summary>
    public required IReadOnlyList<string> FromColumns { get; init; }

    /// <summary>
    /// Референсная таблица
    /// </summary>
    public required string ToTable { get; init; }

    /// <summary>
    /// Колонки в таблице TO
    /// </summary>
    public required IReadOnlyList<string> ToColumns { get; init; }

    /// <summary>
    /// Действие при UPDATE (CASCADE, SET NULL, RESTRICT, NO ACTION)
    /// </summary>
    public string? OnUpdate { get; init; }

    /// <summary>
    /// Действие при DELETE (CASCADE, SET NULL, RESTRICT, NO ACTION)
    /// </summary>
    public string? OnDelete { get; init; }
}
