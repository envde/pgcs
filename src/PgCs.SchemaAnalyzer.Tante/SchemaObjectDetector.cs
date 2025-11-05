using System.Text.RegularExpressions;
using PgCs.Core.Schema.Common;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Детектор типа объекта схемы PostgreSQL по SQL блоку
/// </summary>
public static partial class SchemaObjectDetector
{
    // ============================================================================
    // Regex Patterns
    // ============================================================================
    
    // CREATE TYPE / DOMAIN
    [GeneratedRegex(@"\A\s*CREATE\s+TYPE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateTypePattern();

    [GeneratedRegex(@"\A\s*CREATE\s+DOMAIN\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateDomainPattern();

    // CREATE TABLE
    [GeneratedRegex(@"\A\s*CREATE\s+TABLE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateTablePattern();

    // CREATE TABLE ... PARTITION OF (must be checked before regular tables)
    [GeneratedRegex(@"\A\s*CREATE\s+TABLE\s+.*\s+PARTITION\s+OF\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex CreatePartitionPattern();

    // CREATE INDEX (объединено: обычные и уникальные индексы)
    [GeneratedRegex(@"\A\s*CREATE\s+(?:UNIQUE\s+)?INDEX\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateIndexPattern();

    // CREATE VIEW (объединено: обычные и материализованные)
    [GeneratedRegex(@"\A\s*CREATE\s+(?:MATERIALIZED\s+)?(?:OR\s+REPLACE\s+)?VIEW\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateViewPattern();

    // CREATE FUNCTION/PROCEDURE (объединено)
    [GeneratedRegex(@"\A\s*CREATE\s+(?:OR\s+REPLACE\s+)?(?:FUNCTION|PROCEDURE)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateFunctionPattern();

    // CREATE TRIGGER
    [GeneratedRegex(@"\A\s*CREATE\s+(?:OR\s+REPLACE\s+)?TRIGGER\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateTriggerPattern();

    // ALTER TABLE ADD CONSTRAINT
    [GeneratedRegex(@"\A\s*ALTER\s+TABLE\s+\S+\s+ADD\s+CONSTRAINT\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex AlterTableAddConstraintPattern();

    // COMMENT ON (объединено в один паттерн)
    [GeneratedRegex(@"\A\s*COMMENT\s+ON\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnPattern();

    // Дополнительные паттерны для уточнения типа COMMENT ON
    [GeneratedRegex(@"\A\s*COMMENT\s+ON\s+(TYPE|DOMAIN|TABLE|COLUMN|INDEX|VIEW|MATERIALIZED\s+VIEW|FUNCTION|PROCEDURE|TRIGGER|CONSTRAINT)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentOnObjectTypePattern();

    // ============================================================================
    // Public API: Методы детектирования типа объекта
    // ============================================================================

    /// <summary>
    /// Определяет тип объекта схемы PostgreSQL по SQL блоку
    /// </summary>
    /// <param name="sqlBlock">SQL блок для анализа</param>
    /// <returns>Тип объекта схемы</returns>
    public static SchemaObjectType DetectObjectType(string sqlBlock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlBlock);

        // Порядок проверки важен: более специфичные паттерны проверяются первыми
        
        // COMMENT ON - самый приоритетный, так как относится к метаданным объектов
        if (IsCommentOn(sqlBlock))
            return SchemaObjectType.Comments;

        // CREATE TABLE ... PARTITION OF - проверяем раньше обычных таблиц
        if (IsCreatePartition(sqlBlock))
            return SchemaObjectType.Partitions;

        // CREATE TABLE
        if (IsCreateTable(sqlBlock))
            return SchemaObjectType.Tables;

        // CREATE VIEW (включая материализованные)
        if (IsCreateView(sqlBlock))
            return SchemaObjectType.Views;

        // CREATE TYPE или CREATE DOMAIN
        if (IsCreateType(sqlBlock) || IsCreateDomain(sqlBlock))
            return SchemaObjectType.Types;

        // CREATE INDEX
        if (IsCreateIndex(sqlBlock))
            return SchemaObjectType.Indexes;

        // CREATE FUNCTION/PROCEDURE
        if (IsCreateFunction(sqlBlock))
            return SchemaObjectType.Functions;

        // CREATE TRIGGER
        if (IsCreateTrigger(sqlBlock))
            return SchemaObjectType.Triggers;

        // ALTER TABLE ADD CONSTRAINT
        if (IsAlterTableAddConstraint(sqlBlock))
            return SchemaObjectType.Constraints;

        return SchemaObjectType.None;
    }

    // ============================================================================
    // Private: Методы проверки типа объекта
    // ============================================================================

    /// <summary>
    /// Проверяет, является ли блок определением TYPE (ENUM, композитный тип)
    /// </summary>
    private static bool IsCreateType(string sqlBlock)
        => CreateTypePattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок определением DOMAIN
    /// </summary>
    private static bool IsCreateDomain(string sqlBlock)
        => CreateDomainPattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок определением TABLE
    /// </summary>
    private static bool IsCreateTable(string sqlBlock)
        => CreateTablePattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок определением PARTITION
    /// </summary>
    private static bool IsCreatePartition(string sqlBlock)
        => CreatePartitionPattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок определением INDEX (включая UNIQUE)
    /// </summary>
    private static bool IsCreateIndex(string sqlBlock)
        => CreateIndexPattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок определением VIEW (включая MATERIALIZED VIEW)
    /// </summary>
    private static bool IsCreateView(string sqlBlock)
        => CreateViewPattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок определением FUNCTION или PROCEDURE
    /// </summary>
    private static bool IsCreateFunction(string sqlBlock)
        => CreateFunctionPattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок определением TRIGGER
    /// </summary>
    private static bool IsCreateTrigger(string sqlBlock)
        => CreateTriggerPattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок командой ALTER TABLE ADD CONSTRAINT
    /// </summary>
    private static bool IsAlterTableAddConstraint(string sqlBlock)
        => AlterTableAddConstraintPattern().IsMatch(sqlBlock);

    /// <summary>
    /// Проверяет, является ли блок комментарием COMMENT ON
    /// </summary>
    private static bool IsCommentOn(string sqlBlock)
        => CommentOnPattern().IsMatch(sqlBlock);

    /// <summary>
    /// Извлекает тип объекта из команды COMMENT ON
    /// Возвращает тип объекта (TYPE, TABLE, COLUMN и т.д.)
    /// </summary>
    public static string? ExtractCommentOnObjectType(string sqlBlock)
    {
        var match = CommentOnObjectTypePattern().Match(sqlBlock);
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }
}