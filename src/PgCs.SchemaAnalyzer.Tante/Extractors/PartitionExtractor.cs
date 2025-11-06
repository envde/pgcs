using System.Text.RegularExpressions;
using PgCs.Core.Extraction;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор определений партиций таблиц из SQL блоков PostgreSQL.
/// <para>
/// Поддерживает извлечение партиций в формате:
/// <code>
/// CREATE TABLE sales_2023 PARTITION OF sales_data FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');
/// CREATE TABLE sales_active PARTITION OF sales_data FOR VALUES IN ('active', 'pending');
/// CREATE TABLE sales_p0 PARTITION OF sales_data FOR VALUES WITH (MODULUS 4, REMAINDER 0);
/// CREATE TABLE sales_default PARTITION OF sales_data DEFAULT;
/// </code>
/// </para>
/// </summary>
public sealed partial class PartitionExtractor : IExtractor<PartitionDefinition>
{
    // Regex для RANGE партиций: CREATE TABLE ... PARTITION OF ... FOR VALUES FROM (...) TO (...)
    [GeneratedRegex(
        @"^\s*CREATE\s+TABLE\s+(?:(?<schema>\w+)\.)?(?<name>\w+)\s+PARTITION\s+OF\s+(?:(?<parent_schema>\w+)\.)?(?<parent>\w+)\s+FOR\s+VALUES\s+FROM\s*\(\s*(?<from>[^)]+)\s*\)\s+TO\s*\(\s*(?<to>[^)]+)\s*\)(?:\s+TABLESPACE\s+(?<tablespace>\w+))?\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex RangePartitionPattern();

    // Regex для LIST партиций: CREATE TABLE ... PARTITION OF ... FOR VALUES IN (...)
    [GeneratedRegex(
        @"^\s*CREATE\s+TABLE\s+(?:(?<schema>\w+)\.)?(?<name>\w+)\s+PARTITION\s+OF\s+(?:(?<parent_schema>\w+)\.)?(?<parent>\w+)\s+FOR\s+VALUES\s+IN\s*\(\s*(?<values>[^)]*)\s*\)(?:\s+TABLESPACE\s+(?<tablespace>\w+))?\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ListPartitionPattern();

    // Regex для HASH партиций: CREATE TABLE ... PARTITION OF ... FOR VALUES WITH (MODULUS ..., REMAINDER ...)
    [GeneratedRegex(
        @"^\s*CREATE\s+TABLE\s+(?:(?<schema>\w+)\.)?(?<name>\w+)\s+PARTITION\s+OF\s+(?:(?<parent_schema>\w+)\.)?(?<parent>\w+)\s+FOR\s+VALUES\s+WITH\s*\(\s*MODULUS\s+(?<modulus>\d+)\s*,\s*REMAINDER\s+(?<remainder>\d+)\s*\)(?:\s+TABLESPACE\s+(?<tablespace>\w+))?\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex HashPartitionPattern();

    // Regex для DEFAULT партиций: CREATE TABLE ... PARTITION OF ... DEFAULT
    [GeneratedRegex(
        @"^\s*CREATE\s+TABLE\s+(?:(?<schema>\w+)\.)?(?<name>\w+)\s+PARTITION\s+OF\s+(?:(?<parent_schema>\w+)\.)?(?<parent>\w+)\s+DEFAULT(?:\s+TABLESPACE\s+(?<tablespace>\w+))?\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex DefaultPartitionPattern();

    // Regex для извлечения значений из списка (для LIST партиций)
    [GeneratedRegex(@"'([^']*)'", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ListValuePattern();

    /// <inheritdoc />
    public bool CanExtract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (blocks.Count == 0)
        {
            return false;
        }

        var block = blocks[0];
        var content = block.Content;

        // Быстрая проверка: должно содержать CREATE TABLE и PARTITION OF
        return content.Contains("CREATE TABLE", StringComparison.OrdinalIgnoreCase) &&
               content.Contains("PARTITION OF", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ExtractionResult<PartitionDefinition> Extract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (!CanExtract(blocks))
        {
            return ExtractionResult<PartitionDefinition>.NotApplicable();
        }

        var block = blocks[0];
        var issues = new List<ValidationIssue>();

        // Пытаемся определить тип партиции и извлечь данные
        var rangeMatch = RangePartitionPattern().Match(block.Content);
        if (rangeMatch.Success)
        {
            return ExtractRangePartition(rangeMatch, block, issues);
        }

        var listMatch = ListPartitionPattern().Match(block.Content);
        if (listMatch.Success)
        {
            return ExtractListPartition(listMatch, block, issues);
        }

        var hashMatch = HashPartitionPattern().Match(block.Content);
        if (hashMatch.Success)
        {
            return ExtractHashPartition(hashMatch, block, issues);
        }

        var defaultMatch = DefaultPartitionPattern().Match(block.Content);
        if (defaultMatch.Success)
        {
            return ExtractDefaultPartition(defaultMatch, block, issues);
        }

        // Не удалось распознать формат партиции
        return ExtractionResult<PartitionDefinition>.Failure([
            ValidationIssue.Error(
                ValidationIssue.ValidationDefinitionType.Partition,
                "PARTITION_PARSE_ERROR",
                "Failed to parse partition definition. Unsupported partition format or syntax error",
                new ValidationIssue.ValidationLocation
                {
                    Segment = block.Content,
                    Line = block.StartLine,
                    Column = 0
                }
            )
        ]);
    }

    /// <summary>
    /// Извлекает RANGE партицию
    /// </summary>
    private static ExtractionResult<PartitionDefinition> ExtractRangePartition(
        Match match,
        SqlBlock block,
        List<ValidationIssue> issues)
    {
        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var name = match.Groups["name"].Value;
        var parentTableName = match.Groups["parent"].Value;
        var fromValue = match.Groups["from"].Value.Trim();
        var toValue = match.Groups["to"].Value.Trim();
        var tablespace = match.Groups["tablespace"].Success ? match.Groups["tablespace"].Value : null;

        // Валидация: FROM должно быть меньше TO
        if (string.Equals(fromValue, toValue, StringComparison.Ordinal))
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Partition,
                "PARTITION_RANGE_EQUAL",
                $"RANGE partition '{name}' has equal FROM and TO values: {fromValue}",
                new ValidationIssue.ValidationLocation
                {
                    Segment = $"FROM ({fromValue}) TO ({toValue})",
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        var definition = new PartitionDefinition
        {
            Name = name,
            Schema = schema,
            ParentTableName = parentTableName,
            Strategy = PartitionStrategy.Range,
            FromValue = fromValue,
            ToValue = toValue,
            Tablespace = tablespace,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };

        return ExtractionResult<PartitionDefinition>.Success(definition, issues);
    }

    /// <summary>
    /// Извлекает LIST партицию
    /// </summary>
    private static ExtractionResult<PartitionDefinition> ExtractListPartition(
        Match match,
        SqlBlock block,
        List<ValidationIssue> issues)
    {
        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var name = match.Groups["name"].Value;
        var parentTableName = match.Groups["parent"].Value;
        var valuesText = match.Groups["values"].Value;
        var tablespace = match.Groups["tablespace"].Success ? match.Groups["tablespace"].Value : null;

        // Извлекаем список значений
        var inValues = ExtractListValues(valuesText);

        if (inValues.Count == 0)
        {
            return ExtractionResult<PartitionDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Partition,
                    "PARTITION_LIST_NO_VALUES",
                    $"LIST partition '{name}' has no values defined",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = valuesText,
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        // Проверка на дубликаты значений
        var duplicates = inValues
            .GroupBy(v => v)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Partition,
                "PARTITION_LIST_DUPLICATE_VALUES",
                $"LIST partition '{name}' contains duplicate values: {string.Join(", ", duplicates)}",
                new ValidationIssue.ValidationLocation
                {
                    Segment = valuesText,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        // Проверка на слишком много значений (более 100 может быть проблемой производительности)
        if (inValues.Count > 100)
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Partition,
                "PARTITION_LIST_TOO_MANY_VALUES",
                $"LIST partition '{name}' has {inValues.Count} values. Consider using RANGE or HASH partitioning for better performance",
                new ValidationIssue.ValidationLocation
                {
                    Segment = valuesText,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        var definition = new PartitionDefinition
        {
            Name = name,
            Schema = schema,
            ParentTableName = parentTableName,
            Strategy = PartitionStrategy.List,
            InValues = inValues,
            Tablespace = tablespace,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };

        return ExtractionResult<PartitionDefinition>.Success(definition, issues);
    }

    /// <summary>
    /// Извлекает HASH партицию
    /// </summary>
    private static ExtractionResult<PartitionDefinition> ExtractHashPartition(
        Match match,
        SqlBlock block,
        List<ValidationIssue> issues)
    {
        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var name = match.Groups["name"].Value;
        var parentTableName = match.Groups["parent"].Value;
        var modulusText = match.Groups["modulus"].Value;
        var remainderText = match.Groups["remainder"].Value;
        var tablespace = match.Groups["tablespace"].Success ? match.Groups["tablespace"].Value : null;

        if (!int.TryParse(modulusText, out var modulus) || modulus <= 0)
        {
            return ExtractionResult<PartitionDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Partition,
                    "PARTITION_HASH_INVALID_MODULUS",
                    $"HASH partition '{name}' has invalid MODULUS value: {modulusText}",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = $"MODULUS {modulusText}",
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        if (!int.TryParse(remainderText, out var remainder) || remainder < 0 || remainder >= modulus)
        {
            return ExtractionResult<PartitionDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Partition,
                    "PARTITION_HASH_INVALID_REMAINDER",
                    $"HASH partition '{name}' has invalid REMAINDER value: {remainderText} (must be between 0 and {modulus - 1})",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = $"REMAINDER {remainderText}",
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        var definition = new PartitionDefinition
        {
            Name = name,
            Schema = schema,
            ParentTableName = parentTableName,
            Strategy = PartitionStrategy.Hash,
            Modulus = modulus,
            Remainder = remainder,
            Tablespace = tablespace,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };

        return ExtractionResult<PartitionDefinition>.Success(definition, issues);
    }

    /// <summary>
    /// Извлекает DEFAULT партицию
    /// </summary>
    private static ExtractionResult<PartitionDefinition> ExtractDefaultPartition(
        Match match,
        SqlBlock block,
        List<ValidationIssue> issues)
    {
        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var name = match.Groups["name"].Value;
        var parentTableName = match.Groups["parent"].Value;
        var tablespace = match.Groups["tablespace"].Success ? match.Groups["tablespace"].Value : null;

        // Информационное сообщение: DEFAULT партиция принимает все строки, не подходящие под другие партиции
        issues.Add(ValidationIssue.Info(
            ValidationIssue.ValidationDefinitionType.Partition,
            "PARTITION_DEFAULT",
            $"Partition '{name}' is a DEFAULT partition that accepts all rows not matching other partitions",
            new ValidationIssue.ValidationLocation
            {
                Segment = block.Content,
                Line = block.StartLine,
                Column = 0
            }
        ));

        // Стратегию определить сложно для DEFAULT партиции, так как она зависит от родительской таблицы
        // Используем Range как значение по умолчанию, но это может быть любая стратегия
        var definition = new PartitionDefinition
        {
            Name = name,
            Schema = schema,
            ParentTableName = parentTableName,
            Strategy = PartitionStrategy.Range, // DEFAULT может быть для любой стратегии
            IsDefault = true,
            Tablespace = tablespace,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };

        return ExtractionResult<PartitionDefinition>.Success(definition, issues);
    }

    /// <summary>
    /// Извлекает список значений из текста LIST партиции
    /// </summary>
    private static IReadOnlyList<string> ExtractListValues(string valuesText)
    {
        if (string.IsNullOrWhiteSpace(valuesText))
        {
            return [];
        }

        var matches = ListValuePattern().Matches(valuesText);
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
