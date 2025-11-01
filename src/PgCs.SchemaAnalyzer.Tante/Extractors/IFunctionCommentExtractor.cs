using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения комментариев к функциям из SQL блоков
/// </summary>
public interface IFunctionCommentExtractor
{
    /// <summary>
    /// Извлекает определение комментария к функции из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением COMMENT ON FUNCTION</param>
    /// <returns>Определение комментария функции или null, если блок не содержит COMMENT ON FUNCTION</returns>
    FunctionCommentDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение комментария к функции
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит COMMENT ON FUNCTION</returns>
    bool CanExtract(SqlBlock block);
}
