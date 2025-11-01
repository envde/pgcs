using System.Text.RegularExpressions;
using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;

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
public sealed partial class EnumExtractor : IExtractor<EnumTypeDefinition>
{
    // Regex для определения CREATE TYPE AS ENUM
    [GeneratedRegex(@"^\s*CREATE\s+TYPE\s+(?:(?<schema>\w+)\.)?(?<name>\w+)\s+AS\s+ENUM\s*\(\s*(?<values>.*?)\s*\)\s*;?\s*$", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled, 
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex EnumPattern();
    
    // Regex для извлечения отдельных значений ENUM (обрабатывает строки с одинарными кавычками)
    [GeneratedRegex(@"'([^']*)'", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex EnumValuePattern();

    /// <inheritdoc />
    public bool CanExtract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        
        // Для ENUM достаточно одного блока
        if (blocks.Count == 0)
        {
            return false;
        }
        
        var block = blocks[0];
        
        // Быстрая проверка по содержимому
        var content = block.Content;
        return content.Contains("CREATE TYPE", StringComparison.OrdinalIgnoreCase) &&
               content.Contains("AS ENUM", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ExtractionResult<EnumTypeDefinition> Extract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        
        if (!CanExtract(blocks))
        {
            return ExtractionResult<EnumTypeDefinition>.NotApplicable();
        }

        var block = blocks[0];
        var issues = new List<ValidationIssue>();
        
        var match = EnumPattern().Match(block.Content);
        
        if (!match.Success)
        {
            return ExtractionResult<EnumTypeDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Enum,
                    "ENUM_PARSE_ERROR",
                    "Failed to parse ENUM type definition",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = block.Content,
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var name = match.Groups["name"].Value;
        var valuesText = match.Groups["values"].Value;

        var values = ExtractEnumValues(valuesText);
        
        if (values.Count == 0)
        {
            return ExtractionResult<EnumTypeDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Enum,
                    "ENUM_NO_VALUES",
                    $"ENUM type '{name}' has no values defined",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = block.Content,
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        // Проверка на дубликаты значений
        var duplicates = values
            .GroupBy(v => v)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        
        if (duplicates.Count > 0)
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Enum,
                "ENUM_DUPLICATE_VALUES",
                $"ENUM type '{name}' contains duplicate values: {string.Join(", ", duplicates)}",
                new ValidationIssue.ValidationLocation
                {
                    Segment = valuesText,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        // Проверка на пустые значения
        if (values.Any(string.IsNullOrWhiteSpace))
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Enum,
                "ENUM_EMPTY_VALUE",
                $"ENUM type '{name}' contains empty or whitespace-only values",
                new ValidationIssue.ValidationLocation
                {
                    Segment = valuesText,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        // Проверка на слишком большое количество значений (более 100 - потенциальная проблема дизайна)
        if (values.Count > 100)
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Enum,
                "ENUM_TOO_MANY_VALUES",
                $"ENUM type '{name}' has {values.Count} values. Consider using a lookup table instead for better maintainability",
                new ValidationIssue.ValidationLocation
                {
                    Segment = block.Content,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        // Проверка на очень длинные значения (более 255 символов)
        var longValues = values.Where(v => v.Length > 255).ToList();
        if (longValues.Count > 0)
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Enum,
                "ENUM_VALUE_TOO_LONG",
                $"ENUM type '{name}' contains values longer than 255 characters, which may cause issues",
                new ValidationIssue.ValidationLocation
                {
                    Segment = valuesText,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        var definition = new EnumTypeDefinition
        {
            Name = name,
            Schema = schema,
            Values = values,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };

        return ExtractionResult<EnumTypeDefinition>.Success(definition, issues);
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
