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
public sealed class SchemaGenerator(
    ITableModelGenerator tableGenerator,
    IViewModelGenerator viewGenerator,
    ICustomTypeGenerator typeGenerator,
    IFunctionMethodGenerator functionGenerator,
    ISchemaValidator validator,
    IRoslynFormatter formatter)
    : ISchemaGenerator
{
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
        var functionGenerator = new FunctionMethodGenerator(typeMapper, nameConverter);
        var validator = new SchemaValidator();

        return new SchemaGenerator(
            tableGenerator,
            viewGenerator,
            typeGenerator,
            functionGenerator,
            validator,
            formatter);
    }

    public SchemaGenerationResult Generate(
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
            tableModels = GenerateTableModels(schemaMetadata, options);
            allCode.AddRange(tableModels);
        }

        // Генерация представлений
        IReadOnlyList<GeneratedCode> viewModels = [];
        if (schemaMetadata.Views.Any())
        {
            viewModels = GenerateViewModels(schemaMetadata, options);
            allCode.AddRange(viewModels);
        }

        // Генерация пользовательских типов
        IReadOnlyList<GeneratedCode> customTypes = [];
        if (schemaMetadata.Types.Any())
        {
            customTypes = GenerateCustomTypes(schemaMetadata, options);
            allCode.AddRange(customTypes);
        }

        // Генерация функций
        IReadOnlyList<GeneratedCode> functions = [];
        if (options.GenerateFunctions && schemaMetadata.Functions.Any())
        {
            functions = GenerateFunctionMethods(schemaMetadata, options);
            allCode.AddRange(functions);
        }

        stopwatch.Stop();

        return new SchemaGenerationResult
        {
            IsSuccess = allIssues.All(i => i.Severity != ValidationSeverity.Error),
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

    public IReadOnlyList<GeneratedCode> GenerateTableModels( SchemaMetadata schemaMetadata, SchemaGenerationOptions options)
    {
        return tableGenerator.Generate(schemaMetadata.Tables, options);
    }

    public IReadOnlyList<GeneratedCode> GenerateViewModels( SchemaMetadata schemaMetadata, SchemaGenerationOptions options)
    {
        return viewGenerator.Generate(schemaMetadata.Views, options);
    }

    public IReadOnlyList<GeneratedCode> GenerateCustomTypes( SchemaMetadata schemaMetadata, SchemaGenerationOptions options)
    {
        var result = typeGenerator.Generate(schemaMetadata.Types, options);
        return result;
    }

    public IReadOnlyList<GeneratedCode> GenerateFunctionMethods( SchemaMetadata schemaMetadata, SchemaGenerationOptions options)
    {
        var result = functionGenerator.Generate(schemaMetadata.Functions, options);
        return result;
    }

    public IReadOnlyList<ValidationIssue> ValidateSchema(SchemaMetadata schemaMetadata)
    {
        return validator.Validate(schemaMetadata);
    }

    public string FormatCode(string sourceCode)
    {
        return formatter.Format(sourceCode);
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
