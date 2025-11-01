using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор комментариев к колонкам таблиц из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение комментариев в формате:
/// <code>
/// COMMENT ON COLUMN users.email IS 'User email address';
/// COMMENT ON COLUMN public.users.email IS 'Email in public schema';
/// </code>
/// </para>
/// </summary>
public sealed partial class ColumnCommentExtractor : IColumnCommentExtractor
{
    // Regex для определения COMMENT ON COLUMN
    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+COLUMN\s+(?:(?<schema>\w+)\.)?(?<table>\w+)\.(?<column>\w+)\s+IS\s+'(?<comment>[^']*)'\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColumnCommentPattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        var content = block.Content;
        return content.Contains("COMMENT ON COLUMN", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ColumnCommentDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var match = ColumnCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var tableName = match.Groups["table"].Value;
        var columnName = match.Groups["column"].Value;
        var comment = match.Groups["comment"].Value;

        return new ColumnCommentDefinition
        {
            TableName = tableName,
            ColumnName = columnName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }
}
