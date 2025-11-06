using System.Text.RegularExpressions;
using PgCs.Core.Extraction;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Универсальный экстрактор комментариев для всех типов объектов PostgreSQL
/// <para>
/// Поддерживает извлечение комментариев для следующих объектов:
/// <list type="bullet">
/// <item><description>TABLE - COMMENT ON TABLE schema.table_name IS 'comment'</description></item>
/// <item><description>COLUMN - COMMENT ON COLUMN schema.table_name.column_name IS 'comment'</description></item>
/// <item><description>FUNCTION - COMMENT ON FUNCTION schema.function_name(params) IS 'comment'</description></item>
/// <item><description>INDEX - COMMENT ON INDEX schema.index_name IS 'comment'</description></item>
/// <item><description>TRIGGER - COMMENT ON TRIGGER trigger_name ON schema.table_name IS 'comment'</description></item>
/// <item><description>CONSTRAINT - COMMENT ON CONSTRAINT constraint_name ON schema.table_name IS 'comment'</description></item>
/// <item><description>TYPE - COMMENT ON TYPE schema.type_name IS 'comment'</description></item>
/// <item><description>VIEW - COMMENT ON VIEW schema.view_name IS 'comment'</description></item>
/// </list>
/// </para>
/// </summary>
public sealed partial class CommentExtractor : IExtractor<CommentDefinition>
{
    // ============================================================================
    // Regex Patterns
    // ============================================================================

    /// <summary>
    /// Паттерн для COMMENT ON TABLE
    /// Группы: schema, table, comment
    /// </summary>
    [GeneratedRegex(
        @"\A\s*COMMENT\s+ON\s+TABLE\s+(?:(?<schema>\w+)\.)?(?<table>\w+)\s+IS\s+'(?<comment>(?:[^']|'')*)'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex TableCommentPattern();

    /// <summary>
    /// Паттерн для COMMENT ON COLUMN
    /// Группы: schema, table, column, comment
    /// </summary>
    [GeneratedRegex(
        @"\A\s*COMMENT\s+ON\s+COLUMN\s+(?:(?<schema>\w+)\.)?(?<table>\w+)\.(?<column>\w+)\s+IS\s+'(?<comment>(?:[^']|'')*)'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColumnCommentPattern();

    /// <summary>
    /// Паттерн для COMMENT ON FUNCTION
    /// Группы: schema, function, params, comment
    /// </summary>
    [GeneratedRegex(
        @"\A\s*COMMENT\s+ON\s+(?:FUNCTION|PROCEDURE)\s+(?:(?<schema>\w+)\.)?(?<function>\w+)\s*\((?<params>[^)]*)\)\s+IS\s+'(?<comment>(?:[^']|'')*)'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex FunctionCommentPattern();

    /// <summary>
    /// Паттерн для COMMENT ON INDEX
    /// Группы: schema, index, comment
    /// </summary>
    [GeneratedRegex(
        @"\A\s*COMMENT\s+ON\s+INDEX\s+(?:(?<schema>\w+)\.)?(?<index>\w+)\s+IS\s+'(?<comment>(?:[^']|'')*)'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex IndexCommentPattern();

    /// <summary>
    /// Паттерн для COMMENT ON TRIGGER
    /// Группы: trigger, schema, table, comment
    /// </summary>
    [GeneratedRegex(
        @"\A\s*COMMENT\s+ON\s+TRIGGER\s+(?<trigger>\w+)\s+ON\s+(?:(?<schema>\w+)\.)?(?<table>\w+)\s+IS\s+'(?<comment>(?:[^']|'')*)'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex TriggerCommentPattern();

    /// <summary>
    /// Паттерн для COMMENT ON CONSTRAINT
    /// Группы: constraint, schema, table, comment
    /// </summary>
    [GeneratedRegex(
        @"\A\s*COMMENT\s+ON\s+CONSTRAINT\s+(?<constraint>\w+)\s+ON\s+(?:(?<schema>\w+)\.)?(?<table>\w+)\s+IS\s+'(?<comment>(?:[^']|'')*)'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ConstraintCommentPattern();

    /// <summary>
    /// Паттерн для COMMENT ON TYPE
    /// Группы: schema, type, comment
    /// </summary>
    [GeneratedRegex(
        @"\A\s*COMMENT\s+ON\s+TYPE\s+(?:(?<schema>\w+)\.)?(?<type>\w+)\s+IS\s+'(?<comment>(?:[^']|'')*)'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeCommentPattern();

    /// <summary>
    /// Паттерн для COMMENT ON VIEW
    /// Группы: schema, view, comment
    /// </summary>
    [GeneratedRegex(
        @"\A\s*COMMENT\s+ON\s+(?:MATERIALIZED\s+)?VIEW\s+(?:(?<schema>\w+)\.)?(?<view>\w+)\s+IS\s+'(?<comment>(?:[^']|'')*)'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ViewCommentPattern();

    // ============================================================================
    // Public Methods
    // ============================================================================

    /// <inheritdoc />
    public bool CanExtract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (blocks.Count == 0)
        {
            return false;
        }

        var content = blocks[0].Content;
        // Use regex to match COMMENT followed by any whitespace and ON
        return Regex.IsMatch(content, @"\ACOMMENT\s+ON\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    /// <inheritdoc />
    public ExtractionResult<CommentDefinition> Extract(IReadOnlyList<SqlBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (!CanExtract(blocks))
        {
            return ExtractionResult<CommentDefinition>.NotApplicable();
        }

        var block = blocks[0];
        var issues = new List<ValidationIssue>();

        // Пытаемся извлечь по каждому типу комментария
        var result = TryExtractTableComment(block, issues)
                     ?? TryExtractColumnComment(block, issues)
                     ?? TryExtractFunctionComment(block, issues)
                     ?? TryExtractIndexComment(block, issues)
                     ?? TryExtractTriggerComment(block, issues)
                     ?? TryExtractConstraintComment(block, issues)
                     ?? TryExtractTypeComment(block, issues)
                     ?? TryExtractViewComment(block, issues);

        if (result is null)
        {
            return ExtractionResult<CommentDefinition>.Failure([
                ValidationIssue.Error(
                    ValidationIssue.ValidationDefinitionType.Comment,
                    "COMMENT_PARSE_ERROR",
                    "Failed to parse COMMENT ON statement. Unknown or unsupported object type",
                    new ValidationIssue.ValidationLocation
                    {
                        Segment = block.Content,
                        Line = block.StartLine,
                        Column = 0
                    }
                )
            ]);
        }

        // Проверяем валидность комментария
        if (string.IsNullOrWhiteSpace(result.Comment))
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Comment,
                "COMMENT_EMPTY",
                $"Comment for {result.ObjectType} '{result.Name}' is empty or whitespace only",
                new ValidationIssue.ValidationLocation
                {
                    Segment = block.Content,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        // Проверяем длину комментария
        if (result.Comment.Length > 1000)
        {
            issues.Add(ValidationIssue.Warning(
                ValidationIssue.ValidationDefinitionType.Comment,
                "COMMENT_TOO_LONG",
                $"Comment for {result.ObjectType} '{result.Name}' is very long ({result.Comment.Length} characters). Consider breaking it down",
                new ValidationIssue.ValidationLocation
                {
                    Segment = block.Content,
                    Line = block.StartLine,
                    Column = 0
                }
            ));
        }

        return ExtractionResult<CommentDefinition>.Success(result, issues);
    }

    // ============================================================================
    // Private Helper Methods
    // ============================================================================

    /// <summary>
    /// Пытается извлечь комментарий к таблице
    /// </summary>
    private static CommentDefinition? TryExtractTableComment(SqlBlock block, List<ValidationIssue> issues)
    {
        var match = TableCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var tableName = match.Groups["table"].Value;
        var comment = UnescapeSqlString(match.Groups["comment"].Value);

        return new CommentDefinition
        {
            ObjectType = SchemaObjectType.Tables,
            Name = tableName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Пытается извлечь комментарий к колонке
    /// </summary>
    private static CommentDefinition? TryExtractColumnComment(SqlBlock block, List<ValidationIssue> issues)
    {
        var match = ColumnCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var tableName = match.Groups["table"].Value;
        var columnName = match.Groups["column"].Value;
        var comment = UnescapeSqlString(match.Groups["comment"].Value);

        return new CommentDefinition
        {
            ObjectType = SchemaObjectType.Columns,
            Name = columnName,
            TableName = tableName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Пытается извлечь комментарий к функции
    /// </summary>
    private static CommentDefinition? TryExtractFunctionComment(SqlBlock block, List<ValidationIssue> issues)
    {
        var match = FunctionCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var functionName = match.Groups["function"].Value;
        var parameters = match.Groups["params"].Value.Trim();
        var comment = UnescapeSqlString(match.Groups["comment"].Value);

        var signature = string.IsNullOrEmpty(parameters) 
            ? $"{functionName}()" 
            : $"{functionName}({parameters})";

        return new CommentDefinition
        {
            ObjectType = SchemaObjectType.Functions,
            Name = functionName,
            FunctionSignature = signature,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Пытается извлечь комментарий к индексу
    /// </summary>
    private static CommentDefinition? TryExtractIndexComment(SqlBlock block, List<ValidationIssue> issues)
    {
        var match = IndexCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var indexName = match.Groups["index"].Value;
        var comment = UnescapeSqlString(match.Groups["comment"].Value);

        return new CommentDefinition
        {
            ObjectType = SchemaObjectType.Indexes,
            Name = indexName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Пытается извлечь комментарий к триггеру
    /// </summary>
    private static CommentDefinition? TryExtractTriggerComment(SqlBlock block, List<ValidationIssue> issues)
    {
        var match = TriggerCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var triggerName = match.Groups["trigger"].Value;
        var tableName = match.Groups["table"].Value;
        var comment = UnescapeSqlString(match.Groups["comment"].Value);

        return new CommentDefinition
        {
            ObjectType = SchemaObjectType.Triggers,
            Name = triggerName,
            TableName = tableName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Пытается извлечь комментарий к ограничению
    /// </summary>
    private static CommentDefinition? TryExtractConstraintComment(SqlBlock block, List<ValidationIssue> issues)
    {
        var match = ConstraintCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var constraintName = match.Groups["constraint"].Value;
        var tableName = match.Groups["table"].Value;
        var comment = UnescapeSqlString(match.Groups["comment"].Value);

        return new CommentDefinition
        {
            ObjectType = SchemaObjectType.Constraints,
            Name = constraintName,
            TableName = tableName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Пытается извлечь комментарий к типу
    /// </summary>
    private static CommentDefinition? TryExtractTypeComment(SqlBlock block, List<ValidationIssue> issues)
    {
        var match = TypeCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var typeName = match.Groups["type"].Value;
        var comment = UnescapeSqlString(match.Groups["comment"].Value);

        return new CommentDefinition
        {
            ObjectType = SchemaObjectType.Types,
            Name = typeName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Пытается извлечь комментарий к представлению
    /// </summary>
    private static CommentDefinition? TryExtractViewComment(SqlBlock block, List<ValidationIssue> issues)
    {
        var match = ViewCommentPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var schema = match.Groups["schema"].Success ? match.Groups["schema"].Value : null;
        var viewName = match.Groups["view"].Value;
        var comment = UnescapeSqlString(match.Groups["comment"].Value);

        return new CommentDefinition
        {
            ObjectType = SchemaObjectType.Views,
            Name = viewName,
            Comment = comment,
            Schema = schema,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Удаляет экранирование одинарных кавычек в SQL строках ('' -> ')
    /// </summary>
    private static string UnescapeSqlString(string value)
    {
        return value.Replace("''", "'");
    }
}
