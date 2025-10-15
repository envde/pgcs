using PgCs.Common.QueryAnalyzer.Models;

namespace PgCs.QueryAnalyzer.Tests.Helpers;

/// <summary>
/// Builder для создания тестовых данных
/// </summary>
public static class TestDataBuilder
{
    public static string BuildQuery(
        string name,
        string cardinality,
        string sqlBody,
        string? comment = null)
    {
        var lines = new List<string>();
        
        if (!string.IsNullOrEmpty(comment))
            lines.Add($"-- {comment}");
        
        lines.Add($"-- name: {name} :{cardinality}");
        lines.Add(sqlBody);
        
        return string.Join(Environment.NewLine, lines);
    }

    public static ReturnColumn CreateColumn(
        string name,
        string postgresType = "text",
        string csharpType = "string",
        bool isNullable = true) => new()
    {
        Name = name,
        PostgresType = postgresType,
        CSharpType = csharpType,
        IsNullable = isNullable
    };
}