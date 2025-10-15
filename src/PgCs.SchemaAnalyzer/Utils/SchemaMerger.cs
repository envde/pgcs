using PgCs.Common.SchemaAnalyzer;

namespace PgCs.SchemaAnalyzer.Utils;

/// <summary>
/// Объединяет несколько SchemaMetadata в одну
/// </summary>
internal static class SchemaMerger
{
    public static SchemaMetadata Merge(IEnumerable<SchemaMetadata> schemas)
    {
        var allTables = new List<TableDefinition>();
        var allViews = new List<ViewDefinition>();
        var allTypes = new List<TypeDefinition>();
        var allFunctions = new List<FunctionDefinition>();
        var allIndexes = new List<IndexDefinition>();
        var allTriggers = new List<TriggerDefinition>();
        var allConstraints = new List<ConstraintDefinition>();
        var allComments = new Dictionary<string, string>();

        foreach (var schema in schemas)
        {
            allTables.AddRange(schema.Tables);
            allViews.AddRange(schema.Views);
            allTypes.AddRange(schema.Types);
            allFunctions.AddRange(schema.Functions);
            allIndexes.AddRange(schema.Indexes);
            allTriggers.AddRange(schema.Triggers);
            allConstraints.AddRange(schema.Constraints);

            if (schema.Comments is not null)
            {
                foreach (var (key, value) in schema.Comments)
                {
                    allComments.TryAdd(key, value);
                }
            }
        }

        return new SchemaMetadata
        {
            Tables = DeduplicateTables(allTables),
            Views = DeduplicateViews(allViews),
            Types = DeduplicateTypes(allTypes),
            Functions = DeduplicateFunctions(allFunctions),
            Indexes = DeduplicateIndexes(allIndexes),
            Triggers = DeduplicateTriggers(allTriggers),
            Constraints = DeduplicateConstraints(allConstraints),
            Comments = allComments.Count > 0 ? allComments : null,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private static IReadOnlyList<TableDefinition> DeduplicateTables(List<TableDefinition> tables)
    {
        return tables
            .GroupBy(t => new { t.Name, t.Schema })
            .Select(g => g.First())
            .ToArray();
    }

    private static IReadOnlyList<ViewDefinition> DeduplicateViews(List<ViewDefinition> views)
    {
        return views
            .GroupBy(v => new { v.Name, v.Schema })
            .Select(g => g.First())
            .ToArray();
    }

    private static IReadOnlyList<TypeDefinition> DeduplicateTypes(List<TypeDefinition> types)
    {
        return types
            .GroupBy(t => new { t.Name, t.Schema })
            .Select(g => g.First())
            .ToArray();
    }

    private static IReadOnlyList<FunctionDefinition> DeduplicateFunctions(List<FunctionDefinition> functions)
    {
        return functions
            .GroupBy(f => new { f.Name, f.Schema, ParameterCount = f.Parameters.Count })
            .Select(g => g.First())
            .ToArray();
    }

    private static IReadOnlyList<IndexDefinition> DeduplicateIndexes(List<IndexDefinition> indexes)
    {
        return indexes
            .GroupBy(i => new { i.Name, i.Schema })
            .Select(g => g.First())
            .ToArray();
    }

    private static IReadOnlyList<TriggerDefinition> DeduplicateTriggers(List<TriggerDefinition> triggers)
    {
        return triggers
            .GroupBy(t => new { t.Name, t.TableName, t.Schema })
            .Select(g => g.First())
            .ToArray();
    }

    private static IReadOnlyList<ConstraintDefinition> DeduplicateConstraints(List<ConstraintDefinition> constraints)
    {
        return constraints
            .GroupBy(c => new { c.Name, c.TableName, c.Schema })
            .Select(g => g.First())
            .ToArray();
    }
}