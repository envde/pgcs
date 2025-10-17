using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Generation;

/// <summary>
/// Интерфейс генератора моделей параметров запросов
/// </summary>
internal interface IParameterModelGenerator
{
    /// <summary>
    /// Генерирует модель параметров для запроса
    /// </summary>
    GeneratedModel? Generate(QueryMetadata query, QueryGenerationOptions options);
}
