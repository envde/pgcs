using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Views;

namespace PgCs.SchemaGenerator.Services;

/// <summary>
/// Валидатор схемы PostgreSQL перед генерацией
/// </summary>
public sealed class SchemaValidator : ISchemaValidator
{
    public IReadOnlyList<ValidationIssue> Validate(SchemaMetadata schemaMetadata)
    {
        var issues = new List<ValidationIssue>();

        // Проверка на пустую схему
        if (!schemaMetadata.Tables.Any() &&
            !schemaMetadata.Views.Any() &&
            !schemaMetadata.Types.Any() &&
            !schemaMetadata.Functions.Any())
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Code = "EMPTY_SCHEMA",
                Message = "Schema is empty - no tables, views, types or functions found"
            });
        }

        // Проверка таблиц
        foreach (var table in schemaMetadata.Tables)
        {
            ValidateTable(table, issues);
        }

        // Проверка представлений
        foreach (var view in schemaMetadata.Views)
        {
            ValidateView(view, issues);
        }

        // Проверка типов
        foreach (var type in schemaMetadata.Types)
        {
            ValidateCustomType(type, issues);
        }

        // Проверка функций
        foreach (var function in schemaMetadata.Functions)
        {
            ValidateFunction(function, issues);
        }

        return issues;
    }

    private static void ValidateTable(TableDefinition table, List<ValidationIssue> issues)
    {
        // Проверка имени таблицы
        if (string.IsNullOrWhiteSpace(table.Name))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "EMPTY_TABLE_NAME",
                Message = "Table has empty name",
                Location = $"Schema: {table.Schema}"
            });
            return;
        }

        // Проверка наличия колонок
        if (!table.Columns.Any())
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Code = "TABLE_NO_COLUMNS",
                Message = $"Table '{table.Name}' has no columns",
                Location = $"{table.Schema}.{table.Name}"
            });
        }

        // Проверка колонок
        var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in table.Columns)
        {
            ValidateColumn(column, table, columnNames, issues);
        }

        // Проверка наличия первичного ключа
        if (!table.Columns.Any(c => c.IsPrimaryKey))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Info,
                Code = "TABLE_NO_PK",
                Message = $"Table '{table.Name}' has no primary key",
                Location = $"{table.Schema}.{table.Name}"
            });
        }
    }

    private static void ValidateColumn(
        ColumnDefinition column,
        TableDefinition table,
        HashSet<string> columnNames,
        List<ValidationIssue> issues)
    {
        // Проверка имени колонки
        if (string.IsNullOrWhiteSpace(column.Name))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "EMPTY_COLUMN_NAME",
                Message = "Column has empty name",
                Location = $"{table.Schema}.{table.Name}"
            });
            return;
        }

        // Проверка на дублирование имен
        if (!columnNames.Add(column.Name))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "DUPLICATE_COLUMN_NAME",
                Message = $"Duplicate column name '{column.Name}'",
                Location = $"{table.Schema}.{table.Name}"
            });
        }

        // Проверка типа данных
        if (string.IsNullOrWhiteSpace(column.DataType))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "EMPTY_COLUMN_TYPE",
                Message = $"Column '{column.Name}' has empty data type",
                Location = $"{table.Schema}.{table.Name}"
            });
        }
    }

    private static void ValidateView(ViewDefinition view, List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(view.Name))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "EMPTY_VIEW_NAME",
                Message = "View has empty name",
                Location = $"Schema: {view.Schema}"
            });
            return;
        }

        if (!view.Columns.Any())
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Code = "VIEW_NO_COLUMNS",
                Message = $"View '{view.Name}' has no columns",
                Location = $"{view.Schema}.{view.Name}"
            });
        }
    }

    private static void ValidateCustomType(TypeDefinition type, List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(type.Name))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "EMPTY_TYPE_NAME",
                Message = "Custom type has empty name",
                Location = $"Schema: {type.Schema}"
            });
            return;
        }

        // Проверка enum типа
        if (type.Kind == TypeKind.Enum && !type.EnumValues.Any())
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Code = "ENUM_NO_VALUES",
                Message = $"Enum type '{type.Name}' has no values",
                Location = $"{type.Schema}.{type.Name}"
            });
        }

        // Проверка composite типа
        if (type.Kind == TypeKind.Composite && !type.CompositeAttributes.Any())
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Code = "COMPOSITE_NO_ATTRIBUTES",
                Message = $"Composite type '{type.Name}' has no attributes",
                Location = $"{type.Schema}.{type.Name}"
            });
        }
    }

    private static void ValidateFunction(FunctionDefinition function, List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(function.Name))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "EMPTY_FUNCTION_NAME",
                Message = "Function has empty name",
                Location = $"Schema: {function.Schema}"
            });
            return;
        }

        // Проверка параметров
        var paramNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var param in function.Parameters)
        {
            if (string.IsNullOrWhiteSpace(param.Name))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Code = "FUNCTION_UNNAMED_PARAM",
                    Message = $"Function '{function.Name}' has unnamed parameter",
                    Location = $"{function.Schema}.{function.Name}"
                });
                continue;
            }

            if (!paramNames.Add(param.Name))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "FUNCTION_DUPLICATE_PARAM",
                    Message = $"Function '{function.Name}' has duplicate parameter '{param.Name}'",
                    Location = $"{function.Schema}.{function.Name}"
                });
            }
        }
    }
}
