using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор комментариев к композитным типам из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение комментариев в формате:
/// <code>
/// COMMENT ON TYPE address IS 'Address composite type';
/// COMMENT ON TYPE public.address IS 'Type in public schema';
/// </code>
/// </para>
/// </summary>
public sealed partial class CompositeTypeCommentExtractor : ICompositeTypeCommentExtractor
{
    // Regex для определения COMMENT ON TYPE
    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+TYPE\s+(?:(?<schema>\w+)\.)?(?<type>\w+)\s+IS\s+'(?<comment>[^']*)'\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeCommentPattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        var content = block.Content;
        return content.Contains("COMMENT ON TYPE", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public CompositeTypeCommentDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var match = TypeCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var typeName = match.Groups["type"].Value;
        var comment = match.Groups["comment"].Value;

        return new CompositeTypeCommentDefinition
        {
            CompositeTypeName = typeName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }
}
