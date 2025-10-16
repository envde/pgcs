namespace PgCs.Common.QueryAnalyzer.Models.Results;

/// <summary>
/// Колонка в результате запроса
/// </summary>
public record ReturnColumn
{
    public required string Name { get; init; }
    public required string PostgresType { get; init; }
    public required string CSharpType { get; init; }
    public bool IsNullable { get; init; }
}
