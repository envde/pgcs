using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Интерфейс для извлечения определений функций и процедур из SQL блоков
/// </summary>
public interface IFunctionExtractor
{
    /// <summary>
    /// Извлекает определение функции или процедуры из SQL блока
    /// </summary>
    /// <param name="block">SQL блок с определением функции</param>
    /// <returns>Определение функции или null, если блок не содержит CREATE FUNCTION/PROCEDURE</returns>
    FunctionDefinition? Extract(SqlBlock block);
    
    /// <summary>
    /// Проверяет, содержит ли блок определение функции или процедуры
    /// </summary>
    /// <param name="block">SQL блок для проверки</param>
    /// <returns>true, если блок содержит CREATE FUNCTION или CREATE PROCEDURE</returns>
    bool CanExtract(SqlBlock block);
}
