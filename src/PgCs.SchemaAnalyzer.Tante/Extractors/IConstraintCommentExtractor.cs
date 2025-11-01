using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения комментариев к ограничениям из SQL блоков
/// </summary>
public interface IConstraintCommentExtractor
{
    /// <summary>
    /// Извлекает определение комментария к ограничению из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением COMMENT ON CONSTRAINT</param>
    /// <returns>Определение комментария ограничения или null, если блок не содержит COMMENT ON CONSTRAINT</returns>
    ConstraintCommentDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение комментария к ограничению
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит COMMENT ON CONSTRAINT</returns>
    bool CanExtract(SqlBlock block);
}
