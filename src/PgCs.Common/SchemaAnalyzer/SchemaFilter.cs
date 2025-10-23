using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Views;

namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Fluent API для фильтрации метаданных схемы базы данных
/// </summary>
public sealed class SchemaFilter
{
    private readonly SchemaMetadata _sourceMetadata;
    private HashSet<string>? _excludedSchemas;
    private HashSet<string>? _includedSchemas;
    private List<Regex>? _excludeTablePatterns;
    private List<Regex>? _includeTablePatterns;
    private List<Regex>? _excludeViewPatterns;
    private List<Regex>? _includeViewPatterns;
    private HashSet<TypeKind>? _includeOnlyTypeKinds;
    private bool _removeTables;
    private bool _removeViews;
    private bool _removeTypes;
    private bool _removeFunctions;
    private bool _removeIndexes;
    private bool _removeTriggers;
    private bool _removeConstraints;

    private SchemaFilter(SchemaMetadata sourceMetadata)
    {
        _sourceMetadata = sourceMetadata;
    }

    /// <summary>
    /// Создаёт новый фильтр на основе метаданных схемы
    /// </summary>
    public static SchemaFilter From(SchemaMetadata metadata) => new(metadata);

    /// <summary>
    /// Исключить указанные схемы
    /// </summary>
    public SchemaFilter ExcludeSchemas(params string[] schemas)
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
    public SchemaFilter IncludeOnlySchemas(params string[] schemas)
    {
        _includedSchemas ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var schema in schemas)
        {
            _includedSchemas.Add(schema);
        }
        return this;
    }

    /// <summary>
    /// Исключить таблицы по regex паттерну
    /// </summary>
    public SchemaFilter ExcludeTables(string pattern)
    {
        _excludeTablePatterns ??= new List<Regex>();
        _excludeTablePatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        return this;
    }

    /// <summary>
    /// Исключить таблицы по нескольким паттернам
    /// </summary>
    public SchemaFilter ExcludeTables(params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            ExcludeTables(pattern);
        }
        return this;
    }

    /// <summary>
    /// Включить только таблицы, соответствующие паттерну
    /// </summary>
    public SchemaFilter IncludeOnlyTables(string pattern)
    {
        _includeTablePatterns ??= new List<Regex>();
        _includeTablePatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        return this;
    }

    /// <summary>
    /// Включить только таблицы, соответствующие паттернам
    /// </summary>
    public SchemaFilter IncludeOnlyTables(params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            IncludeOnlyTables(pattern);
        }
        return this;
    }

    /// <summary>
    /// Исключить представления по regex паттерну
    /// </summary>
    public SchemaFilter ExcludeViews(string pattern)
    {
        _excludeViewPatterns ??= new List<Regex>();
        _excludeViewPatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        return this;
    }

    /// <summary>
    /// Включить только представления, соответствующие паттерну
    /// </summary>
    public SchemaFilter IncludeOnlyViews(string pattern)
    {
        _includeViewPatterns ??= new List<Regex>();
        _includeViewPatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        return this;
    }

    /// <summary>
    /// Включить только определённые виды типов
    /// </summary>
    public SchemaFilter IncludeOnlyTypes(params TypeKind[] kinds)
    {
        _includeOnlyTypeKinds ??= new HashSet<TypeKind>();
        foreach (var kind in kinds)
        {
            _includeOnlyTypeKinds.Add(kind);
        }
        return this;
    }

    /// <summary>
    /// Удалить системные объекты (pg_catalog, information_schema и т.д.)
    /// </summary>
    public SchemaFilter RemoveSystemObjects()
    {
        ExcludeSchemas("pg_catalog", "information_schema", "pg_toast");
        ExcludeTables("^pg_.*", "^sql_.*");
        return this;
    }

    /// <summary>
    /// Удалить все таблицы
    /// </summary>
    public SchemaFilter RemoveTables()
    {
        _removeTables = true;
        return this;
    }

    /// <summary>
    /// Удалить все представления
    /// </summary>
    public SchemaFilter RemoveViews()
    {
        _removeViews = true;
        return this;
    }

    /// <summary>
    /// Удалить все пользовательские типы
    /// </summary>
    public SchemaFilter RemoveTypes()
    {
        _removeTypes = true;
        return this;
    }

    /// <summary>
    /// Удалить все функции
    /// </summary>
    public SchemaFilter RemoveFunctions()
    {
        _removeFunctions = true;
        return this;
    }

    /// <summary>
    /// Удалить все индексы
    /// </summary>
    public SchemaFilter RemoveIndexes()
    {
        _removeIndexes = true;
        return this;
    }

    /// <summary>
    /// Удалить все триггеры
    /// </summary>
    public SchemaFilter RemoveTriggers()
    {
        _removeTriggers = true;
        return this;
    }

    /// <summary>
    /// Удалить все ограничения (constraints)
    /// </summary>
    public SchemaFilter RemoveConstraints()
    {
        _removeConstraints = true;
        return this;
    }

    /// <summary>
    /// Оставить только таблицы и представления
    /// </summary>
    public SchemaFilter OnlyTablesAndViews()
    {
        _removeTypes = true;
        _removeFunctions = true;
        _removeIndexes = true;
        _removeTriggers = true;
        return this;
    }

    /// <summary>
    /// Оставить только таблицы
    /// </summary>
    public SchemaFilter OnlyTables()
    {
        _removeViews = true;
        _removeTypes = true;
        _removeFunctions = true;
        _removeIndexes = true;
        _removeTriggers = true;
        return this;
    }

    /// <summary>
    /// Применяет фильтры и возвращает отфильтрованные метаданные
    /// </summary>
    public SchemaMetadata Build()
    {
        var filteredTables = FilterTables();
        var filteredViews = FilterViews();
        var filteredTypes = FilterTypes();
        var filteredFunctions = FilterFunctions();
        var filteredIndexes = FilterIndexes();
        var filteredTriggers = FilterTriggers();
        var filteredConstraints = FilterConstraints();

        return new SchemaMetadata
        {
            Tables = filteredTables,
            Views = filteredViews,
            Types = filteredTypes,
            Functions = filteredFunctions,
            Indexes = filteredIndexes,
            Triggers = filteredTriggers,
            Constraints = filteredConstraints,
            Comments = _sourceMetadata.Comments,
            SourceFile = _sourceMetadata.SourceFile,
            AnalyzedAt = _sourceMetadata.AnalyzedAt
        };
    }

    private IReadOnlyList<TableDefinition> FilterTables()
    {
        if (_removeTables) return Array.Empty<TableDefinition>();

        return _sourceMetadata.Tables
            .Where(t => !IsSchemaExcluded(t.Schema))
            .Where(t => IsSchemaIncluded(t.Schema))
            .Where(t => !IsTableExcluded(t.Name))
            .Where(t => IsTableIncluded(t.Name))
            .ToList();
    }

    private IReadOnlyList<ViewDefinition> FilterViews()
    {
        if (_removeViews) return Array.Empty<ViewDefinition>();

        return _sourceMetadata.Views
            .Where(v => !IsSchemaExcluded(v.Schema))
            .Where(v => IsSchemaIncluded(v.Schema))
            .Where(v => !IsViewExcluded(v.Name))
            .Where(v => IsViewIncluded(v.Name))
            .ToList();
    }

    private IReadOnlyList<TypeDefinition> FilterTypes()
    {
        if (_removeTypes) return Array.Empty<TypeDefinition>();

        var types = _sourceMetadata.Types
            .Where(t => !IsSchemaExcluded(t.Schema))
            .Where(t => IsSchemaIncluded(t.Schema));

        if (_includeOnlyTypeKinds != null)
        {
            types = types.Where(t => _includeOnlyTypeKinds.Contains(t.Kind));
        }

        return types.ToList();
    }

    private IReadOnlyList<FunctionDefinition> FilterFunctions()
    {
        if (_removeFunctions) return Array.Empty<FunctionDefinition>();

        return _sourceMetadata.Functions
            .Where(f => !IsSchemaExcluded(f.Schema))
            .Where(f => IsSchemaIncluded(f.Schema))
            .ToList();
    }

    private IReadOnlyList<IndexDefinition> FilterIndexes()
    {
        if (_removeIndexes) return Array.Empty<IndexDefinition>();

        return _sourceMetadata.Indexes
            .Where(i => !IsSchemaExcluded(i.Schema))
            .Where(i => IsSchemaIncluded(i.Schema))
            .ToList();
    }

    private IReadOnlyList<TriggerDefinition> FilterTriggers()
    {
        if (_removeTriggers) return Array.Empty<TriggerDefinition>();

        return _sourceMetadata.Triggers
            .Where(t => !IsSchemaExcluded(t.Schema))
            .Where(t => IsSchemaIncluded(t.Schema))
            .ToList();
    }

    private IReadOnlyList<ConstraintDefinition> FilterConstraints()
    {
        if (_removeConstraints) return Array.Empty<ConstraintDefinition>();

        return _sourceMetadata.Constraints
            .Where(c => !IsSchemaExcluded(c.Schema))
            .Where(c => IsSchemaIncluded(c.Schema))
            .ToList();
    }

    private bool IsSchemaExcluded(string? schema)
    {
        if (_excludedSchemas == null || schema == null) return false;
        return _excludedSchemas.Contains(schema);
    }

    private bool IsSchemaIncluded(string? schema)
    {
        if (_includedSchemas == null) return true;
        if (schema == null) return true; // public schema
        return _includedSchemas.Contains(schema);
    }

    private bool IsTableExcluded(string tableName)
    {
        if (_excludeTablePatterns == null) return false;
        return _excludeTablePatterns.Any(pattern => pattern.IsMatch(tableName));
    }

    private bool IsTableIncluded(string tableName)
    {
        if (_includeTablePatterns == null) return true;
        return _includeTablePatterns.Any(pattern => pattern.IsMatch(tableName));
    }

    private bool IsViewExcluded(string viewName)
    {
        if (_excludeViewPatterns == null) return false;
        return _excludeViewPatterns.Any(pattern => pattern.IsMatch(viewName));
    }

    private bool IsViewIncluded(string viewName)
    {
        if (_includeViewPatterns == null) return true;
        return _includeViewPatterns.Any(pattern => pattern.IsMatch(viewName));
    }
}
