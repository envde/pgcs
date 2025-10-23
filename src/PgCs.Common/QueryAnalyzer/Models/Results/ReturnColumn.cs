namespace PgCs.Common.QueryAnalyzer.Models.Results;

/// <summary>
/// Колонка в результате SQL запроса с информацией о типах данных
/// </summary>
public record ReturnColumn
{
    /// <summary>
    /// Имя колонки (или алиас из AS)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// PostgreSQL тип данных колонки
    /// </summary>
    public required string PostgresType { get; init; }

    /// <summary>
    /// Соответствующий C# тип данных
    /// </summary>
    public required string CSharpType { get; init; }

    /// <summary>
    /// Может ли колонка содержать NULL значения
    /// </summary>
    public bool IsNullable { get; init; }
}
