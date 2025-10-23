using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;
using PgCs.QueryGenerator.Services;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PgCs.QueryGenerator.Generators;

/// <summary>
/// Генератор интерфейса и реализации репозитория запросов с использованием Roslyn
/// </summary>
public sealed class RepositoryGenerator : IRepositoryGenerator
{
    private readonly QuerySyntaxBuilder _syntaxBuilder;
    private readonly IQueryMethodGenerator _methodGenerator;

    public RepositoryGenerator(
        QuerySyntaxBuilder syntaxBuilder,
        IQueryMethodGenerator methodGenerator)
    {
        _syntaxBuilder = syntaxBuilder;
        _methodGenerator = methodGenerator;
    }

    public ValueTask<GeneratedInterfaceResult> GenerateInterfaceAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        // Генерируем методы для всех запросов
        var methods = new List<MethodDeclarationSyntax>();
        foreach (var query in queries)
        {
            var method = BuildInterfaceMethod(query, options);
            methods.Add(method);
        }

        // Используем QuerySyntaxBuilder для построения интерфейса
        var interfaceDecl = _syntaxBuilder.BuildRepositoryInterface(
            options.RepositoryInterfaceName,
            methods);

        // Оборачиваем в CompilationUnit с namespace и usings
        var compilationUnit = CompilationUnit()
            .AddUsings(
                UsingDirective(IdentifierName("System")),
                UsingDirective(IdentifierName("System.Collections.Generic")),
                UsingDirective(IdentifierName("System.Threading")),
                UsingDirective(IdentifierName("System.Threading.Tasks")),
                UsingDirective(IdentifierName("Npgsql")))
            .AddMembers(
                FileScopedNamespaceDeclaration(IdentifierName(options.RootNamespace))
                    .AddMembers(interfaceDecl))
            .NormalizeWhitespace();

        var sourceCode = compilationUnit.ToFullString();

        var code = new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = options.RepositoryInterfaceName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.RepositoryInterface,
        };

        return ValueTask.FromResult(new GeneratedInterfaceResult
        {
            IsSuccess = true,
            InterfaceName = options.RepositoryInterfaceName,
            Code = code,
            MethodCount = queries.Count
        });
    }

    public ValueTask<GeneratedClassResult> GenerateImplementationAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        // Генерируем методы для всех запросов
        var methods = new List<MethodDeclarationSyntax>();
        foreach (var query in queries)
        {
            var method = BuildClassMethod(query, options);
            methods.Add(method);
        }

        // Используем QuerySyntaxBuilder для построения класса
        var classDecl = _syntaxBuilder.BuildRepositoryClass(
            options.RepositoryClassName,
            options.GenerateInterface ? options.RepositoryInterfaceName : null,
            methods);

        // Оборачиваем в CompilationUnit с namespace и usings
        var compilationUnit = CompilationUnit()
            .AddUsings(
                UsingDirective(IdentifierName("System")),
                UsingDirective(IdentifierName("System.Collections.Generic")),
                UsingDirective(IdentifierName("System.Threading")),
                UsingDirective(IdentifierName("System.Threading.Tasks")),
                UsingDirective(IdentifierName("Npgsql")))
            .AddMembers(
                FileScopedNamespaceDeclaration(IdentifierName(options.RootNamespace))
                    .AddMembers(classDecl))
            .NormalizeWhitespace();

        var sourceCode = compilationUnit.ToFullString();

        var code = new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = options.RepositoryClassName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.RepositoryClass,
        };

        return ValueTask.FromResult(new GeneratedClassResult
        {
            IsSuccess = true,
            ClassName = options.RepositoryClassName,
            Code = code,
            MethodCount = queries.Count
        });
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

        return MethodDeclaration(ParseTypeName(returnType), queryMetadata.MethodName + "Async")
            .AddParameterListParameters(parameters.ToArray())
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(_syntaxBuilder.CreateXmlComment(
                $"Выполняет запрос: {queryMetadata.MethodName}",
                options.IncludeSqlInDocumentation ? queryMetadata.SqlQuery : null));
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
        
        // ИСПРАВЛЕНО: Используем QueryMethodGenerator для построения тела метода
        var methodResult = _methodGenerator.GenerateAsync(queryMetadata, options).GetAwaiter().GetResult();
        
        // Парсим сгенерированный метод и извлекаем его тело
        var parsedMethod = ParseMemberDeclaration(methodResult.SourceCode) as MethodDeclarationSyntax;
        var body = parsedMethod?.Body ?? BuildFallbackMethodBody(queryMetadata, options);

        return MethodDeclaration(ParseTypeName(returnType), queryMetadata.MethodName + "Async")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
            .AddParameterListParameters(parameters.ToArray())
            .WithBody(body)
            .WithLeadingTrivia(_syntaxBuilder.CreateXmlComment(
                $"Выполняет запрос: {queryMetadata.MethodName}",
                options.IncludeSqlInDocumentation ? queryMetadata.SqlQuery : null));
    }

    /// <summary>
    /// Резервное тело метода (если парсинг не удался)
    /// </summary>
    private BlockSyntax BuildFallbackMethodBody(QueryMetadata queryMetadata, QueryGenerationOptions options)
    {
        return Block(
            ThrowStatement(
                ObjectCreationExpression(ParseTypeName("NotImplementedException"))
                    .WithArgumentList(ArgumentList(
                        SeparatedList(new[] {
                            Argument(LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal($"Method {queryMetadata.MethodName} not implemented")))
                        })))));
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
            var paramName = ConvertParameterName(param.Name);
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
                    ? $"Task<{queryMetadata.ReturnType.ModelName}?>"
                    : $"Task<List<{queryMetadata.ReturnType.ModelName}>>",
            QueryType.Insert or QueryType.Update or QueryType.Delete => "Task<int>",
            _ => "Task"
        };
    }

    /// <summary>
    /// Конвертирует имя параметра из SQL в C# стиль
    /// </summary>
    private string ConvertParameterName(string sqlName)
    {
        // Убираем префиксы @ и $
        var name = sqlName.TrimStart('@', '$');

        // Конвертируем snake_case в camelCase
        var parts = name.Split('_');
        if (parts.Length == 1)
            return char.ToLower(name[0]) + name.Substring(1);

        var result = parts[0].ToLower();
        for (int i = 1; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                result += char.ToUpper(parts[i][0]) + parts[i].Substring(1).ToLower();
            }
        }

        return result;
    }

    /// <summary>
    /// Создает XML комментарий для документации метода
    /// </summary>
    public SyntaxTriviaList CreateXmlComment(string summary, string? additionalInfo = null)
    {
        var lines = new List<string>
        {
            "/// <summary>",
            $"/// {summary}",
            "/// </summary>"
        };

        if (!string.IsNullOrWhiteSpace(additionalInfo))
        {
            lines.Add("/// <remarks>");
            
            // Разбиваем дополнительную информацию на строки для лучшего форматирования
            var infoLines = additionalInfo.Split('\n');
            foreach (var line in infoLines)
            {
                lines.Add($"/// {line.TrimEnd()}");
            }
            
            lines.Add("/// </remarks>");
        }

        var triviaList = TriviaList();
        foreach (var line in lines)
        {
            triviaList = triviaList.Add(
                Comment(line + Environment.NewLine));
        }

        return triviaList;
    }
}
