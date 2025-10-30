using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений Domain типов из SQL блоков
/// </summary>
public interface IDomainExtractor
{
    /// <summary>
    /// Извлекает определение Domain типа из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением Domain</param>
    /// <returns>Определение Domain типа или null, если блок не содержит CREATE DOMAIN</returns>
    DomainTypeDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение Domain типа
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит CREATE DOMAIN</returns>
    bool CanExtract(SqlBlock block);
}
