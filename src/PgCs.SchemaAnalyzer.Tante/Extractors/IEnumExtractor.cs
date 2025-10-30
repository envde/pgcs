using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений ENUM типов из SQL блоков
/// </summary>
public interface IEnumExtractor
{
    /// <summary>
    /// Извлекает определение ENUM типа из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением ENUM</param>
    /// <returns>Определение ENUM типа или null, если блок не содержит CREATE TYPE AS ENUM</returns>
    EnumTypeDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение ENUM типа
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит CREATE TYPE AS ENUM</returns>
    bool CanExtract(SqlBlock block);
}
