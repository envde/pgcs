using Microsoft.CodeAnalysis.CSharp;
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
public sealed class QueryModelGenerator(
    QuerySyntaxBuilder syntaxBuilder,
    ITypeMapper typeMapper)
    : IQueryModelGenerator
{
    public GeneratedModelResult GenerateResultModel(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        if (queryMetadata.ReturnType == null || !queryMetadata.ReturnType.Columns.Any())
        {
            return new GeneratedModelResult
            {
                IsSuccess = false,
                ModelName = string.Empty,
                Code = null!
            };
        }

        // Определяем имя модели
        var modelName = queryMetadata.ExplicitModelName 
            ?? queryMetadata.ReturnType.ModelName
            ?? $"{queryMetadata.MethodName}Result";

        // Проверяем, нужно ли создавать модель (может использоваться существующая)
        if (options.ReuseSchemaModels && !queryMetadata.ReturnType.RequiresCustomModel)
        {
            return new GeneratedModelResult
            {
                IsSuccess = true,
                ModelName = modelName,
                Code = null! // Модель уже существует
            };
        }

        // Создаем compilation unit
        var compilationUnit = syntaxBuilder.BuildResultModelCompilationUnit(
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

        return new GeneratedModelResult
        {
            IsSuccess = true,
            ModelName = modelName,
            Code = code
        };
    }

    public GeneratedModelResult GenerateParameterModel(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        if (!options.GenerateParameterModels || 
            queryMetadata.Parameters.Count < options.ParameterModelThreshold)
        {
            return new GeneratedModelResult
            {
                IsSuccess = false,
                ModelName = string.Empty,
                Code = null!
            };
        }

        var modelName = $"{queryMetadata.MethodName}Parameters";

        // Создаем класс модели параметров
        var classDeclaration = syntaxBuilder.BuildParameterModelClass(
            modelName,
            queryMetadata.Parameters);

        // Собираем usings
        var usings = new HashSet<string> { "System" };
        foreach (var param in queryMetadata.Parameters)
        {
            var ns = typeMapper.GetRequiredNamespace(param.PostgresType);
            if (ns != null)
            {
                usings.Add(ns);
            }
        }

        // Создаем compilation unit
        var usingDirectives = usings
            .OrderBy(u => u)
            .Select(u => SyntaxFactory.UsingDirective(
                SyntaxFactory.IdentifierName(u)))
            .ToArray();

        var fileScopedNamespace = SyntaxFactory
            .FileScopedNamespaceDeclaration(
                SyntaxFactory.IdentifierName(options.RootNamespace))
            .AddMembers(classDeclaration);

        var compilationUnit = SyntaxFactory.CompilationUnit()
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

        return new GeneratedModelResult
        {
            IsSuccess = true,
            ModelName = modelName,
            Code = code
        };
    }
}
