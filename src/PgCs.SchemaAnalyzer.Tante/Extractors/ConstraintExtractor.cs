using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор для извлечения определений ограничений целостности из SQL-скриптов
/// Поддерживает PRIMARY KEY, FOREIGN KEY, UNIQUE, CHECK, EXCLUDE
/// </summary>
public sealed class ConstraintExtractor : IConstraintExtractor
{
    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        var content = block.Content;
        
        // Быстрая проверка на наличие ALTER TABLE и CONSTRAINT
        return content.Contains("ALTER TABLE", StringComparison.OrdinalIgnoreCase) &&
               content.Contains("CONSTRAINT", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ConstraintDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var content = block.Content.Trim();
        
        try
        {
            // Пример: ALTER TABLE users ADD CONSTRAINT pk_users PRIMARY KEY (id);
            
            // Извлекаем имя таблицы
            var tableName = ExtractTableName(content);
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return null;
            }
            
            // Извлекаем схему таблицы если есть
            string? schema = null;
            if (tableName.Contains('.'))
            {
                var parts = tableName.Split('.');
                schema = parts[0];
                tableName = parts[1];
            }
            
            // Извлекаем имя ограничения
            var constraintName = ExtractConstraintName(content);
            if (string.IsNullOrWhiteSpace(constraintName))
            {
                return null;
            }
            
            // Определяем тип ограничения
            var constraintType = DetermineConstraintType(content);
            if (constraintType is null)
            {
                return null;
            }
            
            // Извлекаем детали в зависимости от типа
            return constraintType.Value switch
            {
                ConstraintType.PrimaryKey => ExtractPrimaryKey(content, constraintName, tableName, schema, block),
                ConstraintType.ForeignKey => ExtractForeignKey(content, constraintName, tableName, schema, block),
                ConstraintType.Unique => ExtractUnique(content, constraintName, tableName, schema, block),
                ConstraintType.Check => ExtractCheck(content, constraintName, tableName, schema, block),
                ConstraintType.Exclude => ExtractExclude(content, constraintName, tableName, schema, block),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Извлекает имя таблицы из ALTER TABLE
    /// </summary>
    private static string? ExtractTableName(string content)
    {
        var upper = content.ToUpperInvariant();
        var alterTableIndex = upper.IndexOf("ALTER TABLE", StringComparison.Ordinal);
        if (alterTableIndex < 0)
        {
            return null;
        }
        
        var afterAlterTable = content[(alterTableIndex + 11)..].TrimStart();
        var tokens = afterAlterTable.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        return tokens.Length > 0 ? tokens[0] : null;
    }

    /// <summary>
    /// Извлекает имя ограничения
    /// </summary>
    private static string? ExtractConstraintName(string content)
    {
        var upper = content.ToUpperInvariant();
        var constraintIndex = upper.IndexOf("CONSTRAINT", StringComparison.Ordinal);
        if (constraintIndex < 0)
        {
            return null;
        }
        
        var afterConstraint = content[(constraintIndex + 10)..].TrimStart();
        var tokens = afterConstraint.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        return tokens.Length > 0 ? tokens[0] : null;
    }

    /// <summary>
    /// Определяет тип ограничения
    /// </summary>
    private static ConstraintType? DetermineConstraintType(string content)
    {
        var upper = content.ToUpperInvariant();
        
        // Ищем ADD CONSTRAINT, чтобы начать поиск после имени constraint
        var addConstraintIndex = upper.IndexOf("ADD CONSTRAINT", StringComparison.Ordinal);
        if (addConstraintIndex < 0)
        {
            addConstraintIndex = 0;
        }
        else
        {
            // Пропускаем "ADD CONSTRAINT" и имя constraint
            var afterAddConstraint = upper[(addConstraintIndex + 14)..].TrimStart();
            var spaceIndex = afterAddConstraint.IndexOf(' ');
            if (spaceIndex > 0)
            {
                // Начинаем поиск после имени constraint
                addConstraintIndex = upper.Length - afterAddConstraint.Length + spaceIndex + 1;
            }
        }
        
        var searchArea = upper[addConstraintIndex..];
        
        if (searchArea.Contains("PRIMARY KEY"))
        {
            return ConstraintType.PrimaryKey;
        }
        
        if (searchArea.Contains("FOREIGN KEY"))
        {
            return ConstraintType.ForeignKey;
        }
        
        if (searchArea.Contains("UNIQUE"))
        {
            return ConstraintType.Unique;
        }
        
        if (searchArea.Contains("CHECK"))
        {
            return ConstraintType.Check;
        }
        
        if (searchArea.Contains("EXCLUDE"))
        {
            return ConstraintType.Exclude;
        }
        
        return null;
    }

    /// <summary>
    /// Извлекает PRIMARY KEY ограничение
    /// </summary>
    private static ConstraintDefinition ExtractPrimaryKey(string content, string constraintName, 
        string tableName, string? schema, SqlBlock block)
    {
        var columns = ExtractColumnsInParentheses(content, "PRIMARY KEY");
        
        return new ConstraintDefinition
        {
            Name = constraintName,
            TableName = tableName,
            Type = ConstraintType.PrimaryKey,
            Columns = columns,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает FOREIGN KEY ограничение
    /// </summary>
    private static ConstraintDefinition ExtractForeignKey(string content, string constraintName,
        string tableName, string? schema, SqlBlock block)
    {
        var columns = ExtractColumnsInParentheses(content, "FOREIGN KEY");
        var referencedTable = ExtractReferencedTable(content);
        var referencedColumns = ExtractReferencedColumns(content);
        var onDelete = ExtractReferentialAction(content, "ON DELETE");
        var onUpdate = ExtractReferentialAction(content, "ON UPDATE");
        var (isDeferrable, isInitiallyDeferred) = ExtractDeferrableInfo(content);
        
        return new ConstraintDefinition
        {
            Name = constraintName,
            TableName = tableName,
            Type = ConstraintType.ForeignKey,
            Columns = columns,
            ReferencedTable = referencedTable,
            ReferencedColumns = referencedColumns,
            OnDelete = onDelete,
            OnUpdate = onUpdate,
            IsDeferrable = isDeferrable,
            IsInitiallyDeferred = isInitiallyDeferred,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает UNIQUE ограничение
    /// </summary>
    private static ConstraintDefinition ExtractUnique(string content, string constraintName,
        string tableName, string? schema, SqlBlock block)
    {
        var columns = ExtractColumnsInParentheses(content, "UNIQUE");
        
        return new ConstraintDefinition
        {
            Name = constraintName,
            TableName = tableName,
            Type = ConstraintType.Unique,
            Columns = columns,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает CHECK ограничение
    /// </summary>
    private static ConstraintDefinition ExtractCheck(string content, string constraintName,
        string tableName, string? schema, SqlBlock block)
    {
        var checkExpression = ExtractCheckExpression(content);
        var (isDeferrable, isInitiallyDeferred) = ExtractDeferrableInfo(content);
        
        return new ConstraintDefinition
        {
            Name = constraintName,
            TableName = tableName,
            Type = ConstraintType.Check,
            CheckExpression = checkExpression,
            IsDeferrable = isDeferrable,
            IsInitiallyDeferred = isInitiallyDeferred,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает EXCLUDE ограничение
    /// </summary>
    private static ConstraintDefinition ExtractExclude(string content, string constraintName,
        string tableName, string? schema, SqlBlock block)
    {
        // EXCLUDE ограничения имеют сложный синтаксис, пока сохраняем базовую информацию
        var columns = ExtractColumnsInParentheses(content, "EXCLUDE");
        
        return new ConstraintDefinition
        {
            Name = constraintName,
            TableName = tableName,
            Type = ConstraintType.Exclude,
            Columns = columns,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает колонки в скобках после ключевого слова
    /// </summary>
    private static IReadOnlyList<string> ExtractColumnsInParentheses(string content, string keyword)
    {
        var upper = content.ToUpperInvariant();
        var keywordIndex = upper.IndexOf(keyword, StringComparison.Ordinal);
        if (keywordIndex < 0)
        {
            return [];
        }
        
        var afterKeyword = content[(keywordIndex + keyword.Length)..].TrimStart();
        if (!afterKeyword.StartsWith('('))
        {
            return [];
        }
        
        var closingParen = FindMatchingClosingParen(afterKeyword, 0);
        if (closingParen < 0)
        {
            return [];
        }
        
        var columnsText = afterKeyword[1..closingParen];
        
        return columnsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();
    }

    /// <summary>
    /// Находит соответствующую закрывающую скобку
    /// </summary>
    private static int FindMatchingClosingParen(string text, int startIndex)
    {
        var depth = 0;
        
        for (int i = startIndex; i < text.Length; i++)
        {
            if (text[i] == '(')
            {
                depth++;
            }
            else if (text[i] == ')')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }
        
        return -1;
    }

    /// <summary>
    /// Извлекает имя референсной таблицы для FOREIGN KEY
    /// </summary>
    private static string? ExtractReferencedTable(string content)
    {
        var upper = content.ToUpperInvariant();
        var referencesIndex = upper.IndexOf("REFERENCES", StringComparison.Ordinal);
        if (referencesIndex < 0)
        {
            return null;
        }
        
        var afterReferences = content[(referencesIndex + 10)..].TrimStart();
        var tokens = afterReferences.Split([' ', '\t', '\r', '\n', '('], StringSplitOptions.RemoveEmptyEntries);
        
        if (tokens.Length == 0)
        {
            return null;
        }
        
        var tableName = tokens[0];
        
        // Удаляем схему если есть, оставляем только имя таблицы
        if (tableName.Contains('.'))
        {
            var parts = tableName.Split('.');
            tableName = parts[^1];
        }
        
        return tableName;
    }

    /// <summary>
    /// Извлекает колонки референсной таблицы для FOREIGN KEY
    /// </summary>
    private static IReadOnlyList<string>? ExtractReferencedColumns(string content)
    {
        var upper = content.ToUpperInvariant();
        var referencesIndex = upper.IndexOf("REFERENCES", StringComparison.Ordinal);
        if (referencesIndex < 0)
        {
            return null;
        }
        
        var afterReferences = content[(referencesIndex + 10)..];
        var parenIndex = afterReferences.IndexOf('(');
        if (parenIndex < 0)
        {
            return null;
        }
        
        var closingParen = FindMatchingClosingParen(afterReferences, parenIndex);
        if (closingParen < 0)
        {
            return null;
        }
        
        var columnsText = afterReferences[(parenIndex + 1)..closingParen];
        
        var columns = columnsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();
        
        return columns.Count > 0 ? columns : null;
    }

    /// <summary>
    /// Извлекает действие при нарушении ссылочной целостности
    /// </summary>
    private static ReferentialAction? ExtractReferentialAction(string content, string keyword)
    {
        var upper = content.ToUpperInvariant();
        var keywordIndex = upper.IndexOf(keyword, StringComparison.Ordinal);
        if (keywordIndex < 0)
        {
            return null;
        }
        
        var afterKeyword = content[(keywordIndex + keyword.Length)..].TrimStart();
        
        if (afterKeyword.StartsWith("NO ACTION", StringComparison.OrdinalIgnoreCase))
        {
            return ReferentialAction.NoAction;
        }
        
        if (afterKeyword.StartsWith("RESTRICT", StringComparison.OrdinalIgnoreCase))
        {
            return ReferentialAction.Restrict;
        }
        
        if (afterKeyword.StartsWith("CASCADE", StringComparison.OrdinalIgnoreCase))
        {
            return ReferentialAction.Cascade;
        }
        
        if (afterKeyword.StartsWith("SET NULL", StringComparison.OrdinalIgnoreCase))
        {
            return ReferentialAction.SetNull;
        }
        
        if (afterKeyword.StartsWith("SET DEFAULT", StringComparison.OrdinalIgnoreCase))
        {
            return ReferentialAction.SetDefault;
        }
        
        return null;
    }

    /// <summary>
    /// Извлекает выражение для CHECK ограничения
    /// </summary>
    private static string? ExtractCheckExpression(string content)
    {
        var upper = content.ToUpperInvariant();
        var checkIndex = upper.IndexOf("CHECK", StringComparison.Ordinal);
        if (checkIndex < 0)
        {
            return null;
        }
        
        var afterCheck = content[(checkIndex + 5)..].TrimStart();
        if (!afterCheck.StartsWith('('))
        {
            return null;
        }
        
        var closingParen = FindMatchingClosingParen(afterCheck, 0);
        if (closingParen < 0)
        {
            return null;
        }
        
        var expression = afterCheck[1..closingParen].Trim();
        
        return string.IsNullOrWhiteSpace(expression) ? null : expression;
    }

    /// <summary>
    /// Извлекает информацию о DEFERRABLE и INITIALLY DEFERRED
    /// </summary>
    private static (bool IsDeferrable, bool IsInitiallyDeferred) ExtractDeferrableInfo(string content)
    {
        var upper = content.ToUpperInvariant();
        
        var isDeferrable = upper.Contains("DEFERRABLE") && !upper.Contains("NOT DEFERRABLE");
        var isInitiallyDeferred = upper.Contains("INITIALLY DEFERRED");
        
        return (isDeferrable, isInitiallyDeferred);
    }
}
