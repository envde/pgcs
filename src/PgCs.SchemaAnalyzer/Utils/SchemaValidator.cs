using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Tables;

namespace PgCs.SchemaAnalyzer.Utils;

/// <summary>
/// Валидатор схемы базы данных
/// </summary>
public static class SchemaValidator
{
    public static ValidationResult Validate(SchemaMetadata schema)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        ValidateTables(schema.Tables, errors, warnings);
        ValidateForeignKeys(schema, errors);
        ValidateIndexes(schema, errors, warnings);
        ValidateTriggers(schema, errors);

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    private static void ValidateTables(
        IReadOnlyList<TableDefinition> tables, 
        List<string> errors, 
        List<string> warnings)
    {
        foreach (var table in tables)
        {
            if (table.Columns.Count == 0)
            {
                errors.Add($"Table '{table.Name}' has no columns");
            }

            var hasPrimaryKey = table.Columns.Any(c => c.IsPrimaryKey) ||
                               table.Constraints.Any(c => c.Type == ConstraintType.PrimaryKey);

            if (!hasPrimaryKey)
            {
                warnings.Add($"Table '{table.Name}' has no primary key");
            }

            // Проверка дубликатов имен колонок
            var duplicateColumns = table.Columns
                .GroupBy(c => c.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicate in duplicateColumns)
            {
                errors.Add($"Table '{table.Name}' has duplicate column '{duplicate}'");
            }
        }
    }

    private static void ValidateForeignKeys(SchemaMetadata schema, List<string> errors)
    {
        var tableNames = schema.Tables.Select(t => t.Name).ToHashSet();

        foreach (var constraint in schema.Constraints.Where(c => c.Type == ConstraintType.ForeignKey))
        {
            if (constraint.ReferencedTable is not null && !tableNames.Contains(constraint.ReferencedTable))
            {
                errors.Add($"Foreign key '{constraint.Name}' references non-existent table '{constraint.ReferencedTable}'");
            }
        }
    }

    private static void ValidateIndexes(
        SchemaMetadata schema, 
        List<string> errors, 
        List<string> warnings)
    {
        var tableNames = schema.Tables.Select(t => t.Name).ToHashSet();

        foreach (var index in schema.Indexes)
        {
            if (!tableNames.Contains(index.TableName))
            {
                errors.Add($"Index '{index.Name}' references non-existent table '{index.TableName}'");
            }

            if (index.Columns.Count == 0)
            {
                errors.Add($"Index '{index.Name}' has no columns");
            }
        }
    }

    private static void ValidateTriggers(SchemaMetadata schema, List<string> errors)
    {
        var tableNames = schema.Tables.Select(t => t.Name).ToHashSet();
        var functionNames = schema.Functions.Select(f => f.Name).ToHashSet();

        foreach (var trigger in schema.Triggers)
        {
            if (!tableNames.Contains(trigger.TableName))
            {
                errors.Add($"Trigger '{trigger.Name}' references non-existent table '{trigger.TableName}'");
            }

            if (!functionNames.Contains(trigger.FunctionName))
            {
                errors.Add($"Trigger '{trigger.Name}' references non-existent function '{trigger.FunctionName}'");
            }
        }
    }
}

public sealed record ValidationResult
{
    public required bool IsValid { get; init; }
    public required IReadOnlyList<string> Errors { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
}