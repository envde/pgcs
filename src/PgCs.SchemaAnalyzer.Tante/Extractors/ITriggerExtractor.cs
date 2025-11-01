using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений триггеров из SQL блоков
/// </summary>
public interface ITriggerExtractor
{
    /// <summary>
    /// Извлекает определение триггера из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением триггера</param>
    /// <returns>Определение триггера или null, если блок не содержит CREATE TRIGGER</returns>
    TriggerDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение триггера
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит CREATE TRIGGER</returns>
    bool CanExtract(SqlBlock block);
}
