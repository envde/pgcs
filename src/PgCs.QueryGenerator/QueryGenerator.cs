using System.Diagnostics;
using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;
using PgCs.Common.Services;
using PgCs.QueryGenerator.Generators;
using PgCs.QueryGenerator.Services;

namespace PgCs.QueryGenerator;

/// <summary>
/// Реализация генератора C# методов для SQL запросов
/// </summary>
public sealed class QueryGenerator : IQueryGenerator
{
    private readonly IQueryMethodGenerator _methodGenerator;
    private readonly IQueryModelGenerator _modelGenerator;
    private readonly IRepositoryGenerator _repositoryGenerator;
    private readonly IQueryValidator _validator;
    private readonly IRoslynFormatter _formatter;

    public QueryGenerator(
        IQueryMethodGenerator methodGenerator,
        IQueryModelGenerator modelGenerator,
        IRepositoryGenerator repositoryGenerator,
        IQueryValidator validator,
        IRoslynFormatter formatter)
    {
        _methodGenerator = methodGenerator;
        _modelGenerator = modelGenerator;
        _repositoryGenerator = repositoryGenerator;
        _validator = validator;
        _formatter = formatter;
    }

    /// <summary>
    /// Создает экземпляр QueryGenerator с зависимостями по умолчанию
    /// </summary>
    public static QueryGenerator Create()
    {
        var typeMapper = new PostgreSqlTypeMapper();
        var nameConverter = new NameConverter();
        var formatter = new RoslynFormatter();
        var sqlBuilder = new NpgsqlCommandBuilder();
        var syntaxBuilder = new QuerySyntaxBuilder(typeMapper, nameConverter);
        
        var methodGenerator = new QueryMethodGenerator(syntaxBuilder, sqlBuilder, nameConverter);
        var modelGenerator = new QueryModelGenerator(syntaxBuilder, typeMapper, nameConverter);
        var repositoryGenerator = new RepositoryGenerator(syntaxBuilder, methodGenerator);
        var validator = new QueryValidator();

        return new QueryGenerator(
            methodGenerator,
            modelGenerator,
            repositoryGenerator,
            validator,
            formatter);
    }

    public async ValueTask<QueryGenerationResult> GenerateAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        var allCode = new List<GeneratedCode>();
        var allIssues = new List<ValidationIssue>();

        // Валидация запросов
        var validationIssues = ValidateQueries(queries);
        allIssues.AddRange(validationIssues);

        // Если есть критические ошибки, останавливаемся
        if (validationIssues.Any(i => i.Severity == ValidationSeverity.Error))
        {
            return new QueryGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = allCode,
                Methods = new List<GeneratedMethodResult>(),
                ValidationIssues = allIssues,
                Duration = stopwatch.Elapsed
            };
        }

        // Генерация отдельных методов для каждого запроса
        var methods = new List<GeneratedMethodResult>();
        foreach (var query in queries)
        {
            var method = await GenerateMethodAsync(query, options);
            methods.Add(method);
        }

        // Генерация моделей результатов
        var resultModels = new List<GeneratedModelResult>();
        foreach (var query in queries.Where(q => q.ReturnType != null))
        {
            var model = await GenerateResultModelAsync(query, options);
            if (model.Code != null)
            {
                resultModels.Add(model);
                allCode.Add(model.Code);
            }
        }

        // Генерация моделей параметров (если требуется)
        var parameterModels = new List<GeneratedModelResult>();
        if (options.GenerateParameterModels)
        {
            foreach (var query in queries.Where(q => 
                q.Parameters.Count >= options.ParameterModelThreshold))
            {
                var model = await GenerateParameterModelAsync(query, options);
                if (model.Code != null)
                {
                    parameterModels.Add(model);
                    allCode.Add(model.Code);
                }
            }
        }

        // Генерация репозитория
        GeneratedClassResult? repositoryClass = null;
        GeneratedInterfaceResult? repositoryInterface = null;

        if (options.GenerateInterface)
        {
            repositoryInterface = await GenerateRepositoryInterfaceAsync(queries, options);
            if (repositoryInterface.Code != null)
            {
                allCode.Add(repositoryInterface.Code);
            }
        }

        repositoryClass = await GenerateRepositoryImplementationAsync(queries, options);
        if (repositoryClass.Code != null)
        {
            allCode.Add(repositoryClass.Code);
        }

        stopwatch.Stop();

        return new QueryGenerationResult
        {
            IsSuccess = !allIssues.Any(i => i.Severity == ValidationSeverity.Error),
            GeneratedCode = allCode,
            ValidationIssues = allIssues,
            Duration = stopwatch.Elapsed,
            Methods = methods, // ИСПРАВЛЕНО: теперь используем сгенерированные методы
            RepositoryInterface = repositoryInterface,
            RepositoryImplementation = repositoryClass,
            ResultModels = resultModels,
            ParameterModels = parameterModels,
            Statistics = CalculateStatistics(queries, allCode, allIssues)
        };
    }

    public async ValueTask<GeneratedMethodResult> GenerateMethodAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        return await _methodGenerator.GenerateAsync(queryMetadata, options);
    }

    public async ValueTask<GeneratedModelResult> GenerateResultModelAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        if (queryMetadata.ReturnType == null)
        {
            return new GeneratedModelResult
            {
                IsSuccess = false,
                ModelName = string.Empty,
                Code = null!
            };
        }

        return await _modelGenerator.GenerateResultModelAsync(queryMetadata, options);
    }

    public async ValueTask<GeneratedModelResult> GenerateParameterModelAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        if (queryMetadata.Parameters.Count < options.ParameterModelThreshold)
        {
            return new GeneratedModelResult
            {
                IsSuccess = false,
                ModelName = string.Empty,
                Code = null!
            };
        }

        return await _modelGenerator.GenerateParameterModelAsync(queryMetadata, options);
    }

    public async ValueTask<GeneratedInterfaceResult> GenerateRepositoryInterfaceAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        return await _repositoryGenerator.GenerateInterfaceAsync(queries, options);
    }

    public async ValueTask<GeneratedClassResult> GenerateRepositoryImplementationAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        return await _repositoryGenerator.GenerateImplementationAsync(queries, options);
    }

    public IReadOnlyList<ValidationIssue> ValidateQueries(IReadOnlyList<QueryMetadata> queries)
    {
        return _validator.Validate(queries);
    }

    public async ValueTask<string> FormatCodeAsync(string sourceCode)
    {
        return await _formatter.FormatAsync(sourceCode);
    }

    private static QueryGenerationStatistics CalculateStatistics(
        IReadOnlyList<QueryMetadata> queries,
        IReadOnlyList<GeneratedCode> code,
        IReadOnlyList<ValidationIssue> issues)
    {
        return new QueryGenerationStatistics
        {
            TotalFilesGenerated = code.Count,
            QueriesProcessed = queries.Count,
            MethodsGenerated = queries.Count,
            ResultModelsGenerated = code.Count(c => c.CodeType == GeneratedFileType.ResultModel),
            ParameterModelsGenerated = code.Count(c => c.CodeType == GeneratedFileType.ParameterModel),
            SelectQueriesCount = queries.Count(q => q.QueryType == QueryType.Select),
            InsertQueriesCount = queries.Count(q => q.QueryType == QueryType.Insert),
            UpdateQueriesCount = queries.Count(q => q.QueryType == QueryType.Update),
            DeleteQueriesCount = queries.Count(q => q.QueryType == QueryType.Delete),
            ErrorCount = issues.Count(i => i.Severity == ValidationSeverity.Error),
            WarningCount = issues.Count(i => i.Severity == ValidationSeverity.Warning)
        };
    }
}
