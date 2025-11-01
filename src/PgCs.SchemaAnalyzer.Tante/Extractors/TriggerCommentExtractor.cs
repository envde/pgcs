using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор комментариев к триггерам из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение комментариев в формате:
/// <code>
/// COMMENT ON TRIGGER update_timestamp ON users IS 'Updates timestamp on modification';
/// COMMENT ON TRIGGER update_timestamp ON public.users IS 'Trigger in public schema';
/// </code>
/// </para>
/// </summary>
public sealed partial class TriggerCommentExtractor : ITriggerCommentExtractor
{
    // Regex для определения COMMENT ON TRIGGER
    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+TRIGGER\s+(?<trigger>\w+)\s+ON\s+(?:(?<schema>\w+)\.)?(?<table>\w+)\s+IS\s+'(?<comment>[^']*)'\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex TriggerCommentPattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        var content = block.Content;
        return content.Contains("COMMENT ON TRIGGER", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public TriggerCommentDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var match = TriggerCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var triggerName = match.Groups["trigger"].Value;
        var comment = match.Groups["comment"].Value;

        return new TriggerCommentDefinition
        {
            TriggerName = triggerName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }
}
