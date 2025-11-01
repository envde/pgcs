using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор комментариев к индексам из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение комментариев в формате:
/// <code>
/// COMMENT ON INDEX idx_users_email IS 'Index for email lookups';
/// COMMENT ON INDEX public.idx_users_email IS 'Index in public schema';
/// </code>
/// </para>
/// </summary>
public sealed partial class IndexCommentExtractor : IIndexCommentExtractor
{
    // Regex для определения COMMENT ON INDEX
    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+INDEX\s+(?:(?<schema>\w+)\.)?(?<index>\w+)\s+IS\s+'(?<comment>[^']*)'\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex IndexCommentPattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        var content = block.Content;
        return content.Contains("COMMENT ON INDEX", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public IndexCommentDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var match = IndexCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var indexName = match.Groups["index"].Value;
        var comment = match.Groups["comment"].Value;

        return new IndexCommentDefinition
        {
            IndexName = indexName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }
}
