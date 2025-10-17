using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;
using PgCs.Common.Services;
using PgCs.QueryGenerator.Services;

namespace PgCs.QueryGenerator.Generators;

/// <summary>
/// Генератор моделей для результатов и параметров запросов
/// </summary>
public sealed class QueryModelGenerator : IQueryModelGenerator
{
    private readonly QuerySyntaxBuilder _syntaxBuilder;
    private readonly ITypeMapper _typeMapper;
    private readonly INameConverter _nameConverter;

    public QueryModelGenerator(
        QuerySyntaxBuilder syntaxBuilder,
        ITypeMapper typeMapper,
        INameConverter nameConverter)
    {
        _syntaxBuilder = syntaxBuilder;
        _typeMapper = typeMapper;
        _nameConverter = nameConverter;
    }

    public ValueTask<GeneratedModelResult> GenerateResultModelAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        if (queryMetadata.ReturnType == null || !queryMetadata.ReturnType.Columns.Any())
        {
            return ValueTask.FromResult(new GeneratedModelResult
            {
                IsSuccess = false,
                ModelName = string.Empty,
                Code = null!
            });
        }

        // Определяем имя модели
        var modelName = queryMetadata.ExplicitModelName 
            ?? queryMetadata.ReturnType.ModelName
            ?? $"{queryMetadata.MethodName}Result";

        // Проверяем, нужно ли создавать модель (может использоваться существующая)
        if (options.ReuseSchemaModels && !queryMetadata.ReturnType.RequiresCustomModel)
        {
            return ValueTask.FromResult(new GeneratedModelResult
            {
                IsSuccess = true,
                ModelName = modelName,
                Code = null! // Модель уже существует
            });
        }

        // Создаем compilation unit
        var compilationUnit = _syntaxBuilder.BuildResultModelCompilationUnit(
            options.RootNamespace,
            modelName,
            queryMetadata.ReturnType.Columns);

        var sourceCode = compilationUnit.ToFullString();

        // Определяем путь к файлу
        var fileName = $"{modelName}.cs";
        var filePath = Path.Combine("Models", "Results", fileName);

        var code = new GeneratedCode
        {
            // FilePath removed: //filePath,
            SourceCode = sourceCode,
            TypeName = modelName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.ResultModel,
        };

        return ValueTask.FromResult(new GeneratedModelResult
        {
            IsSuccess = true,
            ModelName = modelName,
            Code = code
        });
    }

    public ValueTask<GeneratedModelResult> GenerateParameterModelAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        if (!options.GenerateParameterModels || 
            queryMetadata.Parameters.Count < options.ParameterModelThreshold)
        {
            return ValueTask.FromResult(new GeneratedModelResult
            {
                IsSuccess = false,
                ModelName = string.Empty,
                Code = null!
            });
        }

        var modelName = $"{queryMetadata.MethodName}Parameters";

        // Создаем класс модели параметров
        var classDeclaration = _syntaxBuilder.BuildParameterModelClass(
            modelName,
            queryMetadata.Parameters);

        // Собираем usings
        var usings = new HashSet<string> { "System" };
        foreach (var param in queryMetadata.Parameters)
        {
            var ns = _typeMapper.GetRequiredNamespace(param.PostgresType);
            if (ns != null)
            {
                usings.Add(ns);
            }
        }

        // Создаем compilation unit
        var usingDirectives = usings
            .OrderBy(u => u)
            .Select(u => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(u)))
            .ToArray();

        var fileScopedNamespace = Microsoft.CodeAnalysis.CSharp.SyntaxFactory
            .FileScopedNamespaceDeclaration(
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(options.RootNamespace))
            .AddMembers(classDeclaration);

        var compilationUnit = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectives)
            .AddMembers(fileScopedNamespace);

        var syntaxTree = CSharpSyntaxTree.Create(compilationUnit);
        var sourceCode = syntaxTree.ToString();

        // Определяем путь к файлу
        var fileName = $"{modelName}.cs";
        var filePath = Path.Combine("Models", "Parameters", fileName);

        var code = new GeneratedCode
        {
            // FilePath removed: //filePath,
            SourceCode = sourceCode,
            TypeName = modelName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.ParameterModel,
        };

        return ValueTask.FromResult(new GeneratedModelResult
        {
            IsSuccess = true,
            ModelName = modelName,
            Code = code
        });
    }
}
