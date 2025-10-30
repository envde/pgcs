using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений таблиц из SQL блоков
/// </summary>
public interface ITableExtractor
{
    /// <summary>
    /// Извлекает определение таблицы из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением таблицы</param>
    /// <returns>Определение таблицы или null, если блок не содержит CREATE TABLE</returns>
    TableDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение таблицы
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит CREATE TABLE</returns>
    bool CanExtract(SqlBlock block);
}
