using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Generation;

/// <summary>
/// Интерфейс генератора классов с методами запросов
/// </summary>
internal interface IClassGenerator
{
    /// <summary>
    /// Генерирует класс с методами для выполнения запросов
    /// </summary>
    GeneratedClass Generate(
        IReadOnlyList<QueryMetadata> queries,
        string className,
        QueryGenerationOptions options);
}
