using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений Composite типов из SQL блоков
/// </summary>
public interface ICompositeExtractor
{
    /// <summary>
    /// Извлекает определение Composite типа из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением Composite</param>
    /// <returns>Определение Composite типа или null, если блок не содержит CREATE TYPE AS ()</returns>
    CompositeTypeDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение Composite типа
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит CREATE TYPE AS ()</returns>
    bool CanExtract(SqlBlock block);
}
