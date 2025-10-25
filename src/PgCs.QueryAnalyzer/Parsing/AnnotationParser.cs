using System.Text.RegularExpressions;
using PgCs.Common.QueryAnalyzer.Models.Annotations;
using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.QueryAnalyzer.Parsing;

internal static partial class AnnotationParser
{
    private static readonly Regex NameAnnotationRegex = GenerateNameAnnotationRegex();
    private static readonly Regex SummaryRegex = GenerateSummaryRegex();
    private static readonly Regex ParamRegex = GenerateParamRegex();
    private static readonly Regex ReturnsRegex = GenerateReturnsRegex();

    public static QueryAnnotation Parse(string comments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(comments);
        var match = NameAnnotationRegex.Match(comments);
        if (!match.Success)
        {
            throw new InvalidOperationException("Not found annotation: -- name: QueryName :cardinality");
        }
        var name = match.Groups[1].Value;
        var cardinalityStr = match.Groups[2].Value;
        var cardinality = ParseCardinality(cardinalityStr);
        var summary = ParseSummary(comments);
        var paramDescriptions = ParseParameterDescriptions(comments);
        var returns = ParseReturns(comments);
        return new QueryAnnotation
        {
            Name = name,
            Cardinality = cardinality,
            Summary = summary,
            ParameterDescriptions = paramDescriptions,
            Returns = returns
        };
    }

    public static bool HasAnnotation(string text) => NameAnnotationRegex.IsMatch(text);

    private static ReturnCardinality ParseCardinality(string cardinality)
    {
        return cardinality.ToLowerInvariant() switch
        {
            "one" => ReturnCardinality.One,
            "many" => ReturnCardinality.Many,
            "exec" => ReturnCardinality.Exec,
            "execrows" => ReturnCardinality.ExecRows,
            _ => throw new InvalidOperationException($"Unknown cardinality: {cardinality}")
        };
    }

    private static string? ParseSummary(string comments)
    {
        var match = SummaryRegex.Match(comments);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static IReadOnlyDictionary<string, string> ParseParameterDescriptions(string comments)
    {
        var descriptions = new Dictionary<string, string>();
        var matches = ParamRegex.Matches(comments);
        foreach (Match match in matches)
        {
            descriptions[match.Groups[1].Value.Trim()] = match.Groups[2].Value.Trim();
        }
        return descriptions;
    }

    private static string? ParseReturns(string comments)
    {
        var match = ReturnsRegex.Match(comments);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    [GeneratedRegex(@"--\s*name:\s*(\w+)\s*:(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GenerateNameAnnotationRegex();

    [GeneratedRegex(@"--\s*summary:\s*(.+?)(?=\n\s*(?:--\s*(?:param|returns|name):|SELECT|INSERT|UPDATE|DELETE|$))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex GenerateSummaryRegex();

    [GeneratedRegex(@"--\s*param:\s*(\w+)\s+(.+?)(?=\n|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GenerateParamRegex();

    [GeneratedRegex(@"--\s*returns:\s*(.+?)(?=\n|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GenerateReturnsRegex();
}
