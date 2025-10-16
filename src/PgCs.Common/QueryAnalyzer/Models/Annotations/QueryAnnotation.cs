using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.Common.QueryAnalyzer.Models.Annotations;

/// <summary>
/// Аннотации из комментариев sqlc
/// </summary>
public record QueryAnnotation
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
