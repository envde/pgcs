using System.Text;
using System.Text.RegularExpressions;
using PgCs.Core.Extraction;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Parsing.Comments;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Извлекает определения таблиц из SQL блоков
/// <para>
/// Поддерживает:
/// - Обычные таблицы (CREATE TABLE)
/// - Временные таблицы (TEMPORARY/TEMP)
/// - Unlogged таблицы (UNLOGGED)
/// - Партиционированные таблицы (PARTITION BY)
/// - Партиции (PARTITION OF)
/// - Наследование (INHERITS)
/// - Tablespace (TABLESPACE)
/// - Storage parameters (WITH)
/// </para>
/// </summary>
public sealed partial class TableExtractor : IExtractor<TableDefinition>
{
    // ============================================================================
    // Regex Patterns
    // ============================================================================

    /// <summary>
    /// Основной паттерн для извлечения CREATE TABLE
    /// Группы:
    /// - temporary: TEMPORARY/TEMP флаг
    /// - unlogged: UNLOGGED флаг
    /// - ifNotExists: IF NOT EXISTS
    /// - schema: имя схемы (опционально)
    /// - name: имя таблицы
    /// </summary>
    [GeneratedRegex(
        @"^\s*CREATE\s+(?:(TEMPORARY|TEMP|UNLOGGED)\s+)?TABLE\s+(?:(IF\s+NOT\s+EXISTS)\s+)?(?:(\w+)\.)?(\w+)\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex CreateTablePattern();

    /// <summary>
    /// Паттерн для извлечения партиционирования (PARTITION BY)
    /// Группы:
    /// - strategy: стратегия партиционирования (RANGE, LIST, HASH)
    /// - expression: выражение партиционирования
    /// </summary>
    [GeneratedRegex(
        @"PARTITION\s+BY\s+(RANGE|LIST|HASH)\s*\(([^)]+)\)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex PartitionByPattern();

    /// <summary>
    /// Паттерн для извлечения PARTITION OF (партиция таблицы)
    /// Группы:
    /// - schema: имя схемы родительской таблицы (опционально)
    /// - parentTable: имя родительской таблицы
    /// </summary>
    [GeneratedRegex(
        @"^\s*CREATE\s+(?:TEMPORARY|TEMP|UNLOGGED\s+)?TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?(?:\w+\.)?(\w+)\s+PARTITION\s+OF\s+(?:(\w+)\.)?(\w+)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex PartitionOfPattern();

    /// <summary>
    /// Паттерн для извлечения INHERITS (наследование)
    /// Группа:
    /// - tables: список родительских таблиц
    /// </summary>
    [GeneratedRegex(
        @"INHERITS\s*\(([^)]+)\)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex InheritsPattern();

    /// <summary>
    /// Паттерн для извлечения TABLESPACE
    /// Группа:
    /// - tablespace: имя tablespace
    /// </summary>
    [GeneratedRegex(
        @"TABLESPACE\s+(\w+)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex TablespacePattern();

    /// <summary>
    /// Паттерн для извлечения storage parameters (WITH)
    /// Группа:
    /// - params: параметры внутри WITH ( ... )
    /// </summary>
    [GeneratedRegex(
        @"WITH\s*\(([^)]+)\)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex WithParametersPattern();

    // ============================================================================
    // Public Methods
    // ============================================================================

    /// <inheritdoc />
    public bool CanExtract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        // Для таблиц достаточно одного блока
        if (blocks.Count == 0)
        {
            return false;
        }

        var block = blocks[0];
        return CreateTablePattern().IsMatch(block.Content) ||
               PartitionOfPattern().IsMatch(block.Content);
    }

    /// <inheritdoc />
    public ExtractionResult<TableDefinition> Extract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (!CanExtract(blocks))
        {
            return ExtractionResult<TableDefinition>.NotApplicable();
        }

        var block = blocks[0];
        var issues = new List<ValidationIssue>();

        // Проверяем, это партиция или обычная таблица
        var partitionOfMatch = PartitionOfPattern().Match(block.Content);
        if (partitionOfMatch.Success)
        {
            return ExtractPartitionTable(block, partitionOfMatch, issues);
        }

        var match = CreateTablePattern().Match(block.Content);
        if (!match.Success)
        {
            return ExtractionResult<TableDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Table,
                    "TABLE_PARSE_ERROR",
                    "Failed to parse TABLE definition. Expected format: CREATE TABLE [schema.]name (...)",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = block.Content,
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        var schema = match.Groups[3].Success ? match.Groups[3].Value : null;
        var name = match.Groups[4].Value;
        var isTemporary = match.Groups[1].Success &&
                         (match.Groups[1].Value.Equals("TEMPORARY", StringComparison.OrdinalIgnoreCase) ||
                          match.Groups[1].Value.Equals("TEMP", StringComparison.OrdinalIgnoreCase));
        var isUnlogged = match.Groups[1].Success &&
                        match.Groups[1].Value.Equals("UNLOGGED", StringComparison.OrdinalIgnoreCase);

        // Извлечение колонок
        var columns = ExtractColumns(block.Content, name, block, issues);

        if (columns.Count == 0)
        {
            return ExtractionResult<TableDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Table,
                    "TABLE_NO_COLUMNS",
                    $"TABLE '{name}' has no valid columns defined",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = block.Content,
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        // Проверка на дубликаты колонок
        var duplicateColumns = columns
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateColumns.Count > 0)
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Table,
                "TABLE_DUPLICATE_COLUMN",
                $"TABLE '{name}' contains duplicate column names: {string.Join(", ", duplicateColumns)}",
                new ValidationIssue.ValidationLocation
                {
                    Segment = block.Content,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        // Извлечение партиционирования
        var partitionInfo = ExtractPartitionInfo(block.Content);

        // Извлечение наследования
        var inheritsFrom = ExtractInherits(block.Content);

        // Извлечение tablespace
        var tablespace = ExtractTablespace(block.Content);

        // Извлечение storage parameters
        var storageParameters = ExtractStorageParameters(block.Content);

        var definition = new TableDefinition
        {
            Name = name,
            Schema = schema,
            Columns = columns,
            IsPartitioned = partitionInfo != null,
            PartitionInfo = partitionInfo,
            IsPartition = false,
            ParentTableName = null,
            IsTemporary = isTemporary,
            IsUnlogged = isUnlogged,
            Tablespace = tablespace,
            StorageParameters = storageParameters,
            InheritsFrom = inheritsFrom,
            SqlComment = block.HeaderComment,
            RawSql = block.Content
        };

        return ExtractionResult<TableDefinition>.Success(definition, issues);
    }

    // ============================================================================
    // Private Helper Methods
    // ============================================================================

    /// <summary>
    /// Извлекает определение таблицы-партиции (PARTITION OF)
    /// </summary>
    private ExtractionResult<TableDefinition> ExtractPartitionTable(
        SqlBlock block,
        Match partitionOfMatch,
        List<ValidationIssue> issues)
    {
        var schema = partitionOfMatch.Groups[2].Success ? partitionOfMatch.Groups[2].Value : null;
        var name = partitionOfMatch.Groups[1].Value;
        var parentTable = partitionOfMatch.Groups[3].Value;

        var definition = new TableDefinition
        {
            Name = name,
            Schema = schema,
            Columns = [],
            IsPartitioned = false,
            PartitionInfo = null,
            IsPartition = true,
            ParentTableName = parentTable,
            IsTemporary = false,
            IsUnlogged = false,
            Tablespace = ExtractTablespace(block.Content),
            StorageParameters = null,
            InheritsFrom = null,
            SqlComment = block.HeaderComment,
            RawSql = block.Content
        };

        return ExtractionResult<TableDefinition>.Success(definition, issues);
    }

    /// <summary>
    /// Извлекает информацию о партиционировании таблицы
    /// </summary>
    private PartitionInfo? ExtractPartitionInfo(string sql)
    {
        var match = PartitionByPattern().Match(sql);
        if (!match.Success)
        {
            return null;
        }

        var strategyStr = match.Groups[1].Value.ToUpperInvariant();
        var strategy = strategyStr switch
        {
            "RANGE" => PartitionStrategy.Range,
            "LIST" => PartitionStrategy.List,
            "HASH" => PartitionStrategy.Hash,
            _ => PartitionStrategy.Range
        };

        var expression = match.Groups[2].Value.Trim();
        var partitionKeys = new List<string>();

        // Парсинг выражения партиционирования
        // Может быть простое имя колонки или выражение
        if (expression.Contains('('))
        {
            // Сложное выражение, например: EXTRACT(YEAR FROM created_at)
            return new PartitionInfo
            {
                Strategy = strategy,
                PartitionKeys = [],
                PartitionExpression = expression
            };
        }

        // Простые имена колонок, разделенные запятыми
        var keys = expression.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        partitionKeys.AddRange(keys);

        return new PartitionInfo
        {
            Strategy = strategy,
            PartitionKeys = partitionKeys,
            PartitionExpression = null
        };
    }

    /// <summary>
    /// Извлекает список родительских таблиц из INHERITS
    /// </summary>
    private IReadOnlyList<string>? ExtractInherits(string sql)
    {
        var match = InheritsPattern().Match(sql);
        if (!match.Success)
        {
            return null;
        }

        var tablesStr = match.Groups[1].Value;
        var tables = tablesStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.Trim())
            .ToList();

        return tables.Count > 0 ? tables : null;
    }

    /// <summary>
    /// Извлекает имя tablespace
    /// </summary>
    private string? ExtractTablespace(string sql)
    {
        var match = TablespacePattern().Match(sql);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Извлекает storage parameters из WITH (...)
    /// </summary>
    private IReadOnlyDictionary<string, string>? ExtractStorageParameters(string sql)
    {
        var match = WithParametersPattern().Match(sql);
        if (!match.Success)
        {
            return null;
        }

        var paramsStr = match.Groups[1].Value;
        var parameters = new Dictionary<string, string>();

        // Парсинг параметров вида: key = value, key2 = value2
        var paramPairs = paramsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var pair in paramPairs)
        {
            var parts = pair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                parameters[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return parameters.Count > 0 ? parameters : null;
    }

    /// <summary>
    /// Извлекает колонки таблицы
    /// Упрощенная версия - извлекает только базовую информацию
    /// </summary>
    private IReadOnlyList<TableColumn> ExtractColumns(
        string sql,
        string tableName,
        SqlBlock block,
        List<ValidationIssue> issues)
    {
        var columns = new List<TableColumn>();

        // Находим тело таблицы между первой открывающей и последней закрывающей скобкой
        var startIndex = sql.IndexOf('(');
        var endIndex = sql.LastIndexOf(')');

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
        {
            return columns;
        }

        var tableBody = sql.Substring(startIndex + 1, endIndex - startIndex - 1);

        // Разбиваем по запятым, учитывая вложенные скобки
        var columnDefinitions = SplitColumnDefinitions(tableBody);

        foreach (var definition in columnDefinitions)
        {
            var trimmedDef = definition.Trim();

            // Пропускаем CONSTRAINT строки
            if (trimmedDef.StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Пропускаем пустые определения
            if (string.IsNullOrWhiteSpace(trimmedDef))
            {
                continue;
            }

            // Попытка извлечь колонку
            var column = TryExtractColumn(trimmedDef);
            if (column != null)
            {
                // Ищем inline комментарий в block.InlineComments
                if (block.InlineComments != null)
                {
                    var inlineComment = block.InlineComments.FirstOrDefault(c =>
                        c.Key.Equals(column.Name, StringComparison.OrdinalIgnoreCase));

                    if (inlineComment != null)
                    {
                        var commentText = inlineComment.Comment;
                        var parsedComment = new CommentMetadataParser().Parse(commentText);

                        column = column with
                        {
                            SqlComment = parsedComment.Comment,
                            ToName = parsedComment.ToName
                        };
                    }
                }

                columns.Add(column);
            }
            else
            {
                // Добавляем предупреждение для невалидной колонки
                issues.Add(ValidationIssue.Warning(
                    ValidationIssue.ValidationDefinitionType.Table,
                    "TABLE_INVALID_COLUMN",
                    $"Failed to parse column definition in TABLE '{tableName}': '{trimmedDef}'",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = trimmedDef,
                        Line = block.StartLine,
                        Column = 0
                    }
                ));
            }
        }

        return columns;
    }

    /// <summary>
    /// Разбивает определения колонок по запятым, учитывая вложенные скобки
    /// Например: "id BIGSERIAL PRIMARY KEY, name VARCHAR(50), coords point[]"
    /// </summary>
    private static List<string> SplitColumnDefinitions(string tableBody)
    {
        var definitions = new List<string>();
        var current = new StringBuilder();
        var parenDepth = 0;

        for (int i = 0; i < tableBody.Length; i++)
        {
            var ch = tableBody[i];

            if (ch == '(')
            {
                parenDepth++;
                current.Append(ch);
            }
            else if (ch == ')')
            {
                parenDepth--;
                current.Append(ch);
            }
            else if (ch == ',' && parenDepth == 0)
            {
                // Запятая на верхнем уровне - разделитель колонок
                definitions.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        // Добавляем последнее определение
        if (current.Length > 0)
        {
            definitions.Add(current.ToString());
        }

        return definitions;
    }

    /// <summary>
    /// Пытается извлечь определение колонки из строки
    /// </summary>
    private TableColumn? TryExtractColumn(string line)
    {
        // Убираем завершающую запятую
        line = line.TrimEnd(',').Trim();

        // Простой парсинг: имя_колонки тип [NOT NULL] [DEFAULT value] [другие модификаторы]
        var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return null;
        }

        var name = parts[0].Trim();

        // Проверка на ключевые слова (PRIMARY KEY, FOREIGN KEY и т.д.)
        if (IsKeyword(name))
        {
            return null;
        }

        // Извлекаем тип данных - может состоять из нескольких частей (например, DECIMAL(10, 2))
        // Собираем части до первого ключевого слова или до конца
        var dataTypeBuilder = new System.Text.StringBuilder(parts[1]);
        for (int i = 2; i < parts.Length; i++)
        {
            var part = parts[i];
            // Проверяем, является ли часть началом ключевого слова или модификатора
            if (IsKeyword(part) ||
                part.Equals("NOT", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("REFERENCES", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            // Добавляем часть к типу (нужно для DECIMAL(10, 2) где пробел разделяет 10, и 2))
            dataTypeBuilder.Append(' ').Append(part);
        }

        var dataType = dataTypeBuilder.ToString().Trim();

        // Извлечение параметров типа (VARCHAR(255), NUMERIC(12,2) и т.д.)
        var maxLength = ExtractMaxLength(dataType);
        var (precision, scale) = ExtractNumericParams(dataType);
        var isArray = dataType.Contains('[');

        // Очистка типа от параметров
        dataType = CleanDataType(dataType);

        // Проверка на NOT NULL
        var isNullable = !line.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase);

        // Извлечение DEFAULT
        var defaultValue = ExtractDefaultValue(line);

        // Проверка на PRIMARY KEY
        var isPrimaryKey = line.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase);

        // Проверка на UNIQUE
        var isUnique = line.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);

        // Извлечение IDENTITY (GENERATED ALWAYS AS IDENTITY, GENERATED BY DEFAULT AS IDENTITY)
        var (isIdentity, identityGeneration) = ExtractIdentityInfo(line, dataType);

        // Извлечение GENERATED (GENERATED ALWAYS AS ... STORED)
        var (isGenerated, generationExpression) = ExtractGeneratedInfo(line);

        // Извлечение COLLATE
        var collation = ExtractCollation(line);

        return new TableColumn
        {
            Name = name,
            DataType = dataType,
            IsNullable = isNullable,
            DefaultValue = defaultValue,
            MaxLength = maxLength,
            NumericPrecision = precision,
            NumericScale = scale,
            IsArray = isArray,
            IsPrimaryKey = isPrimaryKey,
            IsUnique = isUnique,
            IsIdentity = isIdentity,
            IdentityGeneration = identityGeneration,
            IsGenerated = isGenerated,
            GenerationExpression = generationExpression,
            Collation = collation,
            SqlComment = null
        };
    }

    /// <summary>
    /// Проверяет, является ли строка ключевым словом SQL
    /// </summary>
    private static bool IsKeyword(string word)
    {
        var keywords = new[] { "PRIMARY", "FOREIGN", "UNIQUE", "CHECK", "CONSTRAINT", "REFERENCES" };
        return keywords.Any(k => word.Equals(k, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Извлекает информацию о IDENTITY колонке
    /// Поддерживает: GENERATED ALWAYS AS IDENTITY, GENERATED BY DEFAULT AS IDENTITY
    /// </summary>
    private static (bool isIdentity, string? identityGeneration) ExtractIdentityInfo(string line, string dataType)
    {
        // Проверяем SERIAL типы
        if (dataType.Contains("SERIAL", StringComparison.OrdinalIgnoreCase))
        {
            return (true, "BY DEFAULT");
        }

        // Проверяем GENERATED ... AS IDENTITY
        var upperLine = line.ToUpperInvariant();
        if (!upperLine.Contains("GENERATED") || !upperLine.Contains("AS IDENTITY"))
        {
            return (false, null);
        }

        // Определяем тип генерации
        if (upperLine.Contains("GENERATED ALWAYS AS IDENTITY"))
        {
            return (true, "ALWAYS");
        }

        if (upperLine.Contains("GENERATED BY DEFAULT AS IDENTITY"))
        {
            return (true, "BY DEFAULT");
        }

        return (false, null);
    }

    /// <summary>
    /// Извлекает информацию о вычисляемой колонке (GENERATED ALWAYS AS ... STORED)
    /// </summary>
    private static (bool isGenerated, string? generationExpression) ExtractGeneratedInfo(string line)
    {
        // GENERATED ALWAYS AS (expression) STORED
        var match = Regex.Match(line,
            @"GENERATED\s+ALWAYS\s+AS\s*\(([^)]+)\)\s*STORED",
            RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var expression = match.Groups[1].Value.Trim();
            return (true, expression);
        }

        return (false, null);
    }

    /// <summary>
    /// Извлекает collation из определения колонки
    /// Поддерживает: COLLATE "collation_name" и COLLATE collation_name
    /// </summary>
    private static string? ExtractCollation(string line)
    {
        // COLLATE "en_US" или COLLATE en_US
        var match = Regex.Match(line,
            @"COLLATE\s+(?:""([^""]+)""|(\w+))",
            RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
        }

        return null;
    }

    /// <summary>
    /// Извлекает максимальную длину из типа VARCHAR(n), CHAR(n)
    /// </summary>
    private static int? ExtractMaxLength(string dataType)
    {
        if (!dataType.Contains('('))
        {
            return null;
        }

        var match = Regex.Match(dataType, @"\((\d+)\)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var length))
        {
            return length;
        }

        return null;
    }

    /// <summary>
    /// Извлекает precision и scale из NUMERIC(p,s), DECIMAL(p,s)
    /// </summary>
    private static (int? precision, int? scale) ExtractNumericParams(string dataType)
    {
        var match = Regex.Match(dataType, @"\((\d+),\s*(\d+)\)");
        if (match.Success)
        {
            var precision = int.TryParse(match.Groups[1].Value, out var p) ? p : (int?)null;
            var scale = int.TryParse(match.Groups[2].Value, out var s) ? s : (int?)null;
            return (precision, scale);
        }

        return (null, null);
    }

    /// <summary>
    /// Очищает тип данных от параметров и массивов
    /// </summary>
    private static string CleanDataType(string dataType)
    {
        // Убираем параметры в скобках
        var cleaned = Regex.Replace(dataType, @"\([^)]+\)", string.Empty);

        // Убираем массивы []
        cleaned = cleaned.Replace("[]", string.Empty);

        return cleaned.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Извлекает DEFAULT значение из определения колонки
    /// </summary>
    private static string? ExtractDefaultValue(string line)
    {
        var match = Regex.Match(
            line,
            @"DEFAULT\s+(.+?)(?:\s+(?:NOT\s+NULL|NULL|PRIMARY\s+KEY|UNIQUE|CHECK|,)|\s*$)",
            RegexOptions.IgnoreCase
        );

        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}
