using PgCs.Common.QueryAnalyzer.Models;

namespace PgCs.QueryAnalyzer.Parsing;

using System.Text.RegularExpressions;
using PgCs.Common.QueryAnalyzer;

internal static partial class ParameterExtractor
{
    private static readonly Regex ParameterRegex = GenerateParameterRegex();

    /// <summary>
    /// Извлекает все параметры из SQL запроса
    /// </summary>
    public static IReadOnlyList<QueryParameter> Extract(string sqlQuery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlQuery);

        var parameters = new List<QueryParameter>();
        var matches = ParameterRegex.Matches(sqlQuery);
        var seen = new Dictionary<string, int>();
        var position = 1;

        foreach (Match match in matches)
        {
            var paramName = match.Groups[1].Value;
            
            if (seen.ContainsKey(paramName))
                continue;

            seen[paramName] = position;
            
            var (postgresType, csharpType, isNullable) = TypeInference.InferParameterType(sqlQuery, paramName);
            
            parameters.Add(new QueryParameter
            {
                Name = paramName,
                PostgresType = postgresType,
                CSharpType = csharpType,
                IsNullable = isNullable,
                Position = position
            });
            
            position++;
        }

        return parameters;
    }

    [GeneratedRegex(@"[@$](\w+)", RegexOptions.Compiled)]
    private static partial Regex GenerateParameterRegex();
}