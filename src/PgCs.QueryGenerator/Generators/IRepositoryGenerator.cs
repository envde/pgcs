using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;

namespace PgCs.QueryGenerator.Generators;

/// <summary>
/// Генератор интерфейса и реализации репозитория запросов
/// </summary>
public interface IRepositoryGenerator
{
    /// <summary>
    /// Генерирует интерфейс репозитория с методами запросов
    /// </summary>
    ValueTask<GeneratedInterfaceResult> GenerateInterfaceAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options);

    /// <summary>
    /// Генерирует реализацию репозитория с методами запросов
    /// </summary>
    ValueTask<GeneratedClassResult> GenerateImplementationAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options);
}
