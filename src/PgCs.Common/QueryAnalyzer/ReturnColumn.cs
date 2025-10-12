namespace PgCs.Common.QueryAnalyzer;

/// <summary>
/// Колонка в результате запроса
/// </summary>
public class ReturnColumn
{
    public required string Name { get; init; }
    public required string PostgresType { get; init; }
    public required string CSharpType { get; init; }
    public bool IsNullable { get; init; }
}
