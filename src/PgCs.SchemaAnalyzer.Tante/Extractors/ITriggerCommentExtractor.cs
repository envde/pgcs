using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения комментариев к триггерам из SQL блоков
/// </summary>
public interface ITriggerCommentExtractor
{
    /// <summary>
    /// Извлекает определение комментария к триггеру из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением COMMENT ON TRIGGER</param>
    /// <returns>Определение комментария триггера или null, если блок не содержит COMMENT ON TRIGGER</returns>
    TriggerCommentDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение комментария к триггеру
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит COMMENT ON TRIGGER</returns>
    bool CanExtract(SqlBlock block);
}
