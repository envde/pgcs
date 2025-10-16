using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.Common.QueryAnalyzer.Models.Metadata;

/// <summary>
/// Метаданные проанализированного SQL запроса
/// </summary>
public record QueryMetadata
{
    /// <summary>
    /// Имя метода для генерации
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// Оригинальный SQL запрос
    /// </summary>
    public required string SqlQuery { get; init; }

    /// <summary>
    /// Тип запроса (Select, Insert, Update, Delete)
    /// </summary>
    public QueryType QueryType { get; init; }

    /// <summary>
    /// Тип возвращаемого значения (:one, :many, :exec)
    /// </summary>
    public ReturnCardinality ReturnCardinality { get; init; }

    /// <summary>
    /// Параметры запроса
    /// </summary>
    public required IReadOnlyList<QueryParameter> Parameters { get; init; }

    /// <summary>
    /// Информация о возвращаемом типе
    /// </summary>
    public ReturnTypeInfo? ReturnType { get; init; }

    /// <summary>
    /// Имя модели, которую нужно использовать (если указано явно)
    /// </summary>
    public string? ExplicitModelName { get; init; }
}
