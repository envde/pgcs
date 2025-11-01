using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения комментариев к таблицам из SQL блоков
/// </summary>
public interface ITableCommentExtractor
{
    /// <summary>
    /// Извлекает определение комментария к таблице из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением COMMENT ON TABLE</param>
    /// <returns>Определение комментария таблицы или null, если блок не содержит COMMENT ON TABLE</returns>
    TableCommentDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение комментария к таблице
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит COMMENT ON TABLE</returns>
    bool CanExtract(SqlBlock block);
}
