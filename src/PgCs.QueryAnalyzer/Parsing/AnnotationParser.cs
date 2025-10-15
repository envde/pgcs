using PgCs.Common.QueryAnalyzer.Models;
using PgCs.Common.QueryAnalyzer.Models.Enums;

namespace PgCs.QueryAnalyzer.Parsing;

using System.Text.RegularExpressions;

internal static partial class AnnotationParser
{
    private static readonly Regex NameAnnotationRegex = GenerateNameAnnotationRegex();

    /// <summary>
    /// Парсит sqlc аннотации из комментариев
    /// </summary>
    public static QueryAnnotation Parse(string comments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(comments);

        var match = NameAnnotationRegex.Match(comments);
        
        if (!match.Success)
        {
            throw new InvalidOperationException(
                "Не найдена аннотация формата: -- name: QueryName :cardinality");
        }

        var name = match.Groups[1].Value;
        var cardinalityStr = match.Groups[2].Value;

        var cardinality = ParseCardinality(cardinalityStr);

        return new QueryAnnotation
        {
            Name = name,
            Cardinality = cardinality
        };
    }

    /// <summary>
    /// Проверяет, содержит ли текст аннотацию
    /// </summary>
    public static bool HasAnnotation(string text) 
        => NameAnnotationRegex.IsMatch(text);

    private static ReturnCardinality ParseCardinality(string cardinality)
    {
        return cardinality.ToLowerInvariant() switch
        {
            "one" => ReturnCardinality.One,
            "many" => ReturnCardinality.Many,
            "exec" => ReturnCardinality.Exec,
            "execrows" => ReturnCardinality.ExecRows,
            _ => throw new InvalidOperationException(
                $"Неизвестная кардинальность: {cardinality}. " +
                $"Допустимые значения: one, many, exec, execrows")
        };
    }

    [GeneratedRegex(@"--\s*name:\s*(\w+)\s*:(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GenerateNameAnnotationRegex();
}