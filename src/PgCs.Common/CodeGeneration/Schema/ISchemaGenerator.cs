using PgCs.Common.CodeGeneration.Models;
using PgCs.Common.CodeGeneration.Schema.Models;
using PgCs.Common.SchemaAnalyzer.Models;

namespace PgCs.Common.CodeGeneration.Schema;

public interface ISchemaGenerator
{
    /// <summary>
    /// Генерирует все файлы из схемы
    /// </summary>
    ValueTask<IReadOnlyList<GeneratedFile>> GenerateAsync(
        SchemaMetadata schema,
        ModelGenerationOptions options,
        CancellationToken cancellationToken = default);
}