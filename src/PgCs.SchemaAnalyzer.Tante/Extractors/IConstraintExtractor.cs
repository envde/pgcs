using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений ограничений целостности из SQL блоков
/// </summary>
public interface IConstraintExtractor
{
    /// <summary>
    /// Извлекает определение ограничения целостности из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением ограничения</param>
    /// <returns>Определение ограничения или null, если блок не содержит ALTER TABLE ADD CONSTRAINT</returns>
    ConstraintDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение ограничения целостности
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит ALTER TABLE ADD CONSTRAINT</returns>
    bool CanExtract(SqlBlock block);
}
