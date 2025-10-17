using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Generation;

/// <summary>
/// Интерфейс генератора методов
/// </summary>
internal interface IMethodGenerator
{
    /// <summary>
    /// Генерирует метод для выполнения SQL запроса
    /// </summary>
    GeneratedMethod Generate(QueryMetadata query, QueryGenerationOptions options);
}
