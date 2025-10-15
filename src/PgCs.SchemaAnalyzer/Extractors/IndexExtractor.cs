using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer;
using PgCs.SchemaAnalyzer.Extractors;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает определения индексов
/// </summary>
internal sealed partial class IndexExtractor : BaseExtractor<IndexDefinition>
{
    [GeneratedRegex(@"CREATE\s+(UNIQUE\s+)?INDEX\s+(?:IF\s+NOT\s+EXISTS\s+)?([a-zA-Z_][a-zA-Z0-9_]*)\s+ON\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s+(?:USING\s+([a-zA-Z]+)\s*)?\((.*?)\)(?:\s+WHERE\s+(.*))?", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex IndexPatternRegex();

    protected override Regex Pattern => IndexPatternRegex();

    protected override IndexDefinition? ParseMatch(Match match, string statement)
    {
        var isUnique = !string.IsNullOrWhiteSpace(match.Groups[1].Value);
        var indexName = match.Groups[2].Value.Trim();
        var fullTableName = match.Groups[3].Value.Trim();
        var method = match.Groups[4].Value.Trim();
        var columnsText = match.Groups[5].Value.Trim();
        var whereClause = match.Groups[6].Value.Trim();

        var schema = ExtractSchemaName(fullTableName);
        var tableName = ExtractTableName(fullTableName);

        var columns = ParseIndexColumns(columnsText);
        var indexMethod = ParseIndexMethod(method);
        var isPartial = !string.IsNullOrWhiteSpace(whereClause);

        return new IndexDefinition
        {
            Name = indexName,
            TableName = tableName,
            Schema = schema,
            Columns = columns,
            Method = indexMethod,
            IsUnique = isUnique,
            IsPartial = isPartial,
            WhereClause = isPartial ? whereClause : null,
            RawSql = statement
        };
    }

    private IReadOnlyList<string> ParseIndexColumns(string columnsText)
    {
        return columnsText
            .Split(',')
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c =>
            {
                // Убираем возможные модификаторы (DESC, ASC, NULLS FIRST и т.д.)
                var tokens = c.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return tokens[0];
            })
            .ToArray();
    }

    private static IndexMethod ParseIndexMethod(string method)
    {
        if (string.IsNullOrWhiteSpace(method))
            return IndexMethod.BTree;

        return method.ToUpperInvariant() switch
        {
            "BTREE" => IndexMethod.BTree,
            "HASH" => IndexMethod.Hash,
            "GIST" => IndexMethod.Gist,
            "GIN" => IndexMethod.Gin,
            "SPGIST" => IndexMethod.SpGist,
            "BRIN" => IndexMethod.Brin,
            _ => IndexMethod.BTree
        };
    }
}