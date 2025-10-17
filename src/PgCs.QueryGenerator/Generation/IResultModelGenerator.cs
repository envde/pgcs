using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Generation;

/// <summary>
/// Интерфейс генератора моделей результатов запросов
/// </summary>
internal interface IResultModelGenerator
{
    /// <summary>
    /// Генерирует модель результата для SELECT запроса
    /// </summary>
    GeneratedModel? Generate(QueryMetadata query, QueryGenerationOptions options);
}
