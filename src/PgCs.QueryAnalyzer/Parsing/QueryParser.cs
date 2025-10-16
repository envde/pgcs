using PgCs.Common.QueryAnalyzer.Models.Metadata;

namespace PgCs.QueryAnalyzer.Parsing;

internal static class SqlQueryParser
{
    /// <summary>
    /// Разделяет SQL на комментарии и сам запрос
    /// </summary>
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
    /// Определяет тип SQL запроса
    /// </summary>
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
    /// Разбивает файл на отдельные SQL блоки
    /// </summary>
    public static IEnumerable<string> SplitIntoQueryBlocks(string content)
    {
        var lines = content.Split('\n');
        var currentBlock = new List<string>(capacity: 32);

        foreach (var line in lines)
        {
            currentBlock.Add(line);

            if (line.TrimEnd().EndsWith(';'))
            {
                yield return string.Join('\n', currentBlock);
                currentBlock.Clear();
            }
        }

        if (currentBlock.Count > 0)
        {
            yield return string.Join('\n', currentBlock);
        }
    }
}