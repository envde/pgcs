using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.Common.SchemaGenerator.Models.Results;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;
using PgCs.Common.Writer;
using PgCs.Common.Writer.Models;
using PgCs.SchemaAnalyzer;
using PgCs.QueryAnalyzer;
using PgCs.FileWriter;

namespace PgCs.Cli;

/// <summary>
/// Fluent API для полного pipeline генерации кода: анализ → фильтрация → генерация → запись
/// </summary>
public class CodeGenerationPipeline
{
    private readonly List<string> _schemaFiles = new();
    private readonly List<string> _schemaDirectories = new();
    private readonly List<string> _queryFiles = new();
    private SchemaGenerationOptions? _schemaOptions;
    private QueryGenerationOptions? _queryOptions;
    private WriteOptions? _writeOptions;
    private Func<SchemaMetadata, SchemaMetadata>? _schemaFilter;
    private bool _generateSchema = true;
    private bool _generateQueries = true;
    private bool _writeToFiles = true;
    private Action<string>? _onProgress;
    private Action<string>? _onError;
    private Action<PipelineStatistics>? _onComplete;

    private CodeGenerationPipeline() { }

    /// <summary>
    /// Создать новый pipeline генерации кода
    /// </summary>
    public static CodeGenerationPipeline Create() => new();

    #region Schema Analysis Configuration

    /// <summary>
    /// Анализировать схему из файла
    /// </summary>
    public CodeGenerationPipeline FromSchemaFile(string filePath)
    {
        _schemaFiles.Add(filePath);
        return this;
    }

    /// <summary>
    /// Анализировать схему из нескольких файлов
    /// </summary>
    public CodeGenerationPipeline FromSchemaFiles(params string[] filePaths)
    {
        _schemaFiles.AddRange(filePaths);
        return this;
    }

    /// <summary>
    /// Анализировать схему из директории
    /// </summary>
    public CodeGenerationPipeline FromSchemaDirectory(string directoryPath)
    {
        _schemaDirectories.Add(directoryPath);
        return this;
    }

    #endregion

    #region Query Analysis Configuration

    /// <summary>
    /// Анализировать запросы из файла
    /// </summary>
    public CodeGenerationPipeline FromQueryFile(string filePath)
    {
        _queryFiles.Add(filePath);
        return this;
    }

    /// <summary>
    /// Анализировать запросы из нескольких файлов
    /// </summary>
    public CodeGenerationPipeline FromQueryFiles(params string[] filePaths)
    {
        _queryFiles.AddRange(filePaths);
        return this;
    }

    #endregion

    #region Schema Filtering

    /// <summary>
    /// Применить пользовательский фильтр к схеме
    /// </summary>
    public CodeGenerationPipeline FilterSchema(Func<SchemaMetadata, SchemaMetadata> filter)
    {
        _schemaFilter = filter;
        return this;
    }

    /// <summary>
    /// Исключить системные объекты из схемы
    /// </summary>
    public CodeGenerationPipeline ExcludeSystemObjects()
    {
        return FilterSchema(metadata => 
            SchemaFilter.From(metadata)
                .RemoveSystemObjects()
                .Build());
    }

    /// <summary>
    /// Оставить только таблицы
    /// </summary>
    public CodeGenerationPipeline OnlyTables()
    {
        return FilterSchema(metadata => 
            SchemaFilter.From(metadata)
                .OnlyTables()
                .Build());
    }

    /// <summary>
    /// Исключить таблицы по паттернам
    /// </summary>
    public CodeGenerationPipeline ExcludeTables(params string[] patterns)
    {
        return FilterSchema(metadata =>
        {
            var filter = SchemaFilter.From(metadata);
            foreach (var pattern in patterns)
            {
                filter = filter.ExcludeTables(pattern);
            }
            return filter.Build();
        });
    }

    #endregion

    #region Generation Options

    /// <summary>
    /// Настроить генерацию моделей схемы
    /// </summary>
    public CodeGenerationPipeline WithSchemaGeneration(Action<SchemaGenerationOptionsBuilder> configure)
    {
        var builder = SchemaGenerationOptions.CreateBuilder();
        configure(builder);
        _schemaOptions = builder.Build();
        return this;
    }

    /// <summary>
    /// Настроить генерацию моделей схемы (готовый builder)
    /// </summary>
    public CodeGenerationPipeline WithSchemaGeneration(SchemaGenerationOptions options)
    {
        _schemaOptions = options;
        return this;
    }

    /// <summary>
    /// НЕ генерировать модели схемы
    /// </summary>
    public CodeGenerationPipeline WithoutSchemaGeneration()
    {
        _generateSchema = false;
        return this;
    }

    /// <summary>
    /// Настроить генерацию репозиториев для запросов
    /// </summary>
    public CodeGenerationPipeline WithQueryGeneration(Action<QueryGenerationOptionsBuilder> configure)
    {
        var builder = QueryGenerationOptions.CreateBuilder();
        configure(builder);
        _queryOptions = builder.Build();
        return this;
    }

    /// <summary>
    /// Настроить генерацию репозиториев для запросов (готовый builder)
    /// </summary>
    public CodeGenerationPipeline WithQueryGeneration(QueryGenerationOptions options)
    {
        _queryOptions = options;
        return this;
    }

    /// <summary>
    /// НЕ генерировать репозитории
    /// </summary>
    public CodeGenerationPipeline WithoutQueryGeneration()
    {
        _generateQueries = false;
        return this;
    }

    /// <summary>
    /// Настроить запись файлов
    /// </summary>
    public CodeGenerationPipeline WithFileWriting(Action<WriteOptionsBuilder> configure)
    {
        var builder = WriteOptions.CreateBuilder();
        configure(builder);
        _writeOptions = builder.Build();
        return this;
    }

    /// <summary>
    /// Настроить запись файлов (готовые опции)
    /// </summary>
    public CodeGenerationPipeline WithFileWriting(WriteOptions options)
    {
        _writeOptions = options;
        return this;
    }

    /// <summary>
    /// НЕ записывать файлы (только генерация в память)
    /// </summary>
    public CodeGenerationPipeline WithoutFileWriting()
    {
        _writeToFiles = false;
        return this;
    }

    #endregion

    #region Events & Callbacks

    /// <summary>
    /// Обработчик прогресса выполнения
    /// </summary>
    public CodeGenerationPipeline OnProgress(Action<string> handler)
    {
        _onProgress = handler;
        return this;
    }

    /// <summary>
    /// Обработчик ошибок
    /// </summary>
    public CodeGenerationPipeline OnError(Action<string> handler)
    {
        _onError = handler;
        return this;
    }

    /// <summary>
    /// Обработчик завершения с финальной статистикой
    /// </summary>
    public CodeGenerationPipeline OnComplete(Action<PipelineStatistics> handler)
    {
        _onComplete = handler;
        return this;
    }

    #endregion

    #region Execution

    /// <summary>
    /// Выполнить весь pipeline
    /// </summary>
    public async ValueTask<PipelineResult> ExecuteAsync()
    {
        var result = new PipelineResult { IsSuccess = false };
        var startTime = DateTime.UtcNow;

        try
        {
            // ШАГ 1: Анализ схемы
            SchemaMetadata? schemaMetadata = null;
            if (_generateSchema && (_schemaFiles.Any() || _schemaDirectories.Any()))
            {
                ReportProgress("Анализ схемы базы данных...");
                schemaMetadata = await AnalyzeSchemaAsync();
                result.AnalyzedSchemaMetadata = schemaMetadata;
                ReportProgress($"Проанализировано: {schemaMetadata.Tables.Count} таблиц, {schemaMetadata.Types.Count} типов");
            }

            // ШАГ 2: Фильтрация схемы
            if (schemaMetadata != null && _schemaFilter != null)
            {
                ReportProgress("Применение фильтров к схеме...");
                schemaMetadata = _schemaFilter(schemaMetadata);
                result.FilteredSchemaMetadata = schemaMetadata;
            }

            // ШАГ 3: Генерация моделей схемы
            SchemaGenerationResult? schemaGenResult = null;
            if (_generateSchema && schemaMetadata != null)
            {
                ReportProgress("Генерация C# моделей для схемы...");
                var schemaGen = PgCs.SchemaGenerator.SchemaGenerator.Create();
                schemaGenResult = await schemaGen.GenerateAsync(
                    schemaMetadata,
                    _schemaOptions ?? CreateDefaultSchemaOptions());
                result.SchemaGenerationResult = schemaGenResult;
                ReportProgress($"Сгенерировано {schemaGenResult.GeneratedCode.Count} файлов моделей");
            }

            // ШАГ 4: Анализ запросов
            IReadOnlyList<QueryMetadata>? queries = null;
            if (_generateQueries && _queryFiles.Any())
            {
                ReportProgress("Анализ SQL запросов...");
                queries = await AnalyzeQueriesAsync();
                result.AnalyzedQueries = queries;
                ReportProgress($"Проанализировано {queries.Count} SQL запросов");
            }

            // ШАГ 5: Генерация репозиториев
            QueryGenerationResult? queryGenResult = null;
            if (_generateQueries && queries != null && queries.Any())
            {
                ReportProgress("Генерация C# репозиториев для запросов...");
                var queryGen = PgCs.QueryGenerator.QueryGenerator.Create();
                queryGenResult = await queryGen.GenerateAsync(
                    queries,
                    _queryOptions ?? CreateDefaultQueryOptions());
                result.QueryGenerationResult = queryGenResult;
                ReportProgress($"Сгенерировано {queryGenResult.GeneratedCode.Count} файлов репозиториев");
            }

            // ШАГ 6: Запись файлов
            if (_writeToFiles)
            {
                ReportProgress("Запись сгенерированных файлов...");
                var writer = PgCs.FileWriter.FileWriter.Create();
                var writeOptions = _writeOptions ?? CreateDefaultWriteOptions();

                // Записываем модели схемы
                if (schemaGenResult != null)
                {
                    var schemaWriteResult = await writer.WriteManyAsync(
                        schemaGenResult.GeneratedCode, writeOptions);
                    result.SchemaWriteResult = schemaWriteResult;
                    ReportProgress($"Записано {schemaWriteResult.WrittenFiles.Count} файлов моделей");
                }

                // Записываем репозитории
                if (queryGenResult != null)
                {
                    var queryWriteResult = await writer.WriteManyAsync(
                        queryGenResult.GeneratedCode, writeOptions);
                    result.QueryWriteResult = queryWriteResult;
                    ReportProgress($"Записано {queryWriteResult.WrittenFiles.Count} файлов репозиториев");
                }
            }

            // Успех
            result.IsSuccess = true;
            result.Duration = DateTime.UtcNow - startTime;

            // Формируем статистику
            var statistics = BuildStatistics(result);
            result.Statistics = statistics;

            // Вызываем callback завершения
            _onComplete?.Invoke(statistics);
            ReportProgress("Генерация кода завершена успешно!");

            return result;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Error = ex;
            result.Duration = DateTime.UtcNow - startTime;
            ReportError($"Ошибка выполнения pipeline: {ex.Message}");
            return result;
        }
    }

    #endregion

    #region Private Methods

    private async ValueTask<SchemaMetadata> AnalyzeSchemaAsync()
    {
        var builder = SchemaAnalyzerBuilder.Create();

        foreach (var file in _schemaFiles)
            builder.FromFile(file);

        foreach (var dir in _schemaDirectories)
            builder.FromDirectory(dir);

        return await builder.AnalyzeAsync();
    }

    private async ValueTask<IReadOnlyList<QueryMetadata>> AnalyzeQueriesAsync()
    {
        var builder = QueryAnalyzerBuilder.Create();

        foreach (var file in _queryFiles)
            builder.FromFile(file);

        return await builder.AnalyzeAsync();
    }

    private static SchemaGenerationOptions CreateDefaultSchemaOptions()
    {
        return SchemaGenerationOptions.CreateBuilder()
            .WithNamespace("Generated.Models")
            .OutputTo("./Generated")
            .UseRecords()
            .Build();
    }

    private static QueryGenerationOptions CreateDefaultQueryOptions()
    {
        return QueryGenerationOptions.CreateBuilder()
            .WithNamespace("Generated.Repositories")
            .OutputTo("./Generated")
            .UseAsync()
            .Build();
    }

    private static WriteOptions CreateDefaultWriteOptions()
    {
        return WriteOptions.CreateBuilder()
            .OutputTo("./Generated")
            .OverwriteExisting()
            .CreateDirectories()
            .Build();
    }

    private void ReportProgress(string message)
    {
        _onProgress?.Invoke(message);
    }

    private void ReportError(string message)
    {
        _onError?.Invoke(message);
    }

    private static PipelineStatistics BuildStatistics(PipelineResult result)
    {
        return new PipelineStatistics
        {
            TablesAnalyzed = result.AnalyzedSchemaMetadata?.Tables.Count ?? 0,
            TypesAnalyzed = result.AnalyzedSchemaMetadata?.Types.Count ?? 0,
            ViewsAnalyzed = result.AnalyzedSchemaMetadata?.Views.Count ?? 0,
            QueriesAnalyzed = result.AnalyzedQueries?.Count ?? 0,
            SchemaFilesGenerated = result.SchemaGenerationResult?.GeneratedCode.Count ?? 0,
            QueryFilesGenerated = result.QueryGenerationResult?.GeneratedCode.Count ?? 0,
            TotalFilesGenerated = 
                (result.SchemaGenerationResult?.GeneratedCode.Count ?? 0) +
                (result.QueryGenerationResult?.GeneratedCode.Count ?? 0),
            TotalFilesWritten = 
                (result.SchemaWriteResult?.WrittenFiles.Count ?? 0) +
                (result.QueryWriteResult?.WrittenFiles.Count ?? 0),
            TotalBytesWritten = 
                (result.SchemaWriteResult?.TotalBytesWritten ?? 0) +
                (result.QueryWriteResult?.TotalBytesWritten ?? 0),
            Duration = result.Duration,
            HasErrors = !result.IsSuccess
        };
    }

    #endregion
}

/// <summary>
/// Результат выполнения pipeline
/// </summary>
public record PipelineResult
{
    public required bool IsSuccess { get; set; }
    public TimeSpan Duration { get; set; }
    public Exception? Error { get; set; }

    // Промежуточные результаты
    public SchemaMetadata? AnalyzedSchemaMetadata { get; set; }
    public SchemaMetadata? FilteredSchemaMetadata { get; set; }
    public IReadOnlyList<QueryMetadata>? AnalyzedQueries { get; set; }

    // Результаты генерации
    public SchemaGenerationResult? SchemaGenerationResult { get; set; }
    public QueryGenerationResult? QueryGenerationResult { get; set; }

    // Результаты записи
    public WriteResult? SchemaWriteResult { get; set; }
    public WriteResult? QueryWriteResult { get; set; }

    // Статистика
    public PipelineStatistics? Statistics { get; set; }
}

/// <summary>
/// Статистика выполнения pipeline
/// </summary>
public record PipelineStatistics
{
    public int TablesAnalyzed { get; init; }
    public int TypesAnalyzed { get; init; }
    public int ViewsAnalyzed { get; init; }
    public int QueriesAnalyzed { get; init; }
    public int SchemaFilesGenerated { get; init; }
    public int QueryFilesGenerated { get; init; }
    public int TotalFilesGenerated { get; init; }
    public int TotalFilesWritten { get; init; }
    public long TotalBytesWritten { get; init; }
    public TimeSpan Duration { get; init; }
    public bool HasErrors { get; init; }
}
