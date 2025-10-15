namespace PgCs.Common.QueryAnalyzer.Models;

/// <summary>
/// Параметр SQL запроса
/// </summary>
public class QueryParameter
{
    /// <summary>
    /// Имя параметра (без @/$)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// PostgreSQL тип данных
    /// </summary>
    public required string PostgresType { get; init; }

    /// <summary>
    /// C# тип данных
    /// </summary>
    public required string CSharpType { get; init; }

    /// <summary>
    /// Nullable тип
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Позиция параметра в запросе (для $1, $2 и т.д.)
    /// </summary>
    public int Position { get; init; }
}
