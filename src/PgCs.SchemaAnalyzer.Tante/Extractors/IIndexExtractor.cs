using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений индексов из SQL блоков
/// </summary>
public interface IIndexExtractor
{
    /// <summary>
    /// Извлекает определение индекса из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением индекса</param>
    /// <returns>Определение индекса или null, если блок не содержит CREATE INDEX</returns>
    IndexDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение индекса
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит CREATE INDEX</returns>
    bool CanExtract(SqlBlock block);
}
