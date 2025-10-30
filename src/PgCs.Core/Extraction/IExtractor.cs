using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;

namespace PgCs.Core.Extraction;

/// <summary>
/// Извлекает объект из SqlBlock (ENUM, COMMENT, TABLE, etc..)
/// </summary>
public interface IExtractor<T> where T: DefinitionBase
{
    /// <summary>
    /// Извлекает PostgreSQL объект (ENUM, COMMENT, TABLE, etc..) из блока SqlBlock
    /// </summary>
    /// <param name="block">SQL блок для извлечения объекта</param>
    /// <returns>Результат извлечения объекта. Тип производный от DefinitionBase</returns>
    ExtractionResult<T> Extract(SqlBlock block);
}
