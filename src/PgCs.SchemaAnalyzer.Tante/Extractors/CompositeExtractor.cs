using System.Text;
using System.Text.RegularExpressions;
using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор определений Composite типов из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение Composite типов в формате:
/// <code>
/// CREATE TYPE address AS (
///     street VARCHAR(255),
///     city VARCHAR(100),
///     zip_code VARCHAR(20)
/// );
/// </code>
/// </para>
/// </summary>
public sealed partial class CompositeExtractor : IExtractor<CompositeTypeDefinition>
{
    // Regex для определения CREATE TYPE AS (...)
    [GeneratedRegex(@"^\s*CREATE\s+TYPE\s+(?:(?<schema>\w+)\.)?(?<name>\w+)\s+AS\s*\(\s*(?<attributes>.*?)\s*\)\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex CompositePattern();

    // Regex для извлечения отдельного атрибута: name TYPE [constraints]
    [GeneratedRegex(@"^\s*(?<name>\w+)\s+(?<type>\w+)(?:\((?<params>[\d,\s]+)\))?(?<array>\[\])?\s*(?<constraints>.*?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 500)]
    private static partial Regex AttributePattern();

    /// <inheritdoc />
    public bool CanExtract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        
        // Для Composite типа достаточно одного блока
        if (blocks.Count == 0)
        {
            return false;
        }
        
        var block = blocks[0];
        var content = block.Content;
        return content.Contains("CREATE TYPE", StringComparison.OrdinalIgnoreCase) &&
               content.Contains(" AS ", StringComparison.OrdinalIgnoreCase) &&
               content.Contains('(') &&
               !content.Contains("AS ENUM", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ExtractionResult<CompositeTypeDefinition> Extract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (!CanExtract(blocks))
        {
            return ExtractionResult<CompositeTypeDefinition>.NotApplicable();
        }

        var block = blocks[0];
        var issues = new List<ValidationIssue>();

        var match = CompositePattern().Match(block.Content);
        if (!match.Success)
        {
            return ExtractionResult<CompositeTypeDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Composite,
                    "COMPOSITE_PARSE_ERROR",
                    "Failed to parse COMPOSITE type definition. Expected format: CREATE TYPE [schema.]name AS (attr1 type1, attr2 type2, ...)",
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
        var attributesText = match.Groups["attributes"].Value;

        var attributes = ExtractAttributes(attributesText, name, block, issues);

        if (attributes.Count == 0)
        {
            return ExtractionResult<CompositeTypeDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Composite,
                    "COMPOSITE_EMPTY_ATTRIBUTES",
                    $"COMPOSITE type '{name}' has no valid attributes defined",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = attributesText,
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        // Проверка на дубликаты атрибутов
        var duplicates = attributes
            .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Composite,
                "COMPOSITE_DUPLICATE_ATTRIBUTE",
                $"COMPOSITE type '{name}' contains duplicate attribute names: {string.Join(", ", duplicates)}",
                new ValidationIssue.ValidationLocation
                {
                    Segment = attributesText,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        var definition = new CompositeTypeDefinition
        {
            Name = name,
            Schema = schema,
            Attributes = attributes,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };

        return ExtractionResult<CompositeTypeDefinition>.Success(definition, issues);
    }

    /// <summary>
    /// Извлекает список атрибутов Composite типа
    /// </summary>
    /// <param name="attributesText">Текст с определениями атрибутов</param>
    /// <param name="typeName">Имя Composite типа для сообщений об ошибках</param>
    /// <param name="block">SQL блок для информации о местоположении ошибок</param>
    /// <param name="issues">Список ValidationIssue для добавления предупреждений и ошибок</param>
    /// <returns>Список атрибутов Composite типа</returns>
    private static IReadOnlyList<CompositeTypeAttribute> ExtractAttributes(
        string attributesText, 
        string typeName, 
        SqlBlock block, 
        List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(attributesText))
        {
            return [];
        }

        // Разделяем атрибуты по запятым, но игнорируем запятые внутри скобок
        var attributeLines = SplitAttributes(attributesText);

        if (attributeLines.Count == 0)
        {
            return [];
        }

        var attributes = new List<CompositeTypeAttribute>(attributeLines.Count);

        foreach (var line in attributeLines)
        {
            var attribute = ParseAttribute(line);
            if (attribute is not null)
            {
                attributes.Add(attribute);
            }
            else
            {
                // Добавляем ошибку для невалидного атрибута
                issues.Add(ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Composite,
                    "COMPOSITE_INVALID_ATTRIBUTE",
                    $"Failed to parse attribute definition in COMPOSITE type '{typeName}': '{line.Trim()}'",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = line,
                        Line = block.StartLine,
                        Column = 0
                    }
                ));
            }
        }

        return attributes;
    }

    /// <summary>
    /// Разделяет строку с атрибутами по запятым, игнорируя запятые внутри скобок
    /// </summary>
    private static List<string> SplitAttributes(string text)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;

        foreach (var ch in text)
        {
            if (ch == '(')
            {
                depth++;
                current.Append(ch);
            }
            else if (ch == ')')
            {
                depth--;
                current.Append(ch);
            }
            else if (ch == ',' && depth == 0)
            {
                var attribute = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(attribute))
                {
                    result.Add(attribute);
                }
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        // Добавляем последний атрибут
        var lastAttribute = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(lastAttribute))
        {
            result.Add(lastAttribute);
        }

        return result;
    }

    /// <summary>
    /// Парсит отдельный атрибут Composite типа
    /// </summary>
    private static CompositeTypeAttribute? ParseAttribute(string attributeLine)
    {
        var match = AttributePattern().Match(attributeLine);
        if (!match.Success)
        {
            return null;
        }

        var name = match.Groups["name"].Value;
        var dataType = match.Groups["type"].Value;
        var paramsGroup = match.Groups["params"];
        var isArray = match.Groups["array"].Success;

        // Обработка параметров типа (длина, точность, масштаб)
        int? maxLength = null;
        int? numericPrecision = null;
        int? numericScale = null;

        if (paramsGroup.Success && !string.IsNullOrWhiteSpace(paramsGroup.Value))
        {
            var parameters = paramsGroup.Value
                .Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => int.TryParse(p, out var val) ? val : (int?)null)
                .Where(p => p.HasValue)
                .Select(p => p!.Value)
                .ToArray();

            if (IsStringType(dataType))
            {
                // Для строковых типов первый параметр - это длина
                maxLength = parameters.Length > 0 ? parameters[0] : null;
            }
            else if (IsNumericType(dataType))
            {
                // Для числовых типов: precision, scale
                if (parameters.Length > 0)
                {
                    numericPrecision = parameters[0];
                }
                if (parameters.Length > 1)
                {
                    numericScale = parameters[1];
                }
            }
        }

        return new CompositeTypeAttribute
        {
            Name = name,
            DataType = dataType,
            MaxLength = maxLength,
            NumericPrecision = numericPrecision,
            NumericScale = numericScale,
            IsArray = isArray
        };
    }

    /// <summary>
    /// Проверяет, является ли тип строковым
    /// </summary>
    private static bool IsStringType(string dataType)
    {
        return dataType.Equals("VARCHAR", StringComparison.OrdinalIgnoreCase) ||
               dataType.Equals("CHAR", StringComparison.OrdinalIgnoreCase) ||
               dataType.Equals("CHARACTER", StringComparison.OrdinalIgnoreCase) ||
               dataType.Equals("CHARACTER VARYING", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Проверяет, является ли тип числовым с precision/scale
    /// </summary>
    private static bool IsNumericType(string dataType)
    {
        return dataType.Equals("NUMERIC", StringComparison.OrdinalIgnoreCase) ||
               dataType.Equals("DECIMAL", StringComparison.OrdinalIgnoreCase);
    }
}
