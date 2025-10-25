using System.Text;
using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer.Models.Tables;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает определения таблиц из SQL скрипта
/// </summary>
internal sealed partial class TableExtractor : BaseExtractor<TableDefinition>
{
    [GeneratedRegex(@"CREATE\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?([a-zA-Z_][a-zA-Z0-9_.]*)\s*\((.*?)\)(?:\s+PARTITION\s+BY\s+(RANGE|LIST|HASH)\s*\((.*?)\))?", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex TablePatternRegex();

    protected override Regex Pattern => TablePatternRegex();

    protected override TableDefinition ParseMatch(Match match, string statement)
    {
        // Пропускаем PARTITION OF таблицы (они наследуют структуру от parent table)
        if (Regex.IsMatch(statement, @"PARTITION\s+OF\s+", RegexOptions.IgnoreCase))
        {
            return null!; // Возвращаем null для пропуска
        }

        var fullTableName = match.Groups[1].Value.Trim();
        
        // Извлекаем содержимое скобок с учетом вложенности
        var columnsBlock = ExtractBalancedParentheses(statement);
        
        var partitionMatch = Regex.Match(statement, @"PARTITION\s+BY\s+(RANGE|LIST|HASH)\s*\((.*?)\)", RegexOptions.IgnoreCase);
        var partitionStrategy = partitionMatch.Success ? partitionMatch.Groups[1].Value : string.Empty;
        var partitionKeys = partitionMatch.Success ? partitionMatch.Groups[2].Value : string.Empty;

        var schema = ExtractSchemaName(fullTableName);
        var tableName = ExtractTableName(fullTableName);

        var columns = ParseColumns(columnsBlock);
        var constraints = ParseTableConstraints(columnsBlock, tableName);

        PartitionInfo? partitionInfo = null;
        if (!string.IsNullOrWhiteSpace(partitionStrategy))
        {
            partitionInfo = new PartitionInfo
            {
                Strategy = Enum.Parse<PartitionStrategy>(partitionStrategy, ignoreCase: true),
                PartitionKeys = partitionKeys.Split(',').Select(k => k.Trim()).ToArray()
            };
        }

        return new TableDefinition
        {
            Name = tableName,
            Schema = schema,
            Columns = columns,
            Constraints = constraints,
            IsPartitioned = partitionInfo is not null,
            PartitionInfo = partitionInfo,
            RawSql = statement
        };
    }

    private string ExtractBalancedParentheses(string statement)
    {
        var startIndex = statement.IndexOf('(');
        if (startIndex == -1) return string.Empty;

        var depth = 0;
        var endIndex = startIndex;

        for (var i = startIndex; i < statement.Length; i++)
        {
            if (statement[i] == '(') depth++;
            else if (statement[i] == ')') depth--;

            if (depth == 0)
            {
                endIndex = i;
                break;
            }
        }

        return statement.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
    }

    private IReadOnlyList<ColumnDefinition> ParseColumns(string columnsBlock)
    {
        var columns = new List<ColumnDefinition>();
        var columnMatches = SplitColumns(columnsBlock);

        foreach (var columnDef in columnMatches)
        {
            if (IsConstraint(columnDef))
                continue;

            var column = ParseColumn(columnDef);
            if (column is not null)
            {
                columns.Add(column);
            }
        }

        return columns;
    }

    private ColumnDefinition? ParseColumn(string columnDef)
    {
        var parts = columnDef.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return null;

        var columnName = parts[0].Trim(',').Trim();
        var dataType = ExtractDataType(parts);
        var isArray = dataType.EndsWith("[]");
        if (isArray)
            dataType = dataType[..^2];

        var isNullable = !columnDef.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase);
        var isPrimaryKey = columnDef.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase);
        var isUnique = columnDef.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
        var defaultValue = ExtractDefaultValue(columnDef);

        return new ColumnDefinition
        {
            Name = columnName,
            DataType = dataType,
            IsNullable = isNullable && !isPrimaryKey,
            IsPrimaryKey = isPrimaryKey,
            IsUnique = isUnique || isPrimaryKey,
            IsArray = isArray,
            DefaultValue = defaultValue
        };
    }

    private IReadOnlyList<ConstraintDefinition> ParseTableConstraints(string columnsBlock, string tableName)
    {
        var constraints = new List<ConstraintDefinition>();
        var columns = SplitColumns(columnsBlock);

        foreach (var column in columns)
        {
            var trimmed = column.Trim();
            
            if (trimmed.StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase))
            {
                var constraint = ParseConstraint(trimmed, tableName);
                if (constraint is not null)
                {
                    constraints.Add(constraint);
                }
            }
        }

        return constraints;
    }

    private ConstraintDefinition? ParseConstraint(string constraintDef, string tableName)
    {
        var match = Regex.Match(constraintDef, 
            @"CONSTRAINT\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+(PRIMARY KEY|FOREIGN KEY|UNIQUE|CHECK)\s*\((.*?)\)(?:\s+REFERENCES\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\))?(?:\s+ON\s+DELETE\s+(CASCADE|RESTRICT|SET NULL|SET DEFAULT|NO ACTION))?(?:\s+ON\s+UPDATE\s+(CASCADE|RESTRICT|SET NULL|SET DEFAULT|NO ACTION))?",
            RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            // Попытка парсинга CHECK constraint
            var checkMatch = Regex.Match(constraintDef,
                @"CONSTRAINT\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+CHECK\s*\((.*)\)",
                RegexOptions.IgnoreCase);

            if (checkMatch.Success)
            {
                return new ConstraintDefinition
                {
                    Name = checkMatch.Groups[1].Value,
                    TableName = tableName,
                    Type = ConstraintType.Check,
                    CheckExpression = checkMatch.Groups[2].Value,
                    RawSql = constraintDef
                };
            }

            return null;
        }

        var name = match.Groups[1].Value;
        var type = match.Groups[2].Value.ToUpperInvariant();
        var columns = match.Groups[3].Value.Split(',').Select(c => c.Trim()).ToArray();
        var referencedTable = match.Groups[4].Value;
        var referencedColumns = match.Groups[5].Value.Split(',').Select(c => c.Trim()).ToArray();
        var onDelete = match.Groups[6].Value;
        var onUpdate = match.Groups[7].Value;

        var constraintType = type switch
        {
            "PRIMARY KEY" => ConstraintType.PrimaryKey,
            "FOREIGN KEY" => ConstraintType.ForeignKey,
            "UNIQUE" => ConstraintType.Unique,
            "CHECK" => ConstraintType.Check,
            _ => ConstraintType.Check
        };

        return new ConstraintDefinition
        {
            Name = name,
            TableName = tableName,
            Type = constraintType,
            Columns = columns,
            ReferencedTable = !string.IsNullOrWhiteSpace(referencedTable) ? referencedTable : null,
            ReferencedColumns = referencedColumns.Length > 0 && !string.IsNullOrWhiteSpace(referencedColumns[0]) 
                ? referencedColumns 
                : null,
            OnDelete = ParseReferentialAction(onDelete),
            OnUpdate = ParseReferentialAction(onUpdate),
            RawSql = constraintDef
        };
    }

    private static ReferentialAction? ParseReferentialAction(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
            return null;

        return action.ToUpperInvariant() switch
        {
            "CASCADE" => ReferentialAction.Cascade,
            "RESTRICT" => ReferentialAction.Restrict,
            "SET NULL" => ReferentialAction.SetNull,
            "SET DEFAULT" => ReferentialAction.SetDefault,
            "NO ACTION" => ReferentialAction.NoAction,
            _ => null
        };
    }

    private static List<string> SplitColumns(string columnsBlock)
    {
        var columns = new List<string>();
        var currentColumn = new StringBuilder();
        var parenthesesDepth = 0;
        var inQuotes = false;

        foreach (var ch in columnsBlock)
        {
            if (ch == '\'' && parenthesesDepth == 0)
            {
                inQuotes = !inQuotes;
            }

            if (!inQuotes)
            {
                if (ch == '(') parenthesesDepth++;
                if (ch == ')') parenthesesDepth--;
            }

            if (ch == ',' && parenthesesDepth == 0 && !inQuotes)
            {
                var col = currentColumn.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(col))
                {
                    columns.Add(col);
                }
                currentColumn.Clear();
            }
            else
            {
                currentColumn.Append(ch);
            }
        }

        var lastCol = currentColumn.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(lastCol))
        {
            columns.Add(lastCol);
        }

        return columns;
    }

    private static string ExtractDataType(string[] parts)
    {
        if (parts.Length < 2)
            return "TEXT";

        var typeBuilder = new StringBuilder(parts[1]);
        
        // Обработка составных типов (VARCHAR(255), NUMERIC(10,2) и т.д.)
        for (var i = 2; i < parts.Length; i++)
        {
            var part = parts[i];
            
            if (part.Contains('(') || part.Contains(')') || char.IsDigit(part[0]))
            {
                typeBuilder.Append(' ').Append(part);
            }
            else
            {
                break;
            }
        }

        return typeBuilder.ToString().Trim();
    }

    private static string? ExtractDefaultValue(string columnDef)
    {
        var match = Regex.Match(columnDef, @"DEFAULT\s+([^,\s]+(?:\s*\([^)]*\))?)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static bool IsConstraint(string columnDef)
    {
        var upper = columnDef.Trim().ToUpperInvariant();
        return upper.StartsWith("CONSTRAINT") ||
               upper.StartsWith("PRIMARY KEY") ||
               upper.StartsWith("FOREIGN KEY") ||
               upper.StartsWith("CHECK");
    }
}