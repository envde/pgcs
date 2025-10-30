namespace PgCs.Core.Extraction.Block;

/// <summary>
/// Интерфейс для извлечения SQL блоков из текста.
/// </summary>
public interface IBlockExtractor
{
    /// <summary>
    /// Извлекает отдельные SQL блоки (команды) из текста.
    /// Блоки разделяются точками с запятой.
    /// </summary>
    /// <param name="sql">SQL текст для парсинга</param>
    /// <returns>Список извлеченных SQL блоков</returns>
    IReadOnlyList<SqlBlock> Extract(string sql);
}
