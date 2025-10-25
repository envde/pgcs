using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;

namespace PgCs.SchemaAnalyzer;

/// <summary>
/// Fluent API builder для анализа схемы PostgreSQL
/// </summary>
public class SchemaAnalyzerBuilder
{
    private readonly List<string> _filePaths = new();
    private readonly List<string> _directoryPaths = new();
    private readonly List<string> _sqlScripts = new();
    private bool _extractTables = true;
    private bool _extractViews = true;
    private bool _extractTypes = true;
    private bool _extractFunctions = true;
    private bool _extractIndexes = true;
    private bool _extractTriggers = true;
    private bool _extractConstraints = true;
    private bool _extractComments = true;
    private bool _removeDuplicates = true;
    private readonly List<string> _excludeSchemas = new();
    private readonly List<string> _includeOnlySchemas = new();
    private readonly List<string> _excludeTablePatterns = new();
    private readonly List<string> _includeOnlyTablePatterns = new();

    private SchemaAnalyzerBuilder() { }

    /// <summary>
    /// Создать новый builder для анализа схемы
    /// </summary>
    public static SchemaAnalyzerBuilder Create() => new();

    /// <summary>
    /// Анализировать SQL файл
    /// </summary>
    public SchemaAnalyzerBuilder FromFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePaths.Add(filePath);
        return this;
    }

    /// <summary>
    /// Анализировать несколько SQL файлов
    /// </summary>
    public SchemaAnalyzerBuilder FromFiles(params string[] filePaths)
    {
        ArgumentNullException.ThrowIfNull(filePaths);
        _filePaths.AddRange(filePaths);
        return this;
    }

    /// <summary>
    /// Анализировать все SQL файлы в директории (рекурсивно)
    /// </summary>
    public SchemaAnalyzerBuilder FromDirectory(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        _directoryPaths.Add(directoryPath);
        return this;
    }

    /// <summary>
    /// Анализировать несколько директорий
    /// </summary>
    public SchemaAnalyzerBuilder FromDirectories(params string[] directoryPaths)
    {
        ArgumentNullException.ThrowIfNull(directoryPaths);
        _directoryPaths.AddRange(directoryPaths);
        return this;
    }

    /// <summary>
    /// Анализировать SQL скрипт напрямую
    /// </summary>
    public SchemaAnalyzerBuilder FromScript(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        _sqlScripts.Add(sqlScript);
        return this;
    }

    /// <summary>
    /// Извлекать таблицы (по умолчанию: true)
    /// </summary>
    public SchemaAnalyzerBuilder WithTables(bool extract = true)
    {
        _extractTables = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать таблицы
    /// </summary>
    public SchemaAnalyzerBuilder WithoutTables()
    {
        _extractTables = false;
        return this;
    }

    /// <summary>
    /// Извлекать представления (по умолчанию: true)
    /// </summary>
    public SchemaAnalyzerBuilder WithViews(bool extract = true)
    {
        _extractViews = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать представления
    /// </summary>
    public SchemaAnalyzerBuilder WithoutViews()
    {
        _extractViews = false;
        return this;
    }

    /// <summary>
    /// Извлекать типы (ENUM, COMPOSITE, DOMAIN)
    /// </summary>
    public SchemaAnalyzerBuilder WithTypes(bool extract = true)
    {
        _extractTypes = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать типы
    /// </summary>
    public SchemaAnalyzerBuilder WithoutTypes()
    {
        _extractTypes = false;
        return this;
    }

    /// <summary>
    /// Извлекать функции и процедуры
    /// </summary>
    public SchemaAnalyzerBuilder WithFunctions(bool extract = true)
    {
        _extractFunctions = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать функции
    /// </summary>
    public SchemaAnalyzerBuilder WithoutFunctions()
    {
        _extractFunctions = false;
        return this;
    }

    /// <summary>
    /// Извлекать индексы
    /// </summary>
    public SchemaAnalyzerBuilder WithIndexes(bool extract = true)
    {
        _extractIndexes = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать индексы
    /// </summary>
    public SchemaAnalyzerBuilder WithoutIndexes()
    {
        _extractIndexes = false;
        return this;
    }

    /// <summary>
    /// Извлекать триггеры
    /// </summary>
    public SchemaAnalyzerBuilder WithTriggers(bool extract = true)
    {
        _extractTriggers = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать триггеры
    /// </summary>
    public SchemaAnalyzerBuilder WithoutTriggers()
    {
        _extractTriggers = false;
        return this;
    }

    /// <summary>
    /// Извлекать ограничения (CHECK, UNIQUE, FOREIGN KEY)
    /// </summary>
    public SchemaAnalyzerBuilder WithConstraints(bool extract = true)
    {
        _extractConstraints = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать ограничения
    /// </summary>
    public SchemaAnalyzerBuilder WithoutConstraints()
    {
        _extractConstraints = false;
        return this;
    }

    /// <summary>
    /// Извлекать комментарии (COMMENT ON)
    /// </summary>
    public SchemaAnalyzerBuilder WithComments(bool extract = true)
    {
        _extractComments = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать комментарии
    /// </summary>
    public SchemaAnalyzerBuilder WithoutComments()
    {
        _extractComments = false;
        return this;
    }

    /// <summary>
    /// Извлекать только таблицы (отключить всё остальное)
    /// </summary>
    public SchemaAnalyzerBuilder OnlyTables()
    {
        _extractTables = true;
        _extractViews = false;
        _extractTypes = false;
        _extractFunctions = false;
        _extractIndexes = false;
        _extractTriggers = false;
        _extractConstraints = false;
        return this;
    }

    /// <summary>
    /// Извлекать только таблицы и представления
    /// </summary>
    public SchemaAnalyzerBuilder OnlyTablesAndViews()
    {
        _extractTables = true;
        _extractViews = true;
        _extractTypes = false;
        _extractFunctions = false;
        _extractIndexes = false;
        _extractTriggers = false;
        _extractConstraints = false;
        return this;
    }

    /// <summary>
    /// Извлекать только типы
    /// </summary>
    public SchemaAnalyzerBuilder OnlyTypes()
    {
        _extractTables = false;
        _extractViews = false;
        _extractTypes = true;
        _extractFunctions = false;
        _extractIndexes = false;
        _extractTriggers = false;
        _extractConstraints = false;
        return this;
    }

    /// <summary>
    /// Исключить системные схемы (pg_catalog, information_schema, pg_toast)
    /// </summary>
    public SchemaAnalyzerBuilder ExcludeSystemSchemas()
    {
        _excludeSchemas.Add("pg_catalog");
        _excludeSchemas.Add("information_schema");
        _excludeSchemas.Add("pg_toast");
        _excludeSchemas.Add("pg_temp");
        return this;
    }

    /// <summary>
    /// Исключить определённые схемы
    /// </summary>
    public SchemaAnalyzerBuilder ExcludeSchemas(params string[] schemas)
    {
        ArgumentNullException.ThrowIfNull(schemas);
        _excludeSchemas.AddRange(schemas);
        return this;
    }

    /// <summary>
    /// Анализировать только указанные схемы
    /// </summary>
    public SchemaAnalyzerBuilder IncludeOnlySchemas(params string[] schemas)
    {
        ArgumentNullException.ThrowIfNull(schemas);
        _includeOnlySchemas.AddRange(schemas);
        return this;
    }

    /// <summary>
    /// Исключить таблицы по regex паттернам
    /// </summary>
    public SchemaAnalyzerBuilder ExcludeTables(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        _excludeTablePatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Анализировать только таблицы, соответствующие паттернам
    /// </summary>
    public SchemaAnalyzerBuilder IncludeOnlyTables(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        _includeOnlyTablePatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Удалять дубликаты объектов (по умолчанию: true)
    /// </summary>
    public SchemaAnalyzerBuilder RemoveDuplicates(bool remove = true)
    {
        _removeDuplicates = remove;
        return this;
    }

    /// <summary>
    /// Сохранять дубликаты объектов
    /// </summary>
    public SchemaAnalyzerBuilder KeepDuplicates()
    {
        _removeDuplicates = false;
        return this;
    }

    /// <summary>
    /// Выполнить анализ схемы
    /// </summary>
    public async ValueTask<SchemaMetadata> AnalyzeAsync()
    {
        if (_filePaths.Count == 0 && _directoryPaths.Count == 0 && _sqlScripts.Count == 0)
        {
            throw new InvalidOperationException(
                "No source specified. Use FromFile(), FromDirectory(), or FromScript().");
        }

        var analyzer = new SchemaAnalyzer();
        var allMetadata = new List<SchemaMetadata>();

        // Анализ файлов
        foreach (var filePath in _filePaths)
        {
            var metadata = await analyzer.AnalyzeFileAsync(filePath);
            allMetadata.Add(metadata);
        }

        // Анализ директорий
        foreach (var directoryPath in _directoryPaths)
        {
            var metadata = await analyzer.AnalyzeDirectoryAsync(directoryPath);
            allMetadata.Add(metadata);
        }

        // Анализ скриптов
        foreach (var script in _sqlScripts)
        {
            var metadata = analyzer.AnalyzeScript(script);
            allMetadata.Add(metadata);
        }

        // Объединить все метаданные
        var combined = CombineMetadata(allMetadata);

        // Применить фильтры
        var filtered = ApplyFilters(combined);

        return filtered;
    }

    private SchemaMetadata CombineMetadata(List<SchemaMetadata> metadataList)
    {
        var allTables = new List<TableDefinition>();
        var allViews = new List<ViewDefinition>();
        var allTypes = new List<TypeDefinition>();
        var allFunctions = new List<FunctionDefinition>();
        var allIndexes = new List<IndexDefinition>();
        var allTriggers = new List<TriggerDefinition>();
        var allConstraints = new List<ConstraintDefinition>();
        var allComments = new Dictionary<string, string>();
        var allIssues = new List<ValidationIssue>();

        foreach (var metadata in metadataList)
        {
            if (_extractTables) allTables.AddRange(metadata.Tables);
            if (_extractViews) allViews.AddRange(metadata.Views);
            if (_extractTypes) allTypes.AddRange(metadata.Types);
            if (_extractFunctions) allFunctions.AddRange(metadata.Functions);
            if (_extractIndexes) allIndexes.AddRange(metadata.Indexes);
            if (_extractTriggers) allTriggers.AddRange(metadata.Triggers);
            if (_extractConstraints) allConstraints.AddRange(metadata.Constraints);
            
            if (_extractComments && metadata.Comments is not null)
            {
                foreach (var (key, value) in metadata.Comments)
                {
                    allComments.TryAdd(key, value);
                }
            }
            
            // Собираем ValidationIssues из каждого metadata
            if (metadata.ValidationIssues is not null)
            {
                allIssues.AddRange(metadata.ValidationIssues);
            }
        }

        // Удалить дубликаты по ключевым полям
        if (_removeDuplicates)
        {
            allTables = allTables.DistinctBy(t => new { t.Name, t.Schema }).ToList();
            allViews = allViews.DistinctBy(v => new { v.Name, v.Schema }).ToList();
            allTypes = allTypes.DistinctBy(t => new { t.Name, t.Schema }).ToList();
            allFunctions = allFunctions.DistinctBy(f => new { f.Name, f.Schema }).ToList();
            allIndexes = allIndexes.DistinctBy(i => new { i.Name, i.Schema, i.TableName }).ToList();
            allTriggers = allTriggers.DistinctBy(t => new { t.Name, t.TableName, t.Schema }).ToList();
            allConstraints = allConstraints.DistinctBy(c => new { c.Name, c.TableName, c.Schema }).ToList();
        }

        return new SchemaMetadata
        {
            Tables = allTables,
            Views = allViews,
            Types = allTypes,
            Functions = allFunctions,
            Indexes = allIndexes,
            Triggers = allTriggers,
            Constraints = allConstraints,
            Comments = allComments,
            ValidationIssues = allIssues.Count > 0 ? allIssues : null,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private SchemaMetadata ApplyFilters(SchemaMetadata metadata)
    {
        // Применить фильтры через SchemaFilter
        var filter = SchemaFilter.From(metadata);

        // Фильтрация схем
        if (_excludeSchemas.Count > 0)
        {
            filter = filter.ExcludeSchemas(_excludeSchemas.ToArray());
        }

        if (_includeOnlySchemas.Count > 0)
        {
            filter = filter.IncludeOnlySchemas(_includeOnlySchemas.ToArray());
        }

        // Фильтрация таблиц
        if (_excludeTablePatterns.Count > 0)
        {
            foreach (var pattern in _excludeTablePatterns)
            {
                filter = filter.ExcludeTables(pattern);
            }
        }

        if (_includeOnlyTablePatterns.Count > 0)
        {
            foreach (var pattern in _includeOnlyTablePatterns)
            {
                filter = filter.IncludeOnlyTables(pattern);
            }
        }

        return filter.Build();
    }
}
