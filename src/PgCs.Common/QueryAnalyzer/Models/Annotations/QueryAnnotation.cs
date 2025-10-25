using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.Common.QueryAnalyzer.Models.Annotations;

/// <summary>
/// Аннотации из комментариев sqlc с расширенной документацией
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

    /// <summary>
    /// Описание метода для XML комментария summary (из -- summary:)
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// Описания параметров для XML комментариев param (из -- param: name Description)
    /// </summary>
    public IReadOnlyDictionary<string, string> ParameterDescriptions { get; init; } = 
        new Dictionary<string, string>();

    /// <summary>
    /// Описание возвращаемого значения для XML комментария returns (из -- returns:)
    /// </summary>
    public string? Returns { get; init; }
}
