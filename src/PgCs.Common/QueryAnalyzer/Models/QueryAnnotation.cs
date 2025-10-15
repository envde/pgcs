using PgCs.Common.QueryAnalyzer.Models.Enums;

namespace PgCs.Common.QueryAnalyzer.Models;

/// <summary>
/// Аннотации из комментариев sqlc
/// </summary>
public class QueryAnnotation
{
    /// <summary>
    /// Имя метода из комментария (-- name: GetUser)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип возврата (:one, :many, :exec, :execrows)
    /// </summary>
    public required ReturnCardinality Cardinality { get; init; }
}
