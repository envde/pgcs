using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;
using PgCs.Common.Services;
using PgCs.QueryGenerator.Services;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PgCs.QueryGenerator.Generators;

/// <summary>
/// Генератор интерфейса и реализации репозитория запросов с использованием Roslyn
/// </summary>
public sealed class RepositoryGenerator(
    QuerySyntaxBuilder syntaxBuilder,
    IQueryMethodGenerator methodGenerator,
    INameConverter nameConverter)
    : IRepositoryGenerator
{
    public GeneratedInterfaceResult GenerateInterface( IReadOnlyList<QueryMetadata> queries, QueryGenerationOptions options)
    {
        // Генерируем методы для всех запросов
        var methods = new List<MethodDeclarationSyntax>();
        foreach (var query in queries)
        {
            var method = BuildInterfaceMethod(query, options);
            methods.Add(method);
        }

        // Используем QuerySyntaxBuilder для построения интерфейса
        var interfaceDecl = syntaxBuilder.BuildRepositoryInterface(
            options.RepositoryInterfaceName,
            methods);

        // Определяем usings для интерфейса
        var usings = new List<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Threading",
            "System.Threading.Tasks",
            "Npgsql"
        };
        
        // Добавляем namespace моделей схемы если используем их
        if (!string.IsNullOrWhiteSpace(options.SchemaModelsNamespace))
        {
            usings.Add(options.SchemaModelsNamespace);
        }

        // Используем общий helper для создания compilation unit
        var compilationUnit = RoslynSyntaxHelpers.BuildCompilationUnit(
            options.RootNamespace,
            interfaceDecl,
            usings);

        var sourceCode = compilationUnit.ToFullString();
        
        // Форматируем XML документацию
        sourceCode = RoslynSyntaxHelpers.FormatXmlDocumentation(sourceCode);

        var code = new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = options.RepositoryInterfaceName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.RepositoryInterface,
        };

        return new GeneratedInterfaceResult
        {
            IsSuccess = true,
            InterfaceName = options.RepositoryInterfaceName,
            Code = code,
            MethodCount = queries.Count
        };
    }

    public GeneratedClassResult GenerateImplementation( IReadOnlyList<QueryMetadata> queries, QueryGenerationOptions options)
    {
        // Генерируем методы для всех запросов
        var methods = new List<MethodDeclarationSyntax>();
        foreach (var query in queries)
        {
            var method = BuildClassMethod(query, options);
            methods.Add(method);
        }

        // Используем QuerySyntaxBuilder для построения класса
        var classDecl = syntaxBuilder.BuildRepositoryClass(
            options.RepositoryClassName,
            options.GenerateInterface ? options.RepositoryInterfaceName : null,
            methods);

        // Определяем usings для класса
        var usings = new List<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Threading",
            "System.Threading.Tasks",
            "Npgsql"
        };
        
        // Добавляем namespace моделей схемы если используем их
        if (!string.IsNullOrWhiteSpace(options.SchemaModelsNamespace))
        {
            usings.Add(options.SchemaModelsNamespace);
        }

        // Используем общий helper для создания compilation unit
        var compilationUnit = RoslynSyntaxHelpers.BuildCompilationUnit(
            options.RootNamespace,
            classDecl,
            usings);

        var sourceCode = compilationUnit.ToFullString();
        
        // Форматируем XML документацию
        sourceCode = RoslynSyntaxHelpers.FormatXmlDocumentation(sourceCode);

        var code = new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = options.RepositoryClassName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.RepositoryClass,
        };

        return new GeneratedClassResult
        {
            IsSuccess = true,
            ClassName = options.RepositoryClassName,
            Code = code,
            MethodCount = queries.Count
        };
    }

    /// <summary>
    /// Строит метод интерфейса (только сигнатура)
    /// </summary>
    private MethodDeclarationSyntax BuildInterfaceMethod(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        var returnType = GetReturnType(queryMetadata);
        var parameters = BuildParameters(queryMetadata, options);

        var xmlComment = syntaxBuilder.CreateQueryMethodXmlComment(
            queryMetadata,
            options.IncludeSqlInDocumentation);

        return MethodDeclaration(ParseTypeName(returnType), queryMetadata.MethodName + "Async")
            .AddParameterListParameters(parameters.ToArray())
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(xmlComment);
    }

    /// <summary>
    /// Строит метод класса (с телом)
    /// </summary>
    private MethodDeclarationSyntax BuildClassMethod(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        var returnType = GetReturnType(queryMetadata);
        var parameters = BuildParameters(queryMetadata, options);
        
        var methodResult = methodGenerator.Generate(queryMetadata, options);
        
        // Парсим сгенерированный метод и извлекаем его тело
        var parsedMethod = ParseMemberDeclaration(methodResult.SourceCode) as MethodDeclarationSyntax;
        var body = parsedMethod?.Body ?? BuildFallbackMethodBody(queryMetadata);

        var xmlComment = syntaxBuilder.CreateQueryMethodXmlComment(
            queryMetadata,
            options.IncludeSqlInDocumentation);

        return MethodDeclaration(ParseTypeName(returnType), queryMetadata.MethodName + "Async")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
            .AddParameterListParameters(parameters.ToArray())
            .WithBody(body)
            .WithLeadingTrivia(xmlComment);
    }

    /// <summary>
    /// Резервное тело метода (если парсинг не удался)
    /// </summary>
    private BlockSyntax BuildFallbackMethodBody(QueryMetadata queryMetadata)
    {
        return Block(
            ThrowStatement(
                ObjectCreationExpression(ParseTypeName("NotImplementedException"))
                    .WithArgumentList(ArgumentList(
                        SeparatedList([
                            Argument(LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal($"Method {queryMetadata.MethodName} not implemented")))
                        ])))));
    }

    /// <summary>
    /// Строит список параметров метода
    /// </summary>
    private List<ParameterSyntax> BuildParameters(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        var parameters = new List<ParameterSyntax>();

        // Параметры запроса
        foreach (var param in queryMetadata.Parameters)
        {
            var paramName = nameConverter.ToParameterName(param.Name);
            parameters.Add(
                Parameter(Identifier(paramName))
                    .WithType(ParseTypeName(param.CSharpType)));
        }

        // Транзакция (опциональная)
        if (options.GenerateTransactionSupport)
        {
            parameters.Add(
                Parameter(Identifier("transaction"))
                    .WithType(ParseTypeName("NpgsqlTransaction?"))
                    .WithDefault(EqualsValueClause(
                        LiteralExpression(SyntaxKind.NullLiteralExpression))));
        }

        // CancellationToken
        if (options.SupportCancellation)
        {
            parameters.Add(
                Parameter(Identifier("cancellationToken"))
                    .WithType(ParseTypeName("CancellationToken"))
                    .WithDefault(EqualsValueClause(
                        LiteralExpression(SyntaxKind.DefaultLiteralExpression))));
        }

        return parameters;
    }

    /// <summary>
    /// Получает тип возврата метода
    /// </summary>
    private string GetReturnType(QueryMetadata queryMetadata)
    {
        return queryMetadata.QueryType switch
        {
            QueryType.Select when queryMetadata.ReturnType != null =>
                queryMetadata.ReturnCardinality == ReturnCardinality.One
                    ? $"ValueTask<{queryMetadata.ReturnType.ModelName}?>"
                    : $"ValueTask<List<{queryMetadata.ReturnType.ModelName}>>",
            QueryType.Insert or QueryType.Update or QueryType.Delete => "ValueTask<int>",
            _ => "ValueTask"
        };
    }
}
