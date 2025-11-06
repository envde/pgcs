using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;

namespace PgCs.Core.Extraction;

/// <summary>
/// Извлекает объект из SqlBlock (ENUM, COMMENT, TABLE, etc..)
/// </summary>
public interface IExtractor<T> where T : DefinitionBase
{
    /// <summary>
    /// Извлекает PostgreSQL объект (ENUM, COMMENT, TABLE, SELECT, etc..) из блока SqlBlock
    /// </summary>
    /// <param name="blocks">
    /// SQL блоки для извлечения объекта. Может быть передано больше одного блока, так как
    /// для некоторых объектов требуется дополнительные сведения из других объектов. Например
    /// для View требуется Table для получения информации о столбцах.
    /// </param>
    /// <returns>Результат извлечения объекта. Тип производный от DefinitionBase</returns>
    ExtractionResult<T> Extract(IReadOnlyList<SqlBlock> blocks);

    /// <summary>
    /// Проверяет, содержит ли блок определение представления
    /// </summary>
    /// <param name="blocks">SQL блоки для проверки</param>
    /// <returns>Возвращает true если возможно извлечь Definition объект</returns>
    bool CanExtract(IReadOnlyList<SqlBlock> blocks);
}
