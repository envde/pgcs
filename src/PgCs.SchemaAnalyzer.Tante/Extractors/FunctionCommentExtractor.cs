using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор комментариев к функциям из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение комментариев в формате:
/// <code>
/// COMMENT ON FUNCTION calculate_total() IS 'Calculates the total';
/// COMMENT ON FUNCTION public.calculate_total(integer, text) IS 'Function with parameters';
/// </code>
/// </para>
/// </summary>
public sealed partial class FunctionCommentExtractor : IFunctionCommentExtractor
{
    // Regex для определения COMMENT ON FUNCTION
    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+FUNCTION\s+(?:(?<schema>\w+)\.)?(?<function>\w+)\s*\([^)]*\)\s+IS\s+'(?<comment>[^']*)'\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex FunctionCommentPattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        var content = block.Content;
        return content.Contains("COMMENT ON FUNCTION", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public FunctionCommentDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var match = FunctionCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var functionName = match.Groups["function"].Value;
        var comment = match.Groups["comment"].Value;

        return new FunctionCommentDefinition
        {
            FunctionName = functionName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }
}
