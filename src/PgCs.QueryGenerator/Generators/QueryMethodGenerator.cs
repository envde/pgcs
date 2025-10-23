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
/// Генератор C# методов для SQL запросов с использованием Roslyn
/// </summary>
public sealed class QueryMethodGenerator : IQueryMethodGenerator
{
    private readonly QuerySyntaxBuilder _syntaxBuilder;
    private readonly INameConverter _nameConverter;

    public QueryMethodGenerator(
        QuerySyntaxBuilder syntaxBuilder,
        INameConverter nameConverter)
    {
        _syntaxBuilder = syntaxBuilder;
        _nameConverter = nameConverter;
    }

    public ValueTask<GeneratedMethodResult> GenerateAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        var method = BuildQueryMethod(queryMetadata, options);
        var sourceCode = method.NormalizeWhitespace().ToFullString();

        return ValueTask.FromResult(new GeneratedMethodResult
        {
            IsSuccess = true,
            MethodName = queryMetadata.MethodName,
            MethodSignature = GetMethodSignature(queryMetadata, options),
            SourceCode = sourceCode,
            SqlQuery = queryMetadata.SqlQuery
        });
    }

    /// <summary>
    /// Строит метод запроса с использованием Roslyn
    /// </summary>
    private MethodDeclarationSyntax BuildQueryMethod(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        var returnType = GetReturnType(queryMetadata);
        var parameters = BuildParameters(queryMetadata, options);
        var body = BuildMethodBody(queryMetadata, options);

        var method = MethodDeclaration(ParseTypeName(returnType), queryMetadata.MethodName + "Async")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
            .AddParameterListParameters(parameters.ToArray())
            .WithBody(body)
            .WithLeadingTrivia(_syntaxBuilder.CreateXmlComment(
                $"Выполняет запрос: {queryMetadata.MethodName}",
                options.IncludeSqlInDocumentation ? queryMetadata.SqlQuery : null));

        return method;
    }

    /// <summary>
    /// Строит тело метода
    /// </summary>
    private BlockSyntax BuildMethodBody(QueryMetadata queryMetadata, QueryGenerationOptions options)
    {
        var statements = new List<StatementSyntax>();

        // 1. Получение соединения
        statements.Add(BuildConnectionStatement(options));

        // 2. Создание команды
        statements.Add(BuildCommandStatement(queryMetadata, options));

        // 3. Добавление параметров
        statements.AddRange(BuildParameterStatements(queryMetadata));

        // 4. Выполнение запроса
        statements.AddRange(BuildExecutionStatements(queryMetadata, options));

        return Block(statements);
    }

    /// <summary>
    /// Создаёт statement для получения соединения
    /// </summary>
    private LocalDeclarationStatementSyntax BuildConnectionStatement(QueryGenerationOptions options)
    {
        ExpressionSyntax initializer;

        if (options.GenerateTransactionSupport)
        {
            // transaction?.Connection ?? await _dataSource.OpenConnectionAsync(cancellationToken)
            initializer = BinaryExpression(
                SyntaxKind.CoalesceExpression,
                ConditionalAccessExpression(
                    IdentifierName("transaction"),
                    MemberBindingExpression(IdentifierName("Connection"))),
                AwaitExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("_dataSource"),
                            IdentifierName("OpenConnectionAsync")))
                        .AddArgumentListArguments(
                            Argument(IdentifierName("cancellationToken")))));
        }
        else
        {
            // await _dataSource.OpenConnectionAsync(cancellationToken)
            initializer = AwaitExpression(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("_dataSource"),
                        IdentifierName("OpenConnectionAsync")))
                    .AddArgumentListArguments(
                        Argument(IdentifierName("cancellationToken"))));
        }

        return LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .AddVariables(
                    VariableDeclarator("connection")
                        .WithInitializer(EqualsValueClause(initializer))))
            .WithAwaitKeyword(Token(SyntaxKind.AwaitKeyword))
            .WithUsingKeyword(Token(SyntaxKind.UsingKeyword));
    }

    /// <summary>
    /// Создаёт statement для создания команды
    /// </summary>
    private LocalDeclarationStatementSyntax BuildCommandStatement(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        var arguments = new List<ArgumentSyntax>
        {
            Argument(LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(queryMetadata.SqlQuery))),
            Argument(IdentifierName("connection"))
        };

        if (options.GenerateTransactionSupport)
        {
            arguments.Add(Argument(IdentifierName("transaction")));
        }

        var initializer = ObjectCreationExpression(ParseTypeName("NpgsqlCommand"))
            .WithArgumentList(ArgumentList(SeparatedList(arguments)));

        return LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .AddVariables(
                    VariableDeclarator("cmd")
                        .WithInitializer(EqualsValueClause(initializer))))
            .WithAwaitKeyword(Token(SyntaxKind.AwaitKeyword))
            .WithUsingKeyword(Token(SyntaxKind.UsingKeyword));
    }

    /// <summary>
    /// Создаёт statements для добавления параметров
    /// </summary>
    private IEnumerable<StatementSyntax> BuildParameterStatements(QueryMetadata queryMetadata)
    {
        foreach (var param in queryMetadata.Parameters)
        {
            var paramName = _nameConverter.ToParameterName(param.Name);
            
            yield return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("cmd"),
                            IdentifierName("Parameters")),
                        IdentifierName("AddWithValue")))
                    .AddArgumentListArguments(
                        Argument(LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(param.Name))),
                        Argument(IdentifierName(paramName))));
        }
    }

    /// <summary>
    /// Создаёт statements для выполнения запроса
    /// </summary>
    private IEnumerable<StatementSyntax> BuildExecutionStatements(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        return queryMetadata.QueryType switch
        {
            QueryType.Select when queryMetadata.ReturnType != null =>
                BuildSelectStatements(queryMetadata, options),
            QueryType.Insert or QueryType.Update or QueryType.Delete =>
                BuildModificationStatements(),
            _ => BuildVoidStatements()
        };
    }

    /// <summary>
    /// Создаёт statements для SELECT запроса
    /// </summary>
    private IEnumerable<StatementSyntax> BuildSelectStatements(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        // await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        yield return LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .AddVariables(
                    VariableDeclarator("reader")
                        .WithInitializer(EqualsValueClause(
                            AwaitExpression(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("cmd"),
                                        IdentifierName("ExecuteReaderAsync")))
                                    .AddArgumentListArguments(
                                        Argument(IdentifierName("cancellationToken"))))))))
            .WithAwaitKeyword(Token(SyntaxKind.AwaitKeyword))
            .WithUsingKeyword(Token(SyntaxKind.UsingKeyword));

        if (queryMetadata.ReturnCardinality == ReturnCardinality.One)
        {
            // if (!await reader.ReadAsync(cancellationToken)) return null;
            yield return IfStatement(
                PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    AwaitExpression(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("reader"),
                                IdentifierName("ReadAsync")))
                            .AddArgumentListArguments(
                                Argument(IdentifierName("cancellationToken"))))),
                ReturnStatement(LiteralExpression(SyntaxKind.NullLiteralExpression)));

            // return MapResult(reader);
            yield return ReturnStatement(
                InvocationExpression(IdentifierName($"MapTo{queryMetadata.ReturnType!.ModelName}"))
                    .AddArgumentListArguments(Argument(IdentifierName("reader"))));
        }
        else
        {
            // var result = new List<Model>();
            yield return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .AddVariables(
                        VariableDeclarator("result")
                            .WithInitializer(EqualsValueClause(
                                ObjectCreationExpression(
                                    GenericName("List")
                                        .AddTypeArgumentListArguments(
                                            IdentifierName(queryMetadata.ReturnType!.ModelName)))
                                    .WithArgumentList(ArgumentList())))));

            // while (await reader.ReadAsync(cancellationToken)) { result.Add(...); }
            yield return WhileStatement(
                AwaitExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("reader"),
                            IdentifierName("ReadAsync")))
                        .AddArgumentListArguments(
                            Argument(IdentifierName("cancellationToken")))),
                Block(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("result"),
                                IdentifierName("Add")))
                            .AddArgumentListArguments(
                                Argument(
                                    InvocationExpression(
                                        IdentifierName($"MapTo{queryMetadata.ReturnType.ModelName}"))
                                        .AddArgumentListArguments(
                                            Argument(IdentifierName("reader"))))))));

            // return result;
            yield return ReturnStatement(IdentifierName("result"));
        }
    }

    /// <summary>
    /// Создаёт statements для INSERT/UPDATE/DELETE
    /// </summary>
    private IEnumerable<StatementSyntax> BuildModificationStatements()
    {
        // return await cmd.ExecuteNonQueryAsync(cancellationToken);
        yield return ReturnStatement(
            AwaitExpression(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("cmd"),
                        IdentifierName("ExecuteNonQueryAsync")))
                    .AddArgumentListArguments(
                        Argument(IdentifierName("cancellationToken")))));
    }

    /// <summary>
    /// Создаёт statements для void запросов
    /// </summary>
    private IEnumerable<StatementSyntax> BuildVoidStatements()
    {
        // await cmd.ExecuteNonQueryAsync(cancellationToken);
        yield return ExpressionStatement(
            AwaitExpression(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("cmd"),
                        IdentifierName("ExecuteNonQueryAsync")))
                    .AddArgumentListArguments(
                        Argument(IdentifierName("cancellationToken")))));
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
            var paramName = _nameConverter.ToParameterName(param.Name);
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
    /// Получает сигнатуру метода для отображения
    /// </summary>
    private string GetMethodSignature(QueryMetadata queryMetadata, QueryGenerationOptions options)
    {
        var returnType = GetReturnType(queryMetadata);
        var parameters = BuildParameters(queryMetadata, options);
        var paramStrings = parameters.Select(p => $"{p.Type} {p.Identifier}").ToList();

        return $"public async {returnType} {queryMetadata.MethodName}Async({string.Join(", ", paramStrings)})";
    }
}

