using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений представлений (VIEW) из SQL блоков
/// </summary>
public interface IViewExtractor
{
    /// <summary>
    /// Извлекает определение представления из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением VIEW</param>
    /// <returns>Определение представления или null, если блок не содержит CREATE VIEW</returns>
    ViewDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение представления
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит CREATE VIEW или CREATE MATERIALIZED VIEW</returns>
    bool CanExtract(SqlBlock block);
}
