using System.Diagnostics;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator;
using PgCs.Common.QueryGenerator.Models;
using PgCs.QueryGenerator.Core;
using PgCs.QueryGenerator.Generation;

namespace PgCs.QueryGenerator;

/// <summary>
/// Генератор C# методов для выполнения SQL запросов PostgreSQL
/// </summary>
public sealed class QueryGenerator : IQueryGenerator
{
    private readonly IMethodGenerator _methodGenerator;
    private readonly IResultModelGenerator _resultModelGenerator;
    private readonly IParameterModelGenerator _parameterModelGenerator;
    private readonly IClassGenerator _classGenerator;
    private readonly IFileWriter _fileWriter;
    private readonly ICodeValidator _codeValidator;

    /// <summary>
    /// Инициализирует новый экземпляр генератора запросов
    /// </summary>
    public QueryGenerator()
    {
        _methodGenerator = new MethodGenerator();
        _resultModelGenerator = new ResultModelGenerator();
        _parameterModelGenerator = new ParameterModelGenerator();
        _classGenerator = new ClassGenerator(_methodGenerator, _resultModelGenerator);
        _fileWriter = new FileWriter();
        _codeValidator = new CodeValidator();
    }

    /// <summary>
    /// Инициализирует новый экземпляр генератора запросов с заданными зависимостями
    /// </summary>
    internal QueryGenerator(
        IMethodGenerator methodGenerator,
        IResultModelGenerator resultModelGenerator,
        IParameterModelGenerator parameterModelGenerator,
        IClassGenerator classGenerator,
        IFileWriter fileWriter,
        ICodeValidator codeValidator)
    {
        _methodGenerator = methodGenerator ?? throw new ArgumentNullException(nameof(methodGenerator));
        _resultModelGenerator = resultModelGenerator ?? throw new ArgumentNullException(nameof(resultModelGenerator));
        _parameterModelGenerator = parameterModelGenerator ?? throw new ArgumentNullException(nameof(parameterModelGenerator));
        _classGenerator = classGenerator ?? throw new ArgumentNullException(nameof(classGenerator));
        _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        _codeValidator = codeValidator ?? throw new ArgumentNullException(nameof(codeValidator));
    }

    /// <inheritdoc />
    public async ValueTask<QueryGenerationResult> GenerateAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(queries);

        var effectiveOptions = options ?? CreateDefaultOptions();
        var stopwatch = Stopwatch.StartNew();
        var errors = new List<string>();
        var warnings = new List<string>();

        try
        {
            // Генерация моделей результатов
            var resultModels = await GenerateResultModelsAsync(queries, effectiveOptions);

            // Генерация моделей параметров (если требуется)
            var parameterModels = effectiveOptions.GenerateParameterModels
                ? await GenerateParameterModelsAsync(queries, effectiveOptions)
                : Array.Empty<GeneratedModel>();

            // Генерация класса с методами
            var generatedClass = await GenerateQueryClassAsync(
                queries,
                effectiveOptions.ClassName,
                effectiveOptions);

            // Запись файлов
            var allModels = resultModels.Concat(parameterModels).ToList();
            await _fileWriter.WriteClassAsync(generatedClass, effectiveOptions);
            await _fileWriter.WriteModelsAsync(allModels, effectiveOptions);

            // Валидация (опционально)
            if (effectiveOptions.GenerateErrorHandling)
            {
                var validationResult = await ValidateGeneratedCodeAsync(generatedClass.SourceCode);
                if (!validationResult.IsValid)
                {
                    warnings.AddRange(validationResult.Warnings.Select(w => w.Message));
                }
            }

            stopwatch.Stop();

            return new QueryGenerationResult
            {
                Classes = [generatedClass],
                ResultModels = resultModels,
                ParameterModels = parameterModels,
                OutputDirectory = effectiveOptions.OutputDirectory,
                Success = true,
                Errors = errors,
                Warnings = warnings,
                GenerationTime = stopwatch.Elapsed,
                Statistics = CreateStatistics(queries, resultModels, parameterModels)
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            errors.Add($"Критическая ошибка генерации: {ex.Message}");

            return new QueryGenerationResult
            {
                Classes = [],
                ResultModels = [],
                ParameterModels = [],
                OutputDirectory = effectiveOptions.OutputDirectory,
                Success = false,
                Errors = errors,
                Warnings = warnings,
                GenerationTime = stopwatch.Elapsed
            };
        }
    }

    /// <inheritdoc />
    public async ValueTask<GeneratedMethod> GenerateMethodAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(queryMetadata);

        var effectiveOptions = options ?? CreateDefaultOptions();
        return await ValueTask.FromResult(_methodGenerator.Generate(queryMetadata, effectiveOptions));
    }

    /// <inheritdoc />
    public async ValueTask<GeneratedClass> GenerateQueryClassAsync(
        IReadOnlyList<QueryMetadata> queries,
        string className,
        QueryGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(queries);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);

        var effectiveOptions = options ?? CreateDefaultOptions();
        return await ValueTask.FromResult(_classGenerator.Generate(queries, className, effectiveOptions));
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<GeneratedModel>> GenerateResultModelsAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(queries);

        var effectiveOptions = options ?? CreateDefaultOptions();

        if (!effectiveOptions.GenerateResultModels)
        {
            return Array.Empty<GeneratedModel>();
        }

        var models = new List<GeneratedModel>();

        foreach (var query in queries)
        {
            // Генерируем модель результата только для SELECT запросов с возвратом данных
            if (query.ReturnType != null && query.QueryType == QueryType.Select)
            {
                var model = _resultModelGenerator.Generate(query, effectiveOptions);
                if (model != null)
                {
                    models.Add(model);
                }
            }
        }

        return await ValueTask.FromResult<IReadOnlyList<GeneratedModel>>(models);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<GeneratedModel>> GenerateParameterModelsAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(queries);

        var effectiveOptions = options ?? CreateDefaultOptions();

        if (!effectiveOptions.GenerateParameterModels)
        {
            return Array.Empty<GeneratedModel>();
        }

        var models = new List<GeneratedModel>();

        foreach (var query in queries)
        {
            // Генерируем модель параметров только если параметров больше определённого порога
            if (query.Parameters.Count >= 3) // Порог можно сделать настраиваемым
            {
                var model = _parameterModelGenerator.Generate(query, effectiveOptions);
                if (model != null)
                {
                    models.Add(model);
                }
            }
        }

        return await ValueTask.FromResult<IReadOnlyList<GeneratedModel>>(models);
    }

    /// <inheritdoc />
    public async ValueTask<ValidationResult> ValidateGeneratedCodeAsync(string generatedCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedCode);

        return await ValueTask.FromResult(_codeValidator.Validate(generatedCode));
    }

    /// <summary>
    /// Создаёт опции генерации по умолчанию
    /// </summary>
    private static QueryGenerationOptions CreateDefaultOptions() => new()
    {
        OutputDirectory = "./Generated",
        Namespace = "Generated.Queries",
        ClassName = "Queries",
        GenerateAsyncMethods = true,
        UseValueTask = true,
        GenerateXmlDocumentation = true,
        GenerateResultModels = true,
        DatabaseProvider = DatabaseProvider.Npgsql,
        SupportCancellation = true
    };

    /// <summary>
    /// Создаёт статистику генерации
    /// </summary>
    private static QueryGenerationStatistics CreateStatistics(
        IReadOnlyList<QueryMetadata> queries,
        IReadOnlyList<GeneratedModel> resultModels,
        IReadOnlyList<GeneratedModel> parameterModels)
    {
        return new QueryGenerationStatistics
        {
            MethodsGenerated = queries.Count,
            SelectMethodsGenerated = queries.Count(q => q.QueryType == QueryType.Select),
            MutationMethodsGenerated = queries.Count(q => q.QueryType != QueryType.Select),
            ResultModelsGenerated = resultModels.Count,
            ParameterModelsGenerated = parameterModels.Count,
            TotalFilesGenerated = 1 + resultModels.Count + parameterModels.Count,
            TotalBytesGenerated = resultModels.Sum(m => (long)m.SizeInBytes) +
                                 parameterModels.Sum(m => (long)m.SizeInBytes)
        };
    }
}
