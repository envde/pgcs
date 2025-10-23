using System.Text.RegularExpressions;
using PgCs.Common.QueryAnalyzer.Models.Parameters;

namespace PgCs.QueryAnalyzer.Parsing;

/// <summary>
/// Извлекатель параметров из SQL запросов (поддерживает @param и $param синтаксис)
/// </summary>
internal static partial class ParameterExtractor
{
    private static readonly Regex ParameterRegex = GenerateParameterRegex();

    /// <summary>
    /// Извлекает все уникальные параметры из SQL запроса с определением их типов
    /// </summary>
    /// <param name="sqlQuery">SQL запрос для анализа</param>
    /// <returns>Список параметров с метаданными о типах</returns>
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
            
            // Пропускаем дубликаты
            if (!seen.TryAdd(paramName, position))
                continue;

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

    /// <summary>
    /// Regex для поиска параметров в формате @name или $name
    /// </summary>
    [GeneratedRegex(@"[@$](\w+)", RegexOptions.Compiled)]
    private static partial Regex GenerateParameterRegex();
}