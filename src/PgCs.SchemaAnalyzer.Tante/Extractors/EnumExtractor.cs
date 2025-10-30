using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор определений ENUM типов из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение ENUM типов в формате:
/// <code>
/// CREATE TYPE status AS ENUM ('active', 'inactive', 'pending');
/// CREATE TYPE public.status AS ENUM ('active', 'inactive');
/// </code>
/// </para>
/// </summary>
public sealed partial class EnumExtractor : IEnumExtractor
{
    // Regex для определения CREATE TYPE AS ENUM
    [GeneratedRegex(@"^\s*CREATE\s+TYPE\s+(?:(?<schema>\w+)\.)?(?<name>\w+)\s+AS\s+ENUM\s*\(\s*(?<values>.*?)\s*\)\s*;?\s*$", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled, 
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex EnumPattern();
    
    // Regex для извлечения отдельных значений ENUM (обрабатывает строки с одинарными кавычками)
    [GeneratedRegex(@"'([^']*)'", 
        RegexOptions.Compiled, 
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex EnumValuePattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        // Быстрая проверка по содержимому
        var content = block.Content;
        return content.Contains("CREATE TYPE", StringComparison.OrdinalIgnoreCase) &&
               content.Contains("AS ENUM", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public EnumTypeDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var match = EnumPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var name = match.Groups["name"].Value;
        var valuesText = match.Groups["values"].Value;

        var values = ExtractEnumValues(valuesText);
        
        if (values.Count == 0)
        {
            return null; // ENUM без значений не валиден
        }

        return new EnumTypeDefinition
        {
            Name = name,
            Schema = schema,
            Values = values,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает список значений ENUM из строки формата ('val1', 'val2', 'val3')
    /// </summary>
    /// <param name="valuesText">Текст со значениями ENUM в кавычках</param>
    /// <returns>Список значений ENUM в порядке определения</returns>
    private static IReadOnlyList<string> ExtractEnumValues(string valuesText)
    {
        if (string.IsNullOrWhiteSpace(valuesText))
        {
            return [];
        }

        var matches = EnumValuePattern().Matches(valuesText);
        if (matches.Count == 0)
        {
            return [];
        }

        var values = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                values.Add(match.Groups[1].Value);
            }
        }

        return values;
    }
}
