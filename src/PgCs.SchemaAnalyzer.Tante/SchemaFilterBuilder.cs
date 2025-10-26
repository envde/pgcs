using PgCs.Core.SchemaAnalyzer;
using PgCs.Core.SchemaAnalyzer.Definitions.Base;
using PgCs.Core.SchemaAnalyzer.Options;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Fluent API для построения настроек анализа схемы
/// </summary>
public sealed class SchemaFilterBuilder : ISchemaFilterBuilder
{
    private HashSet<string>? _includedSchemas;
    private HashSet<string>? _excludedSchemas;
    private List<string>? _includeTablePatterns;
    private List<string>? _excludeTablePatterns;
    private List<string>? _includeViewPatterns;
    private List<string>? _excludeViewPatterns;
    private HashSet<TypeKind>? _includedTypeKinds;
    private HashSet<SchemaAnalysisOptions.SchemaObjectType>? _objectsToAnalyze;
    private bool _excludeSystemObjects;

    private SchemaFilterBuilder() { }

    /// <summary>
    /// Создать новый билдер настроек
    /// </summary>
    public static ISchemaFilterBuilder Create() => new SchemaFilterBuilder();

    /// <summary>
    /// Исключить указанные схемы
    /// </summary>
    public ISchemaFilterBuilder ExcludeSchemas(params string[] schemas)
    {
        _excludedSchemas ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var schema in schemas)
        {
            _excludedSchemas.Add(schema);
        }
        return this;
    }

    /// <summary>
    /// Включить только указанные схемы
    /// </summary>
    public ISchemaFilterBuilder IncludeOnlySchemas(params string[] schemas)
    {
        _includedSchemas ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var schema in schemas)
        {
            _includedSchemas.Add(schema);
        }
        return this;
    }

    /// <summary>
    /// Исключить таблицы по regex паттернам
    /// </summary>
    public ISchemaFilterBuilder ExcludeTables(params string[] patterns)
    {
        _excludeTablePatterns ??= [];
        _excludeTablePatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Включить только таблицы, соответствующие паттернам
    /// </summary>
    public ISchemaFilterBuilder IncludeOnlyTables(params string[] patterns)
    {
        _includeTablePatterns ??= [];
        _includeTablePatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Исключить представления по regex паттернам
    /// </summary>
    public ISchemaFilterBuilder ExcludeViews(params string[] patterns)
    {
        _excludeViewPatterns ??= [];
        _excludeViewPatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Включить только представления, соответствующие паттернам
    /// </summary>
    public ISchemaFilterBuilder IncludeOnlyViews(params string[] patterns)
    {
        _includeViewPatterns ??= [];
        _includeViewPatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Включить только определённые виды типов
    /// </summary>
    public ISchemaFilterBuilder IncludeOnlyTypes(params TypeKind[] kinds)
    {
        _includedTypeKinds ??= new HashSet<TypeKind>();
        foreach (var kind in kinds)
        {
            _includedTypeKinds.Add(kind);
        }
        return this;
    }

    /// <summary>
    /// Исключить системные объекты (pg_catalog, information_schema и т.д.)
    /// </summary>
    public ISchemaFilterBuilder ExcludeSystemObjects()
    {
        _excludeSystemObjects = true;
        ExcludeSchemas("pg_catalog", "information_schema", "pg_toast");
        ExcludeTables("^pg_.*", "^sql_.*");
        return this;
    }

    /// <summary>
    /// Указать, какие объекты анализировать
    /// </summary>
    public ISchemaFilterBuilder WithObjects(params SchemaAnalysisOptions.SchemaObjectType[] objectTypes)
    {
        _objectsToAnalyze ??= new HashSet<SchemaAnalysisOptions.SchemaObjectType>();
        foreach (var objectType in objectTypes)
        {
            if (objectType != SchemaAnalysisOptions.SchemaObjectType.None)
            {
                _objectsToAnalyze.Add(objectType);
            }
        }
        return this;
    }

    /// <summary>
    /// Анализировать только таблицы
    /// </summary>
    public ISchemaFilterBuilder OnlyTables()
    {
        return WithObjects(
            SchemaAnalysisOptions.SchemaObjectType.Tables,
            SchemaAnalysisOptions.SchemaObjectType.Indexes,
            SchemaAnalysisOptions.SchemaObjectType.Constraints,
            SchemaAnalysisOptions.SchemaObjectType.Triggers
        );
    }

    /// <summary>
    /// Анализировать только таблицы и представления
    /// </summary>
    public ISchemaFilterBuilder OnlyTablesAndViews()
    {
        return WithObjects(
            SchemaAnalysisOptions.SchemaObjectType.Tables,
            SchemaAnalysisOptions.SchemaObjectType.Views
        );
    }

    /// <summary>
    /// Построить объект настроек
    /// </summary>
    public SchemaAnalysisOptions Build()
    {
        return new SchemaAnalysisOptions
        {
            IncludedSchemas = _includedSchemas,
            ExcludedSchemas = _excludedSchemas,
            IncludeTablePatterns = _includeTablePatterns,
            ExcludeTablePatterns = _excludeTablePatterns,
            IncludeViewPatterns = _includeViewPatterns,
            ExcludeViewPatterns = _excludeViewPatterns,
            IncludedTypeKinds = _includedTypeKinds,
            ObjectsToAnalyze = _objectsToAnalyze,
            ExcludeSystemObjects = _excludeSystemObjects,
        };
    }
}