using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения комментариев к колонкам из SQL блоков
/// </summary>
public interface IColumnCommentExtractor
{
    /// <summary>
    /// Извлекает определение комментария к колонке из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением COMMENT ON COLUMN</param>
    /// <returns>Определение комментария колонки или null, если блок не содержит COMMENT ON COLUMN</returns>
    ColumnCommentDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение комментария к колонке
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит COMMENT ON COLUMN</returns>
    bool CanExtract(SqlBlock block);
}
