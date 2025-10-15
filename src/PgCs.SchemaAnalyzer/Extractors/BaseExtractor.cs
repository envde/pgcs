using System.Text.RegularExpressions;
using PgCs.SchemaAnalyzer.Utils;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Базовый класс для всех экстракторов
/// </summary>
internal abstract class BaseExtractor<T>
{
    protected abstract Regex Pattern { get; }

    public virtual IReadOnlyList<T> Extract(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            return [];

        var statements = SqlStatementSplitter.Split(sqlScript);
        var results = new List<T>();

        foreach (var statement in statements)
        {
            var match = Pattern.Match(statement);
            if (match.Success)
            {
                var item = ParseMatch(match, statement);
                if (item is not null)
                {
                    results.Add(item);
                }
            }
        }

        return results;
    }

    protected abstract T? ParseMatch(Match match, string statement);

    protected static string? ExtractSchemaName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts.Length > 1 ? parts[0].Trim('"') : null;
    }

    protected static string ExtractTableName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts.Length > 1 ? parts[1].Trim('"') : parts[0].Trim('"');
    }
}