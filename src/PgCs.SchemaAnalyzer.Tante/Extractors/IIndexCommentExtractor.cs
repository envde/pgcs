using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения комментариев к индексам из SQL блоков
/// </summary>
public interface IIndexCommentExtractor
{
    /// <summary>
    /// Извлекает определение комментария к индексу из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением COMMENT ON INDEX</param>
    /// <returns>Определение комментария индекса или null, если блок не содержит COMMENT ON INDEX</returns>
    IndexCommentDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение комментария к индексу
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит COMMENT ON INDEX</returns>
    bool CanExtract(SqlBlock block);
}
