using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.Utils;

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
            Tables = allTables.DeduplicateBy(t => new { t.Name, t.Schema }),
            Views = allViews.DeduplicateBy(v => new { v.Name, v.Schema }),
            Types = allTypes.DeduplicateBy(t => new { t.Name, t.Schema }),
            Functions = allFunctions.DeduplicateBy(f => new { f.Name, f.Schema, ParameterCount = f.Parameters.Count }),
            Indexes = allIndexes.DeduplicateBy(i => new { i.Name, i.Schema }),
            Triggers = allTriggers.DeduplicateBy(t => new { t.Name, t.TableName, t.Schema }),
            Constraints = allConstraints.DeduplicateBy(c => new { c.Name, c.TableName, c.Schema }),
            Comments = allComments.Count > 0 ? allComments : null,
            AnalyzedAt = DateTime.UtcNow
        };
    }
}