using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;

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
public sealed partial class CompositeExtractor : ICompositeExtractor
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
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        var content = block.Content;
        return content.Contains("CREATE TYPE", StringComparison.OrdinalIgnoreCase) &&
               content.Contains(" AS ", StringComparison.OrdinalIgnoreCase) &&
               content.Contains('(') &&
               !content.Contains("AS ENUM", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public CompositeTypeDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (!CanExtract(block))
        {
            return null;
        }

        var match = CompositePattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var name = match.Groups["name"].Value;
        var attributesText = match.Groups["attributes"].Value;

        var attributes = ExtractAttributes(attributesText);

        if (attributes.Count == 0)
        {
            return null; // Composite без атрибутов не валиден
        }

        return new CompositeTypeDefinition
        {
            Name = name,
            Schema = schema,
            Attributes = attributes,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает список атрибутов Composite типа
    /// </summary>
    /// <param name="attributesText">Текст с определениями атрибутов</param>
    /// <returns>Список атрибутов Composite типа</returns>
    private static IReadOnlyList<CompositeTypeAttribute> ExtractAttributes(string attributesText)
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
