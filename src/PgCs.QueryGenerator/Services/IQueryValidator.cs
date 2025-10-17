using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;

namespace PgCs.QueryGenerator.Services;

/// <summary>
/// Валидатор метаданных запросов перед генерацией
/// </summary>
public interface IQueryValidator
{
    /// <summary>
    /// Проверяет корректность метаданных запросов
    /// </summary>
    IReadOnlyList<ValidationIssue> Validate(IReadOnlyList<QueryMetadata> queries);
}
