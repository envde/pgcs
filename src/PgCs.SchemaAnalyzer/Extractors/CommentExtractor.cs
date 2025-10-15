using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает комментарии (COMMENT ON)
/// </summary>
internal sealed partial class CommentExtractor
{
    [GeneratedRegex(@"COMMENT\s+ON\s+(TABLE|COLUMN|VIEW|FUNCTION|TYPE|INDEX|TRIGGER)\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s+IS\s+'(.*?)'(?<!\\)'", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex CommentPatternRegex();

    public IReadOnlyDictionary<string, string>? ExtractComments(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            return null;

        var comments = new Dictionary<string, string>();
        var matches = CommentPatternRegex().Matches(sqlScript);

        foreach (Match match in matches)
        {
            var objectType = match.Groups[1].Value;
            var objectName = match.Groups[2].Value;
            var comment = match.Groups[3].Value;

            var key = $"{objectType}.{objectName}";
            comments[key] = comment;
        }

        return comments.Count > 0 ? comments.ToFrozenDictionary() : null;
    }
}