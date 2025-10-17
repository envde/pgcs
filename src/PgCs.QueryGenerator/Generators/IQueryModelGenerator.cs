using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;

namespace PgCs.QueryGenerator.Generators;

/// <summary>
/// Генератор моделей для результатов и параметров запросов
/// </summary>
public interface IQueryModelGenerator
{
    /// <summary>
    /// Генерирует модель результата запроса (DTO)
    /// </summary>
    ValueTask<GeneratedModelResult> GenerateResultModelAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options);

    /// <summary>
    /// Генерирует модель параметров запроса
    /// </summary>
    ValueTask<GeneratedModelResult> GenerateParameterModelAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options);
}
