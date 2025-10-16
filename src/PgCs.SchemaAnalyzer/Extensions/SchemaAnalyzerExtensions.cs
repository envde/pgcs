using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Views;

namespace PgCs.SchemaAnalyzer.Extensions;

/// <summary>
/// Методы расширения для работы со схемой
/// </summary>
public static class SchemaAnalyzerExtensions
{
    public static TableDefinition? FindTable(this SchemaMetadata schema, string tableName)
    {
        return schema.Tables.FirstOrDefault(t => 
            t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
    }

    public static ViewDefinition? FindView(this SchemaMetadata schema, string viewName)
    {
        return schema.Views.FirstOrDefault(v => 
            v.Name.Equals(viewName, StringComparison.OrdinalIgnoreCase));
    }

    public static TypeDefinition? FindType(this SchemaMetadata schema, string typeName)
    {
        return schema.Types.FirstOrDefault(t => 
            t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyList<IndexDefinition> GetTableIndexes(
        this SchemaMetadata schema, 
        string tableName)
    {
        return schema.Indexes
            .Where(i => i.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public static IReadOnlyList<TriggerDefinition> GetTableTriggers(
        this SchemaMetadata schema, 
        string tableName)
    {
        return schema.Triggers
            .Where(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public static IReadOnlyList<ConstraintDefinition> GetTableConstraints(
        this SchemaMetadata schema, 
        string tableName)
    {
        return schema.Constraints
            .Where(c => c.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public static IReadOnlyList<TableDefinition> GetTablesReferencingTable(
        this SchemaMetadata schema, 
        string tableName)
    {
        var referencingTableNames = schema.Constraints
            .Where(c => c.Type == ConstraintType.ForeignKey && 
                       c.ReferencedTable?.Equals(tableName, StringComparison.OrdinalIgnoreCase) == true)
            .Select(c => c.TableName)
            .Distinct()
            .ToHashSet();

        return schema.Tables
            .Where(t => referencingTableNames.Contains(t.Name))
            .ToArray();
    }

    public static bool HasColumn(this TableDefinition table, string columnName)
    {
        return table.Columns.Any(c => 
            c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
    }

    public static ColumnDefinition? GetColumn(this TableDefinition table, string columnName)
    {
        return table.Columns.FirstOrDefault(c => 
            c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyList<ColumnDefinition> GetPrimaryKeyColumns(this TableDefinition table)
    {
        return table.Columns.Where(c => c.IsPrimaryKey).ToArray();
    }

    public static IReadOnlyList<ColumnDefinition> GetRequiredColumns(this TableDefinition table)
    {
        return table.Columns.Where(c => !c.IsNullable && c.DefaultValue is null).ToArray();
    }
}