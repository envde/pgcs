using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает автономные определения ограничений (ALTER TABLE ... ADD CONSTRAINT)
/// </summary>
internal sealed partial class ConstraintExtractor : BaseExtractor<ConstraintDefinition>
{
    [GeneratedRegex(@"ALTER\s+TABLE\s+(?:ONLY\s+)?([a-zA-Z_][a-zA-Z0-9_.]*)\s+ADD\s+CONSTRAINT\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+(.*)", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex ConstraintPatternRegex();

    protected override Regex Pattern => ConstraintPatternRegex();

    protected override ConstraintDefinition? ParseMatch(Match match, string statement)
    {
        var fullTableName = match.Groups[1].Value.Trim();
        var constraintName = match.Groups[2].Value.Trim();
        var constraintBody = match.Groups[3].Value.Trim();

        var schema = ExtractSchemaName(fullTableName);
        var tableName = ExtractTableName(fullTableName);

        return ParseConstraintBody(constraintName, tableName, schema, constraintBody, statement);
    }

    private ConstraintDefinition? ParseConstraintBody(
        string name, 
        string tableName, 
        string? schema, 
        string body, 
        string rawSql)
    {
        // PRIMARY KEY
        var pkMatch = Regex.Match(body, @"PRIMARY\s+KEY\s*\((.*?)\)", RegexOptions.IgnoreCase);
        if (pkMatch.Success)
        {
            var columns = ParseColumns(pkMatch.Groups[1].Value);
            return new ConstraintDefinition
            {
                Name = name,
                TableName = tableName,
                Schema = schema,
                Type = ConstraintType.PrimaryKey,
                Columns = columns,
                RawSql = rawSql
            };
        }

        // FOREIGN KEY
        var fkMatch = Regex.Match(body, 
            @"FOREIGN\s+KEY\s*\((.*?)\)\s+REFERENCES\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s*\((.*?)\)(?:\s+ON\s+DELETE\s+(CASCADE|RESTRICT|SET NULL|SET DEFAULT|NO ACTION))?(?:\s+ON\s+UPDATE\s+(CASCADE|RESTRICT|SET NULL|SET DEFAULT|NO ACTION))?", 
            RegexOptions.IgnoreCase);
        if (fkMatch.Success)
        {
            var columns = ParseColumns(fkMatch.Groups[1].Value);
            var referencedTable = fkMatch.Groups[2].Value.Trim();
            var referencedColumns = ParseColumns(fkMatch.Groups[3].Value);
            var onDelete = fkMatch.Groups[4].Value;
            var onUpdate = fkMatch.Groups[5].Value;

            return new ConstraintDefinition
            {
                Name = name,
                TableName = tableName,
                Schema = schema,
                Type = ConstraintType.ForeignKey,
                Columns = columns,
                ReferencedTable = referencedTable,
                ReferencedColumns = referencedColumns,
                OnDelete = ParseReferentialAction(onDelete),
                OnUpdate = ParseReferentialAction(onUpdate),
                RawSql = rawSql
            };
        }

        // UNIQUE
        var uniqueMatch = Regex.Match(body, @"UNIQUE\s*\((.*?)\)", RegexOptions.IgnoreCase);
        if (uniqueMatch.Success)
        {
            var columns = ParseColumns(uniqueMatch.Groups[1].Value);
            return new ConstraintDefinition
            {
                Name = name,
                TableName = tableName,
                Schema = schema,
                Type = ConstraintType.Unique,
                Columns = columns,
                RawSql = rawSql
            };
        }

        // CHECK
        var checkMatch = Regex.Match(body, @"CHECK\s*\((.*)\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (checkMatch.Success)
        {
            return new ConstraintDefinition
            {
                Name = name,
                TableName = tableName,
                Schema = schema,
                Type = ConstraintType.Check,
                CheckExpression = checkMatch.Groups[1].Value,
                RawSql = rawSql
            };
        }

        return null;
    }

    private static IReadOnlyList<string> ParseColumns(string columnsText)
    {
        return columnsText
            .Split(',')
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToArray();
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
}