using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор комментариев к ограничениям из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение комментариев в формате:
/// <code>
/// COMMENT ON CONSTRAINT pk_users ON users IS 'Primary key constraint';
/// COMMENT ON CONSTRAINT fk_orders_user ON public.orders IS 'Foreign key to users';
/// </code>
/// </para>
/// </summary>
public sealed partial class ConstraintCommentExtractor : IConstraintCommentExtractor
{
    // Regex для определения COMMENT ON CONSTRAINT
    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+CONSTRAINT\s+(?<constraint>\w+)\s+ON\s+(?:(?<schema>\w+)\.)?(?<table>\w+)\s+IS\s+'(?<comment>[^']*)'\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ConstraintCommentPattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        var content = block.Content;
        return content.Contains("COMMENT ON CONSTRAINT", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ConstraintCommentDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var match = ConstraintCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var constraintName = match.Groups["constraint"].Value;
        var comment = match.Groups["comment"].Value;

        return new ConstraintCommentDefinition
        {
            ConstraintName = constraintName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }
}
