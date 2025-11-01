using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор комментариев к таблицам из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение комментариев в формате:
/// <code>
/// COMMENT ON TABLE users IS 'User accounts table';
/// COMMENT ON TABLE public.users IS 'User accounts in public schema';
/// </code>
/// </para>
/// </summary>
public sealed partial class TableCommentExtractor : ITableCommentExtractor
{
    // Regex для определения COMMENT ON TABLE
    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+TABLE\s+(?:(?<schema>\w+)\.)?(?<table>\w+)\s+IS\s+'(?<comment>[^']*)'\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex TableCommentPattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        var content = block.Content;
        return content.Contains("COMMENT ON TABLE", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public TableCommentDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var match = TableCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var tableName = match.Groups["table"].Value;
        var comment = match.Groups["comment"].Value;

        return new TableCommentDefinition
        {
            TableName = tableName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }
}
