

using PgCs.Core.Definitions.Schema;
using PgCs.Core.SchemaAnalyzer;

namespace PgCs.SchemaAnalyzer.Tante;

public class SchemaAnalyzer : ISchemaAnalyzer
{
    public async ValueTask<SchemaMetadata> AnalyzeFileAsync(string schemaFilePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<SchemaMetadata> AnalyzeDirectoryAsync(string schemaDirectoryPath, CancellationToken cancellationToken = default)
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

    public IReadOnlyList<EnumTypeDefinition> ExtractEnums(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<DomainTypeDefinition> ExtractDomains(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<CompositeTypeDefinition> ExtractComposites(string sqlScript)
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