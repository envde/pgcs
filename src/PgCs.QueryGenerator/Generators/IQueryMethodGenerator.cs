using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;

namespace PgCs.QueryGenerator.Generators;

/// <summary>
/// Генератор C# методов для SQL запросов
/// </summary>
public interface IQueryMethodGenerator
{
    /// <summary>
    /// Генерирует C# метод для выполнения SQL запроса
    /// </summary>
    ValueTask<GeneratedMethodResult> GenerateAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options);
}
