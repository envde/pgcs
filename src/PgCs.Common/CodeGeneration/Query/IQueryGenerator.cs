using PgCs.Common.CodeGeneration.Models;
using PgCs.Common.CodeGeneration.Query.Models;
using PgCs.Common.QueryAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models;

namespace PgCs.Common.CodeGeneration.Query;

public interface IQueryGenerator
{
    /// <summary>
    /// Генерирует все файлы для запросов
    /// </summary>
    ValueTask<IReadOnlyList<GeneratedFile>> GenerateAsync(
        IReadOnlyList<QueryMetadata> queries,
        MethodGenerationOptions options,
        SchemaMetadata? schema = null,
        CancellationToken cancellationToken = default);
}