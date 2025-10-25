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
public sealed class QueryGenerator(
    IQueryMethodGenerator methodGenerator,
    IQueryModelGenerator modelGenerator,
    IRepositoryGenerator repositoryGenerator,
    IQueryValidator validator,
    IRoslynFormatter formatter)
    : IQueryGenerator
{
    /// <summary>
    /// Создает экземпляр QueryGenerator с зависимостями по умолчанию
    /// </summary>
    public static QueryGenerator Create()
    {
        var typeMapper = new PostgreSqlTypeMapper();
        var nameConverter = new NameConverter();
        var syntaxBuilder = new QuerySyntaxBuilder(typeMapper, nameConverter);
        
        var methodGenerator = new QueryMethodGenerator(syntaxBuilder, nameConverter);
        var modelGenerator = new QueryModelGenerator(syntaxBuilder, typeMapper);
        var repositoryGenerator = new RepositoryGenerator(syntaxBuilder, methodGenerator, nameConverter);
        var validator = new QueryValidator();
        var formatter = new RoslynFormatter();

        return new QueryGenerator(
            methodGenerator,
            modelGenerator,
            repositoryGenerator,
            validator,
            formatter);
    }

    public QueryGenerationResult Generate( IReadOnlyList<QueryMetadata> queries, QueryGenerationOptions options)
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
            var method = GenerateMethod(query, options);
            methods.Add(method);
        }

        // Генерация моделей результатов
        var resultModels = new List<GeneratedModelResult>();
        foreach (var query in queries.Where(q => q.ReturnType != null))
        {
            var model = GenerateResultModel(query, options);
            resultModels.Add(model);
            if (model.Code != null)
            {
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
                var model = GenerateParameterModel(query, options);
                parameterModels.Add(model);
                if (model.Code != null)
                {
                    allCode.Add(model.Code);
                }
            }
        }

        // Генерация репозитория
        GeneratedInterfaceResult? repositoryInterface = null;

        if (options.GenerateInterface)
        {
            repositoryInterface = GenerateRepositoryInterface(queries, options);
            allCode.Add(repositoryInterface.Code);
        }

        var repositoryClass = GenerateRepositoryImplementation(queries, options);
        allCode.Add(repositoryClass.Code);

        stopwatch.Stop();

        return new QueryGenerationResult
        {
            IsSuccess = allIssues.All(i => i.Severity != ValidationSeverity.Error),
            GeneratedCode = allCode,
            ValidationIssues = allIssues,
            Duration = stopwatch.Elapsed,
            Methods = methods,
            RepositoryInterface = repositoryInterface,
            RepositoryImplementation = repositoryClass,
            ResultModels = resultModels,
            ParameterModels = parameterModels,
            Statistics = CalculateStatistics(queries, allCode, allIssues)
        };
    }

    public GeneratedMethodResult GenerateMethod( QueryMetadata queryMetadata, QueryGenerationOptions options)
    {
        return methodGenerator.Generate(queryMetadata, options);
    }

    public GeneratedModelResult GenerateResultModel( QueryMetadata queryMetadata, QueryGenerationOptions options)
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

        return modelGenerator.GenerateResultModel(queryMetadata, options);
    }

    public GeneratedModelResult GenerateParameterModel( QueryMetadata queryMetadata, QueryGenerationOptions options)
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

        return modelGenerator.GenerateParameterModel(queryMetadata, options);
    }

    public GeneratedInterfaceResult GenerateRepositoryInterface( IReadOnlyList<QueryMetadata> queries, QueryGenerationOptions options)
    {
        return repositoryGenerator.GenerateInterface(queries, options);
    }

    public GeneratedClassResult GenerateRepositoryImplementation( IReadOnlyList<QueryMetadata> queries, QueryGenerationOptions options)
    {
        return repositoryGenerator.GenerateImplementation(queries, options);
    }

    public IReadOnlyList<ValidationIssue> ValidateQueries(IReadOnlyList<QueryMetadata> queries)
    {
        return validator.Validate(queries);
    }

    public string FormatCode(string sourceCode)
    {
        return formatter.Format(sourceCode);
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
            ResultModelsGenerated = code.Count(c => c != null && c.CodeType == GeneratedFileType.ResultModel),
            ParameterModelsGenerated = code.Count(c => c != null && c.CodeType == GeneratedFileType.ParameterModel),
            SelectQueriesCount = queries.Count(q => q.QueryType == QueryType.Select),
            InsertQueriesCount = queries.Count(q => q.QueryType == QueryType.Insert),
            UpdateQueriesCount = queries.Count(q => q.QueryType == QueryType.Update),
            DeleteQueriesCount = queries.Count(q => q.QueryType == QueryType.Delete),
            ErrorCount = issues.CountErrors(),
            WarningCount = issues.CountWarnings()
        };
    }
}
