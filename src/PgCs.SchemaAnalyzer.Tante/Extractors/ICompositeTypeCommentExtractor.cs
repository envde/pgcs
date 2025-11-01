using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения комментариев к композитным типам из SQL блоков
/// </summary>
public interface ICompositeTypeCommentExtractor
{
    /// <summary>
    /// Извлекает определение комментария к композитному типу из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением COMMENT ON TYPE</param>
    /// <returns>Определение комментария типа или null, если блок не содержит COMMENT ON TYPE</returns>
    CompositeTypeCommentDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение комментария к композитному типу
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит COMMENT ON TYPE</returns>
    bool CanExtract(SqlBlock block);
}
