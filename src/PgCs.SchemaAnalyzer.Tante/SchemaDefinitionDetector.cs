using System.Text.RegularExpressions;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Детектор типа объекта схемы PostgreSQL по SQL блоку
/// </summary>
public static partial class SchemaDefinitionDetector
{
    // ============================================================================
    // CREATE TYPE
    // ============================================================================
    
    [GeneratedRegex(@"^\s*CREATE\s+TYPE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateTypePattern();

    [GeneratedRegex(@"\s+AS\s+ENUM\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EnumDefinitionPattern();

    [GeneratedRegex(@"\s+AS\s*\([^)]*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CompositeTypePattern();

    // ============================================================================
    // CREATE DOMAIN
    // ============================================================================
    
    [GeneratedRegex(@"^\s*CREATE\s+DOMAIN\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateDomainPattern();

    // ============================================================================
    // CREATE TABLE
    // ============================================================================
    
    [GeneratedRegex(@"^\s*CREATE\s+TABLE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateTablePattern();

    [GeneratedRegex(@"\s+PARTITION\s+BY\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex PartitionedTablePattern();

    [GeneratedRegex(@"^\s*CREATE\s+TABLE\s+\S+\s+PARTITION\s+OF\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TablePartitionPattern();

    // ============================================================================
    // CREATE INDEX
    // ============================================================================
    
    [GeneratedRegex(@"^\s*CREATE\s+(?:UNIQUE\s+)?INDEX\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateIndexPattern();

    [GeneratedRegex(@"^\s*CREATE\s+UNIQUE\s+INDEX\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex UniqueIndexPattern();

    [GeneratedRegex(@"\s+USING\s+(BTREE|HASH|GIN|GIST|SPGIST|BRIN)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex IndexMethodPattern();

    // ============================================================================
    // CREATE VIEW
    // ============================================================================
    
    [GeneratedRegex(@"^\s*CREATE\s+(?:OR\s+REPLACE\s+)?VIEW\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateViewPattern();

    [GeneratedRegex(@"^\s*CREATE\s+MATERIALIZED\s+VIEW\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateMaterializedViewPattern();

    // ============================================================================
    // CREATE FUNCTION / PROCEDURE
    // ============================================================================
    
    [GeneratedRegex(@"^\s*CREATE\s+(?:OR\s+REPLACE\s+)?FUNCTION\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateFunctionPattern();

    [GeneratedRegex(@"^\s*CREATE\s+(?:OR\s+REPLACE\s+)?PROCEDURE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateProcedurePattern();

    [GeneratedRegex(@"\s+RETURNS\s+TRIGGER\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TriggerFunctionPattern();

    [GeneratedRegex(@"\$\$\s+LANGUAGE\s+(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex FunctionLanguagePattern();

    // ============================================================================
    // CREATE TRIGGER
    // ============================================================================
    
    [GeneratedRegex(@"^\s*CREATE\s+(?:OR\s+REPLACE\s+)?TRIGGER\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateTriggerPattern();

    [GeneratedRegex(@"\s+(BEFORE|AFTER|INSTEAD\s+OF)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TriggerTimingPattern();

    [GeneratedRegex(@"\s+(INSERT|UPDATE|DELETE|TRUNCATE)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TriggerEventPattern();

    // ============================================================================
    // CREATE CONSTRAINT
    // ============================================================================
    
    [GeneratedRegex(@"^\s*ALTER\s+TABLE\s+\S+\s+ADD\s+CONSTRAINT\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex AlterTableAddConstraintPattern();

    [GeneratedRegex(@"\s+FOREIGN\s+KEY\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ForeignKeyConstraintPattern();

    [GeneratedRegex(@"\s+PRIMARY\s+KEY\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex PrimaryKeyConstraintPattern();

    [GeneratedRegex(@"\s+CHECK\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CheckConstraintPattern();

    [GeneratedRegex(@"\s+UNIQUE\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex UniqueConstraintPattern();

    // ============================================================================
    // COMMENT ON
    // ============================================================================
    
    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+TYPE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentTypePattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+DOMAIN\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnDomainPattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+TABLE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnTablePattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+COLUMN\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnColumnPattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+INDEX\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnIndexPattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+(?:MATERIALIZED\s+)?VIEW\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnViewPattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+FUNCTION\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnFunctionPattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+PROCEDURE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnProcedurePattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+TRIGGER\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnTriggerPattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+CONSTRAINT\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnConstraintPattern();

    // ============================================================================
    // INSERT / UPDATE / DELETE (DML)
    // ============================================================================
    
    [GeneratedRegex(@"^\s*INSERT\s+INTO\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex InsertPattern();

    [GeneratedRegex(@"^\s*UPDATE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex UpdatePattern();

    [GeneratedRegex(@"^\s*DELETE\s+FROM\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DeletePattern();

    // ============================================================================
    // OTHER CREATE STATEMENTS
    // ============================================================================
    
    [GeneratedRegex(@"^\s*CREATE\s+(?:OR\s+REPLACE\s+)?SEQUENCE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateSequencePattern();

    [GeneratedRegex(@"^\s*CREATE\s+SCHEMA\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateSchemaPattern();

    [GeneratedRegex(@"^\s*CREATE\s+EXTENSION\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateExtensionPattern();

    [GeneratedRegex(@"^\s*CREATE\s+(?:UNIQUE\s+)?(?:CONSTRAINT\s+)?(?:PRIMARY\s+KEY|FOREIGN\s+KEY|CHECK|UNIQUE)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateConstraintPattern();
}
