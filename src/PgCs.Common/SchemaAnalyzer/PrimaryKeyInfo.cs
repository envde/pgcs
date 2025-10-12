namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация о первичном ключе
/// </summary>
public class PrimaryKeyInfo
{
    /// <summary>
    /// Имя constraint первичного ключа
    /// </summary>
    public required string ConstraintName { get; init; }

    /// <summary>
    /// Колонки, входящие в первичный ключ
    /// </summary>
    public required IReadOnlyList<string> ColumnNames { get; init; }
}
