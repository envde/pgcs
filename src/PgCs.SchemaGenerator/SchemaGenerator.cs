using System.Diagnostics;
using PgCs.Common.Formatting;
using PgCs.Common.Generation.Models;
using PgCs.Common.SchemaAnalyzer;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaGenerator;
using PgCs.Common.SchemaGenerator.Models;
using PgCs.SchemaGenerator.Core;
using PgCs.SchemaGenerator.Generation;

namespace PgCs.SchemaGenerator;

/// <summary>
/// Генератор C# моделей на основе схемы PostgreSQL базы данных
/// </summary>
public sealed class SchemaGenerator : ISchemaGenerator
{
    private readonly ITableModelGenerator _tableGenerator;
    private readonly IViewModelGenerator _viewGenerator;
    private readonly ITypeModelGenerator _typeGenerator;
    private readonly IFunctionModelGenerator _functionGenerator;
    private readonly IFileWriter _fileWriter;

    /// <summary>
    /// Инициализирует новый экземпляр генератора схемы
    /// </summary>
    public SchemaGenerator()
        : this(
            new TableModelGenerator(),
            new ViewModelGenerator(),
            new TypeModelGenerator(),
            new FunctionModelGenerator(),
            new FileWriter())
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр генератора схемы с заданными зависимостями
    /// </summary>
    internal SchemaGenerator(
        ITableModelGenerator tableGenerator,
        IViewModelGenerator viewGenerator,
        ITypeModelGenerator typeGenerator,
        IFunctionModelGenerator functionGenerator,
        IFileWriter fileWriter)
    {
        _tableGenerator = tableGenerator ?? throw new ArgumentNullException(nameof(tableGenerator));
        _viewGenerator = viewGenerator ?? throw new ArgumentNullException(nameof(viewGenerator));
        _typeGenerator = typeGenerator ?? throw new ArgumentNullException(nameof(typeGenerator));
        _functionGenerator = functionGenerator ?? throw new ArgumentNullException(nameof(functionGenerator));
        _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
    }

    /// <inheritdoc />
    public async ValueTask<SchemaGenerationResult> GenerateAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(schemaMetadata);

        var effectiveOptions = options ?? CreateDefaultOptions();
        var stopwatch = Stopwatch.StartNew();
        var errors = new List<string>();
        var warnings = new List<string>();
        var allModels = new List<GeneratedModel>();

        try
        {
            // Генерация типов (должна быть первой, т.к. таблицы могут их использовать)
            var typeModels = await GenerateTypeModelsAsync(schemaMetadata, effectiveOptions);
            allModels.AddRange(typeModels);

            // Генерация таблиц
            var tableModels = await GenerateTableModelsAsync(schemaMetadata, effectiveOptions);
            allModels.AddRange(tableModels);

            // Генерация представлений
            var viewModels = await GenerateViewModelsAsync(schemaMetadata, effectiveOptions);
            allModels.AddRange(viewModels);

            // Генерация параметров функций
            var functionModels = await GenerateFunctionParameterModelsAsync(schemaMetadata, effectiveOptions);
            allModels.AddRange(functionModels);

            // Запись файлов
            await _fileWriter.WriteModelsAsync(allModels, effectiveOptions);

            stopwatch.Stop();

            return new SchemaGenerationResult
            {
                Models = allModels,
                OutputDirectory = effectiveOptions.OutputDirectory,
                Success = true,
                Errors = errors,
                Warnings = warnings,
                GenerationTime = stopwatch.Elapsed,
                Statistics = CreateStatistics(allModels)
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            errors.Add($"Критическая ошибка генерации: {ex.Message}");

            return new SchemaGenerationResult
            {
                Models = allModels,
                OutputDirectory = effectiveOptions.OutputDirectory,
                Success = false,
                Errors = errors,
                Warnings = warnings,
                GenerationTime = stopwatch.Elapsed
            };
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<GeneratedModel>> GenerateTableModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(schemaMetadata);

        var effectiveOptions = options ?? CreateDefaultOptions();
        var models = new List<GeneratedModel>();

        foreach (var table in schemaMetadata.Tables)
        {
            var model = _tableGenerator.Generate(table, effectiveOptions);
            models.Add(model);
        }

        return await ValueTask.FromResult<IReadOnlyList<GeneratedModel>>(models);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<GeneratedModel>> GenerateViewModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(schemaMetadata);

        var effectiveOptions = options ?? CreateDefaultOptions();
        var models = new List<GeneratedModel>();

        foreach (var view in schemaMetadata.Views)
        {
            var model = _viewGenerator.Generate(view, effectiveOptions);
            models.Add(model);
        }

        return await ValueTask.FromResult<IReadOnlyList<GeneratedModel>>(models);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<GeneratedModel>> GenerateTypeModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(schemaMetadata);

        var effectiveOptions = options ?? CreateDefaultOptions();
        var models = new List<GeneratedModel>();

        foreach (var type in schemaMetadata.Types)
        {
            var model = _typeGenerator.Generate(type, effectiveOptions);
            models.Add(model);
        }

        return await ValueTask.FromResult<IReadOnlyList<GeneratedModel>>(models);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<GeneratedModel>> GenerateFunctionParameterModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(schemaMetadata);

        var effectiveOptions = options ?? CreateDefaultOptions();
        var models = new List<GeneratedModel>();

        foreach (var function in schemaMetadata.Functions)
        {
            // Генерируем модель параметров только если у функции есть параметры
            if (function.Parameters.Count > 0)
            {
                var model = _functionGenerator.Generate(function, effectiveOptions);
                if (model != null)
                {
                    models.Add(model);
                }
            }
        }

        return await ValueTask.FromResult<IReadOnlyList<GeneratedModel>>(models);
    }

    /// <inheritdoc />
    public async ValueTask<bool> RequiresRegenerationAsync(
        SchemaMetadata schemaMetadata,
        IReadOnlyList<string> existingFiles)
    {
        ArgumentNullException.ThrowIfNull(schemaMetadata);
        ArgumentNullException.ThrowIfNull(existingFiles);

        // Если нет существующих файлов, требуется генерация
        if (existingFiles.Count == 0)
        {
            return true;
        }

        // Проверка даты анализа схемы
        if (schemaMetadata.AnalyzedAt > GetOldestFileDate(existingFiles))
        {
            return true;
        }

        // Подсчёт ожидаемого количества файлов
        var expectedFileCount = schemaMetadata.Tables.Count +
                               schemaMetadata.Views.Count +
                               schemaMetadata.Types.Count;

        // Если количество файлов не совпадает, требуется регенерация
        return await ValueTask.FromResult(existingFiles.Count != expectedFileCount);
    }

    /// <summary>
    /// Создаёт опции генерации по умолчанию
    /// </summary>
    private static SchemaGenerationOptions CreateDefaultOptions() => new()
    {
        OutputDirectory = "./Generated",
        Namespace = "Generated.Models",
        UseRecords = true,
        GenerateXmlDocumentation = true,
        NamingStrategy = NamingStrategy.PascalCase
    };

    /// <summary>
    /// Создаёт статистику генерации
    /// </summary>
    private static GenerationStatistics CreateStatistics(IReadOnlyList<GeneratedModel> models)
    {
        return new GenerationStatistics
        {
            TablesGenerated = models.Count(m => m.ModelType == ModelType.Table),
            ViewsGenerated = models.Count(m => m.ModelType == ModelType.View),
            TypesGenerated = models.Count(m => m.ModelType == ModelType.CustomType),
            EnumsGenerated = models.Count(m => m.ModelType == ModelType.Enum),
            TotalFilesGenerated = models.Count,
            TotalBytesGenerated = models.Sum(m => (long)m.SizeInBytes)
        };
    }

    /// <summary>
    /// Получает дату самого старого файла
    /// </summary>
    private static DateTime GetOldestFileDate(IReadOnlyList<string> files)
    {
        var oldestDate = DateTime.MaxValue;

        foreach (var file in files)
        {
            if (File.Exists(file))
            {
                var fileDate = File.GetLastWriteTimeUtc(file);
                if (fileDate < oldestDate)
                {
                    oldestDate = fileDate;
                }
            }
        }

        return oldestDate == DateTime.MaxValue ? DateTime.MinValue : oldestDate;
    }
}
