using System.Diagnostics;
using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaGenerator;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.Common.SchemaGenerator.Models.Results;
using PgCs.Common.Services;
using PgCs.SchemaGenerator.Generators;
using PgCs.SchemaGenerator.Services;

namespace PgCs.SchemaGenerator;

/// <summary>
/// Реализация генератора C# кода на основе схемы PostgreSQL базы данных
/// </summary>
public sealed class SchemaGenerator : ISchemaGenerator
{
    private readonly ITableModelGenerator _tableGenerator;
    private readonly IViewModelGenerator _viewGenerator;
    private readonly ICustomTypeGenerator _typeGenerator;
    private readonly IFunctionMethodGenerator _functionGenerator;
    private readonly ISchemaValidator _validator;
    private readonly IRoslynFormatter _formatter;

    public SchemaGenerator(
        ITableModelGenerator tableGenerator,
        IViewModelGenerator viewGenerator,
        ICustomTypeGenerator typeGenerator,
        IFunctionMethodGenerator functionGenerator,
        ISchemaValidator validator,
        IRoslynFormatter formatter)
    {
        _tableGenerator = tableGenerator;
        _viewGenerator = viewGenerator;
        _typeGenerator = typeGenerator;
        _functionGenerator = functionGenerator;
        _validator = validator;
        _formatter = formatter;
    }

    /// <summary>
    /// Создает экземпляр SchemaGenerator с зависимостями по умолчанию
    /// </summary>
    public static SchemaGenerator Create()
    {
        var typeMapper = new PostgreSqlTypeMapper();
        var nameConverter = new NameConverter();
        var syntaxBuilder = new SyntaxBuilder(typeMapper, nameConverter);
        var formatter = new RoslynFormatter();

        var tableGenerator = new TableModelGenerator(syntaxBuilder);
        var viewGenerator = new ViewModelGenerator(syntaxBuilder);
        var typeGenerator = new CustomTypeGenerator(syntaxBuilder);
        var functionGenerator = new FunctionMethodGenerator(syntaxBuilder);
        var validator = new SchemaValidator();

        return new SchemaGenerator(
            tableGenerator,
            viewGenerator,
            typeGenerator,
            functionGenerator,
            validator,
            formatter);
    }

    public async ValueTask<SchemaGenerationResult> GenerateAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        var allCode = new List<GeneratedCode>();
        var allIssues = new List<ValidationIssue>();

        // Валидация схемы
        var validationIssues = ValidateSchema(schemaMetadata);
        allIssues.AddRange(validationIssues);

        // Если есть критические ошибки, останавливаемся
        if (validationIssues.Any(i => i.Severity == ValidationSeverity.Error))
        {
            return new SchemaGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = allCode,
                ValidationIssues = allIssues,
                Duration = stopwatch.Elapsed
            };
        }

        // Генерация таблиц
        IReadOnlyList<GeneratedCode> tableModels = [];
        if (schemaMetadata.Tables.Any())
        {
            tableModels = await GenerateTableModelsAsync(schemaMetadata, options);
            allCode.AddRange(tableModels);
        }

        // Генерация представлений
        IReadOnlyList<GeneratedCode> viewModels = [];
        if (schemaMetadata.Views.Any())
        {
            viewModels = await GenerateViewModelsAsync(schemaMetadata, options);
            allCode.AddRange(viewModels);
        }

        // Генерация пользовательских типов
        IReadOnlyList<GeneratedCode> customTypes = [];
        if (schemaMetadata.Types.Any())
        {
            customTypes = await GenerateCustomTypesAsync(schemaMetadata, options);
            allCode.AddRange(customTypes);
        }

        // Генерация функций
        IReadOnlyList<GeneratedCode> functions = [];
        if (options.GenerateFunctions && schemaMetadata.Functions.Any())
        {
            functions = await GenerateFunctionMethodsAsync(schemaMetadata, options);
            allCode.AddRange(functions);
        }

        stopwatch.Stop();

        return new SchemaGenerationResult
        {
            IsSuccess = !allIssues.Any(i => i.Severity == ValidationSeverity.Error),
            GeneratedCode = allCode,
            ValidationIssues = allIssues,
            Duration = stopwatch.Elapsed,
            TableModels = tableModels,
            ViewModels = viewModels,
            CustomTypes = customTypes,
            Functions = functions,
            Statistics = CalculateStatistics(allCode, schemaMetadata, allIssues)
        };
    }

    public async ValueTask<IReadOnlyList<GeneratedCode>> GenerateTableModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options)
    {
        return await _tableGenerator.GenerateAsync(schemaMetadata.Tables, options);
    }

    public async ValueTask<IReadOnlyList<GeneratedCode>> GenerateViewModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options)
    {
        return await _viewGenerator.GenerateAsync(schemaMetadata.Views, options);
    }

    public async ValueTask<IReadOnlyList<GeneratedCode>> GenerateCustomTypesAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options)
    {
        var result = await _typeGenerator.GenerateAsync(schemaMetadata.Types, options);

        return result;
    }

    public async ValueTask<IReadOnlyList<GeneratedCode>> GenerateFunctionMethodsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options)
    {
        return await _functionGenerator.GenerateAsync(schemaMetadata.Functions, options);
    }

    public IReadOnlyList<ValidationIssue> ValidateSchema(SchemaMetadata schemaMetadata)
    {
        return _validator.Validate(schemaMetadata);
    }

    public async ValueTask<string> FormatCodeAsync(string sourceCode)
    {
        return await _formatter.FormatAsync(sourceCode);
    }

    private static GenerationStatistics CalculateStatistics(
        IReadOnlyList<GeneratedCode> code,
        SchemaMetadata schemaMetadata,
        IReadOnlyList<ValidationIssue> issues)
    {
        return new GenerationStatistics
        {
            TotalFilesGenerated = code.Count,
            TotalSizeInBytes = code.Sum(c => c.SizeInBytes),
            TotalLinesOfCode = code.Sum(c => c.SourceCode.Split('\n').Length),
            TablesProcessed = schemaMetadata.Tables.Count,
            ViewsProcessed = schemaMetadata.Views.Count,
            TypesProcessed = schemaMetadata.Types.Count,
            FunctionsProcessed = schemaMetadata.Functions.Count,
            ErrorCount = issues.Count(i => i.Severity == ValidationSeverity.Error),
            WarningCount = issues.Count(i => i.Severity == ValidationSeverity.Warning)
        };
    }
}
