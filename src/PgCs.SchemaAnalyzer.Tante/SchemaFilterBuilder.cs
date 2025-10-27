using PgCs.Core.Definitions.Schema.Base;
using PgCs.Core.SchemaAnalyzer;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Fluent API для построения настроек анализа схемы
/// </summary>
public sealed class SchemaFilterBuilder : ISchemaFilterBuilder
{
    public ISchemaFilterBuilder ExcludeSchemas(params string[] schemas)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder IncludeOnlySchemas(params string[] schemas)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder ExcludeTables(params string[] patterns)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder IncludeOnlyTables(params string[] patterns)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder ExcludeViews(params string[] patterns)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder IncludeOnlyViews(params string[] patterns)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder IncludeOnlyTypes(params TypeKind[] kinds)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder ExcludeSystemObjects()
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder WithObjects(params SchemaObjectType[] objectTypes)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder OnlyTables()
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder OnlyTablesAndViews()
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder WithDependencyDepth(int depth)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder WithStrictMode(bool enabled = true)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilterBuilder WithCommentParsing(bool enabled = true)
    {
        throw new NotImplementedException();
    }

    public ISchemaFilter Build()
    {
        throw new NotImplementedException();
    }
}