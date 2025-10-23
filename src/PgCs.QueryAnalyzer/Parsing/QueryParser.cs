using PgCs.Common.QueryAnalyzer.Models.Metadata;

namespace PgCs.QueryAnalyzer.Parsing;

/// <summary>
/// Парсер SQL запросов для разделения на части и определения типа
/// </summary>
internal static class SqlQueryParser
{
    /// <summary>
    /// Разделяет SQL текст на комментарии (аннотации) и сам запрос
    /// </summary>
    /// <param name="sqlQuery">Исходный SQL текст</param>
    /// <returns>Кортеж (комментарии, запрос)</returns>
    public static (string Comments, string Query) SplitCommentsAndQuery(ReadOnlySpan<char> sqlQuery)
    {
        Span<Range> lineRanges = stackalloc Range[128];
        var lineCount = sqlQuery.Split(lineRanges, '\n', StringSplitOptions.None);
        
        var commentLines = new List<string>(capacity: 4);
        var queryLines = new List<string>(capacity: 16);
        var isInQuery = false;

        for (var i = 0; i < lineCount; i++)
        {
            var line = sqlQuery[lineRanges[i]];
            var trimmed = line.Trim();

            // Строки начинающиеся с -- это комментарии
            if (trimmed.StartsWith("--"))
            {
                if (!isInQuery)
                    commentLines.Add(trimmed.ToString());
            }
            else if (!trimmed.IsEmpty && !trimmed.IsWhiteSpace())
            {
                isInQuery = true;
                queryLines.Add(line.ToString());
            }
        }

        return (
            Comments: string.Join('\n', commentLines),
            Query: string.Join('\n', queryLines).Trim()
        );
    }

    /// <summary>
    /// Определяет тип SQL запроса по первому ключевому слову
    /// </summary>
    /// <param name="sqlQuery">SQL запрос для анализа</param>
    /// <returns>Тип запроса (SELECT, INSERT, UPDATE, DELETE, Unknown)</returns>
    public static QueryType DetermineQueryType(string sqlQuery)
    {
        var normalized = sqlQuery.AsSpan().Trim();
        
        if (normalized.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) || 
            normalized.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            return QueryType.Select;
        
        if (normalized.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
            return QueryType.Insert;
        
        if (normalized.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
            return QueryType.Update;
        
        if (normalized.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
            return QueryType.Delete;
        
        return QueryType.Unknown;
    }

    /// <summary>
    /// Разбивает многострочный SQL файл на отдельные блоки запросов (по точке с запятой)
    /// </summary>
    /// <param name="content">Содержимое SQL файла</param>
    /// <returns>Коллекция SQL блоков</returns>
    public static IEnumerable<string> SplitIntoQueryBlocks(string content)
    {
        var lines = content.Split('\n');
        var currentBlock = new List<string>(capacity: 32);

        foreach (var line in lines)
        {
            currentBlock.Add(line);

            // Точка с запятой обозначает конец блока
            if (line.TrimEnd().EndsWith(';'))
            {
                yield return string.Join('\n', currentBlock);
                currentBlock.Clear();
            }
        }

        // Последний блок без точки с запятой
        if (currentBlock.Count > 0)
        {
            yield return string.Join('\n', currentBlock);
        }
    }
}