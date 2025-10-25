using System.Collections.Frozen;
using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.SchemaAnalyzer.Extractors;
using PgCs.SchemaAnalyzer.Utils;

namespace PgCs.SchemaAnalyzer;

/// <summary>
/// Анализатор схемы PostgreSQL базы данных
/// </summary>
public sealed class SchemaAnalyzer : ISchemaAnalyzer
{
    private readonly TableExtractor _tableExtractor = new();
    private readonly ViewExtractor _viewExtractor = new();
    private readonly TypeExtractor _typeExtractor = new();
    private readonly FunctionExtractor _functionExtractor = new();
    private readonly IndexExtractor _indexExtractor = new();
    private readonly TriggerExtractor _triggerExtractor = new();
    private readonly ConstraintExtractor _constraintExtractor = new();
    private readonly CommentExtractor _commentExtractor = new();

    /// <summary>
    /// Warnings и errors собранные во время parsing schema
    /// </summary>
    public List<ValidationIssue> Issues { get; } = new();

    public async ValueTask<SchemaMetadata> AnalyzeFileAsync(string schemaFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaFilePath);

        if (!File.Exists(schemaFilePath))
            throw new FileNotFoundException($"Schema file not found: {schemaFilePath}");

        Issues.Clear(); // Очищаем перед новым анализом
        var sqlScript = await File.ReadAllTextAsync(schemaFilePath);
        var metadata = AnalyzeScript(sqlScript);

        return metadata with { SourceFile = schemaFilePath };
    }

    public async ValueTask<SchemaMetadata> AnalyzeDirectoryAsync(string schemaDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaDirectoryPath);

        if (!Directory.Exists(schemaDirectoryPath))
            throw new DirectoryNotFoundException($"Schema directory not found: {schemaDirectoryPath}");

        Issues.Clear(); // Очищаем перед новым анализом

        var sqlFiles = Directory.GetFiles(schemaDirectoryPath, "*.sql", SearchOption.AllDirectories)
            .OrderBy(f => f)
            .ToArray();

        if (sqlFiles.Length == 0)
            throw new InvalidOperationException($"No SQL files found in directory: {schemaDirectoryPath}");

        var allTables = new List<TableDefinition>();
        var allViews = new List<ViewDefinition>();
        var allTypes = new List<TypeDefinition>();
        var allFunctions = new List<FunctionDefinition>();
        var allIndexes = new List<IndexDefinition>();
        var allTriggers = new List<TriggerDefinition>();
        var allConstraints = new List<ConstraintDefinition>();
        var allComments = new Dictionary<string, string>();
        var allIssues = new List<ValidationIssue>();

        foreach (var file in sqlFiles)
        {
            var sqlScript = await File.ReadAllTextAsync(file);
            var metadata = AnalyzeScript(sqlScript);

            allTables.AddRange(metadata.Tables);
            allViews.AddRange(metadata.Views);
            allTypes.AddRange(metadata.Types);
            allFunctions.AddRange(metadata.Functions);
            allIndexes.AddRange(metadata.Indexes);
            allTriggers.AddRange(metadata.Triggers);
            allConstraints.AddRange(metadata.Constraints);

            if (metadata.Comments is not null)
            {
                foreach (var (key, value) in metadata.Comments)
                    allComments.TryAdd(key, value);
            }

            // Собираем ValidationIssues из каждого файла
            if (metadata.ValidationIssues is not null)
            {
                allIssues.AddRange(metadata.ValidationIssues);
            }
        }

        // Добавляем в public Issues property
        Issues.AddRange(allIssues);

        return new SchemaMetadata
        {
            Tables = allTables.ToFrozenSet().ToArray(),
            Views = allViews.ToFrozenSet().ToArray(),
            Types = allTypes.ToFrozenSet().ToArray(),
            Functions = allFunctions.ToFrozenSet().ToArray(),
            Indexes = allIndexes.ToFrozenSet().ToArray(),
            Triggers = allTriggers.ToFrozenSet().ToArray(),
            Constraints = allConstraints.ToFrozenSet().ToArray(),
            Comments = allComments.ToFrozenDictionary(),
            ValidationIssues = allIssues.Count > 0 ? allIssues : null,
            SourceFile = schemaDirectoryPath,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    public IReadOnlyList<TableDefinition> ExtractTables(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        return _tableExtractor.Extract(sqlScript);
    }

    public IReadOnlyList<ViewDefinition> ExtractViews(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        return _viewExtractor.Extract(sqlScript);
    }

    public IReadOnlyList<TypeDefinition> ExtractTypes(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        return _typeExtractor.Extract(sqlScript);
    }

    public IReadOnlyList<FunctionDefinition> ExtractFunctions(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        return _functionExtractor.Extract(sqlScript);
    }

    public IReadOnlyList<IndexDefinition> ExtractIndexes(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        return _indexExtractor.Extract(sqlScript);
    }

    public IReadOnlyList<TriggerDefinition> ExtractTriggers(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        return _triggerExtractor.Extract(sqlScript);
    }

    public IReadOnlyList<ConstraintDefinition> ExtractConstraints(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        return _constraintExtractor.Extract(sqlScript);
    }
    
    public SchemaMetadata AnalyzeScript(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);

        Issues.Clear(); // Очищаем перед новым анализом

        // Сначала разделяем на statements (ДО нормализации, чтобы сохранить переводы строк)
        var statements = SqlStatementSplitter.Split(sqlScript);
        
        // Нормализуем каждый statement отдельно
        var normalizedStatements = statements.Select(SqlNormalizer.Normalize).ToArray();
        
        // Объединяем обратно с точкой с запятой для extractors
        var normalizedScript = string.Join("; ", normalizedStatements);

        var types = ExtractTypes(normalizedScript);
        var tables = ExtractTables(normalizedScript);
        var views = ExtractViews(normalizedScript);
        var functions = ExtractFunctions(normalizedScript);
        var indexes = ExtractIndexes(normalizedScript);
        var triggers = ExtractTriggers(normalizedScript);
        var constraints = ExtractConstraints(normalizedScript);
        var comments = _commentExtractor.ExtractComments(normalizedScript);

        // Собрать constraints из таблиц
        var allConstraints = new List<ConstraintDefinition>(constraints);
        foreach (var table in tables)
        {
            allConstraints.AddRange(table.Constraints);
        }

        // Собрать validation issues из всех extractors
        var allIssues = new List<ValidationIssue>();
        allIssues.AddRange(_tableExtractor.Issues);
        allIssues.AddRange(_viewExtractor.Issues);
        allIssues.AddRange(_typeExtractor.Issues);
        allIssues.AddRange(_functionExtractor.Issues);
        allIssues.AddRange(_indexExtractor.Issues);
        allIssues.AddRange(_triggerExtractor.Issues);
        allIssues.AddRange(_constraintExtractor.Issues);

        // Добавляем в public Issues property для единообразия с QueryAnalyzer
        Issues.AddRange(allIssues);

        return new SchemaMetadata
        {
            Tables = tables,
            Views = views,
            Types = types,
            Functions = functions,
            Indexes = indexes,
            Triggers = triggers,
            Constraints = allConstraints,
            Comments = comments,
            ValidationIssues = allIssues.Count > 0 ? allIssues : null,
            AnalyzedAt = DateTime.UtcNow
        };
    }
}