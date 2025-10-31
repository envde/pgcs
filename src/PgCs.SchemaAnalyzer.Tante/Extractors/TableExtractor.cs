using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;

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
public sealed partial class TableExtractor : ITableExtractor
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

    /// <summary>
    /// Паттерн для извлечения одной колонки
    /// Упрощенный паттерн для базового извлечения имени и типа
    /// </summary>
    [GeneratedRegex(
        @"^\s*(\w+)\s+([A-Za-z_][\w\s\(\),\[\]]+?)(?:\s+(NOT\s+NULL|NULL|PRIMARY\s+KEY|UNIQUE|DEFAULT\s+.+?))?(?:\s*,|\s*$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColumnPattern();

    /// <summary>
    /// Паттерн для извлечения CONSTRAINT на уровне таблицы
    /// </summary>
    [GeneratedRegex(
        @"CONSTRAINT\s+(\w+)\s+(PRIMARY\s+KEY|FOREIGN\s+KEY|UNIQUE|CHECK|EXCLUDE)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ConstraintPattern();

    // ============================================================================
    // Public Methods
    // ============================================================================

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        return CreateTablePattern().IsMatch(block.Content) || 
               PartitionOfPattern().IsMatch(block.Content);
    }

    /// <inheritdoc />
    public TableDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (!CanExtract(block))
        {
            return null;
        }

        // Проверяем, это партиция или обычная таблица
        var partitionOfMatch = PartitionOfPattern().Match(block.Content);
        if (partitionOfMatch.Success)
        {
            return ExtractPartitionTable(block, partitionOfMatch);
        }

        var match = CreateTablePattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups[3].Success ? match.Groups[3].Value : null;
        var name = match.Groups[4].Value;
        var isTemporary = match.Groups[1].Success && 
                         (match.Groups[1].Value.Equals("TEMPORARY", StringComparison.OrdinalIgnoreCase) ||
                          match.Groups[1].Value.Equals("TEMP", StringComparison.OrdinalIgnoreCase));
        var isUnlogged = match.Groups[1].Success && 
                        match.Groups[1].Value.Equals("UNLOGGED", StringComparison.OrdinalIgnoreCase);

        // Извлечение колонок
        var columns = ExtractColumns(block.Content);

        // Извлечение партиционирования
        var partitionInfo = ExtractPartitionInfo(block.Content);

        // Извлечение наследования
        var inheritsFrom = ExtractInherits(block.Content);

        // Извлечение tablespace
        var tablespace = ExtractTablespace(block.Content);

        // Извлечение storage parameters
        var storageParameters = ExtractStorageParameters(block.Content);

        return new TableDefinition
        {
            Name = name,
            Schema = schema,
            Columns = columns,
            Constraints = [],
            Indexes = [],
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
    }

    // ============================================================================
    // Private Helper Methods
    // ============================================================================

    /// <summary>
    /// Извлекает определение таблицы-партиции (PARTITION OF)
    /// </summary>
    private TableDefinition? ExtractPartitionTable(SqlBlock block, Match partitionOfMatch)
    {
        var schema = partitionOfMatch.Groups[2].Success ? partitionOfMatch.Groups[2].Value : null;
        var name = partitionOfMatch.Groups[1].Value;
        var parentTable = partitionOfMatch.Groups[3].Value;

        return new TableDefinition
        {
            Name = name,
            Schema = schema,
            Columns = [],
            Constraints = [],
            Indexes = [],
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
    private IReadOnlyList<TableColumn> ExtractColumns(string sql)
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

        // Разделяем на строки и обрабатываем каждую
        var lines = tableBody.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Пропускаем CONSTRAINT строки
            if (trimmedLine.StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Пропускаем пустые строки и комментарии
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("--"))
            {
                continue;
            }

            // Попытка извлечь колонку
            var column = TryExtractColumn(trimmedLine);
            if (column != null)
            {
                columns.Add(column);
            }
        }

        return columns;
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
        var typeEndIndex = 2;
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
                typeEndIndex = i;
                break;
            }
            
            // Добавляем часть к типу (нужно для DECIMAL(10, 2) где пробел разделяет 10, и 2))
            dataTypeBuilder.Append(' ').Append(part);
            typeEndIndex = i + 1;
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
            IsIdentity = dataType.Contains("SERIAL", StringComparison.OrdinalIgnoreCase),
            Comment = null
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
