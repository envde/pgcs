using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор определений Domain типов из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение Domain типов в формате:
/// <code>
/// CREATE DOMAIN email AS VARCHAR(255)
///     CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');
/// 
/// CREATE DOMAIN positive_numeric AS NUMERIC(12, 2)
///     DEFAULT 0
///     NOT NULL
///     CHECK (VALUE >= 0);
/// </code>
/// </para>
/// </summary>
public sealed partial class DomainExtractor : IDomainExtractor
{
    // Regex для определения CREATE DOMAIN
    [GeneratedRegex(@"^\s*CREATE\s+DOMAIN\s+(?:(?<schema>\w+)\.)?(?<name>\w+)\s+AS\s+(?<baseType>\w+)(?:\((?<params>[\d,\s]+)\))?\s*(?<rest>.*?)\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex DomainPattern();

    // Regex для извлечения DEFAULT значения
    [GeneratedRegex(@"\bDEFAULT\s+(.+?)(?:\s+NOT\s+NULL|\s+CHECK|;|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 500)]
    private static partial Regex DefaultPattern();

    // Regex для извлечения CHECK ограничений
    [GeneratedRegex(@"\bCHECK\s*\(([^)]+)\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 500)]
    private static partial Regex CheckPattern();

    // Regex для извлечения COLLATE
    [GeneratedRegex(@"\bCOLLATE\s+([""']?)(\w+)\1",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 500)]
    private static partial Regex CollatePattern();

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        return block.Content.Contains("CREATE DOMAIN", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public DomainTypeDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (!CanExtract(block))
        {
            return null;
        }

        var match = DomainPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var name = match.Groups["name"].Value;
        var baseType = match.Groups["baseType"].Value.Trim();
        var paramsGroup = match.Groups["params"];
        var restText = match.Groups["rest"].Value;

        // Обработка параметров базового типа
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

            if (IsStringType(baseType))
            {
                maxLength = parameters.Length > 0 ? parameters[0] : null;
            }
            else if (IsNumericType(baseType))
            {
                if (parameters.Length > 0)
                {
                    numericPrecision = parameters[0];
                }
                if (parameters.Length > 1)
                {
                    numericScale = parameters[1];
                }
            }

            // Добавляем параметры к baseType для полного представления
            baseType = $"{baseType}({paramsGroup.Value})";
        }

        // Извлечение DEFAULT значения
        var defaultValue = ExtractDefault(restText);

        // Проверка NOT NULL
        var isNotNull = restText.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase);

        // Извлечение CHECK ограничений
        var checkConstraints = ExtractCheckConstraints(restText);

        // Извлечение COLLATE
        var collation = ExtractCollation(restText);

        return new DomainTypeDefinition
        {
            Name = name,
            Schema = schema,
            BaseType = baseType,
            DefaultValue = defaultValue,
            IsNotNull = isNotNull,
            CheckConstraints = checkConstraints,
            MaxLength = maxLength,
            NumericPrecision = numericPrecision,
            NumericScale = numericScale,
            Collation = collation,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает DEFAULT значение
    /// </summary>
    private static string? ExtractDefault(string text)
    {
        var match = DefaultPattern().Match(text);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }
        return null;
    }

    /// <summary>
    /// Извлекает список CHECK ограничений
    /// </summary>
    private static IReadOnlyList<string> ExtractCheckConstraints(string text)
    {
        var matches = CheckPattern().Matches(text);
        if (matches.Count == 0)
        {
            return [];
        }

        var constraints = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                constraints.Add(match.Groups[1].Value.Trim());
            }
        }

        return constraints;
    }

    /// <summary>
    /// Извлекает COLLATE значение
    /// </summary>
    private static string? ExtractCollation(string text)
    {
        var match = CollatePattern().Match(text);
        if (match.Success && match.Groups.Count > 2)
        {
            return match.Groups[2].Value;
        }
        return null;
    }

    /// <summary>
    /// Проверяет, является ли тип строковым
    /// </summary>
    private static bool IsStringType(string dataType)
    {
        return dataType.Equals("VARCHAR", StringComparison.OrdinalIgnoreCase) ||
               dataType.Equals("CHAR", StringComparison.OrdinalIgnoreCase) ||
               dataType.Equals("CHARACTER", StringComparison.OrdinalIgnoreCase) ||
               dataType.Equals("CHARACTER VARYING", StringComparison.OrdinalIgnoreCase) ||
               dataType.Equals("TEXT", StringComparison.OrdinalIgnoreCase);
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
