using PgCs.Core.SchemaAnalyzer;
using PgCs.Core.SchemaAnalyzer.Definitions;
using PgCs.Core.SchemaAnalyzer.Metadata;
using PgCs.Core.SchemaAnalyzer.Options;

namespace PgCs.SchemaAnalyzer.Tante;

public class SchemaAnalyzer: ISchemaAnalyzer
{
    public async ValueTask<SchemaMetadata> AnalyzeFileAsync(string schemaFilePath, SchemaAnalysisOptions options,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<SchemaMetadata> AnalyzeDirectoryAsync(string schemaDirectoryPath, SchemaAnalysisOptions options,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<TableDefinition> ExtractTables(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<ViewDefinition> ExtractViews(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<TypeDefinition> ExtractTypes(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<FunctionDefinition> ExtractFunctions(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<IndexDefinition> ExtractIndexes(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<TriggerDefinition> ExtractTriggers(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<ConstraintDefinition> ExtractConstraints(string sqlScript)
    {
        throw new NotImplementedException();
    }
}