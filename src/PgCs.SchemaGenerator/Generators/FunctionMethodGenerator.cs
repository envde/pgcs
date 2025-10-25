using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.Common.Services;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# методов для функций PostgreSQL с использованием Roslyn
/// </summary>
public sealed class FunctionMethodGenerator(ITypeMapper typeMapper, INameConverter nameConverter)
    : IFunctionMethodGenerator
{
    public IReadOnlyList<GeneratedCode> Generate( IReadOnlyList<FunctionDefinition> functions, SchemaGenerationOptions options)
    {
        if (!functions.Any())
            return [];

        var generatedCode = GenerateFunctionsRepositoryClass(functions, options);
        
        return [generatedCode];  
    }

    private GeneratedCode GenerateFunctionsRepositoryClass(
        IReadOnlyList<FunctionDefinition> functions,
        SchemaGenerationOptions options)
    {
        var className = "DatabaseFunctions";
        
        // Создаём методы для каждой функции
        var methods = functions
            .Select(GenerateFunctionMethod)
            .Cast<MemberDeclarationSyntax>()
            .ToArray();

        // Создаём класс репозитория
        var classDeclaration = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword))
            .AddMembers(methods);

        // Добавляем using директивы
        var usings = new[]
        {
            "System",
            "System.Threading.Tasks",
            "Npgsql"
        };

        // Используем общий helper для создания compilation unit
        var compilationUnit = RoslynSyntaxHelpers.BuildCompilationUnit(
            options.RootNamespace,
            classDeclaration,
            usings);

        var sourceCode = compilationUnit.ToFullString();

        return new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = className,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.RepositoryClass
        };
    }

    private MethodDeclarationSyntax GenerateFunctionMethod(FunctionDefinition function)
    {
        var methodName = nameConverter.ToMethodName(function.Name);
        var returnType = MapReturnType(function.ReturnType);
        
        // Параметры метода
        var parameters = new List<ParameterSyntax>
        {
            Parameter(Identifier("connection"))
                .WithType(ParseTypeName("NpgsqlConnection"))
        };

        // Добавляем параметры функции
        foreach (var param in function.Parameters)
        {
            var paramType = typeMapper.MapType(param.DataType, isNullable: false, isArray: false);
            var paramName = nameConverter.ToParameterName(param.Name);
            
            parameters.Add(
                Parameter(Identifier(paramName))
                    .WithType(ParseTypeName(paramType))
            );
        }

        // Создаём тело метода
        var body = GenerateMethodBody(function, parameters.Skip(1).ToList());

        return MethodDeclaration(ParseTypeName(returnType), methodName)
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(parameters.ToArray())
            .WithBody(body);
    }

    private BlockSyntax GenerateMethodBody(
        FunctionDefinition function,
        List<ParameterSyntax> functionParams)
    {
        var statements = new List<StatementSyntax>();

        // Формируем SQL запрос с использованием Roslyn для строки
        var paramPlaceholders = functionParams
            .Select((_, i) => $"${i + 1}")
            .ToArray();
        
        var sqlQuery = $"SELECT * FROM {function.Schema}.{function.Name}({string.Join(", ", paramPlaceholders)})";

        // await using var cmd = new NpgsqlCommand(sql, connection);
        statements.Add(
            LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .AddVariables(
                        VariableDeclarator("cmd")
                            .WithInitializer(EqualsValueClause(
                                ObjectCreationExpression(ParseTypeName("NpgsqlCommand"))
                                    .AddArgumentListArguments(
                                        Argument(LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(sqlQuery))),
                                        Argument(IdentifierName("connection")))))))
            .WithAwaitKeyword(Token(SyntaxKind.AwaitKeyword))
            .WithUsingKeyword(Token(SyntaxKind.UsingKeyword)));

        // Добавляем параметры
        for (int i = 0; i < functionParams.Count; i++)
        {
            var paramName = functionParams[i].Identifier.Text;
            statements.Add(
                ExpressionStatement(
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
                            Literal($"${i + 1}"))),
                        Argument(IdentifierName(paramName)))));
        }

        // await cmd.ExecuteNonQueryAsync();
        statements.Add(
            ExpressionStatement(
                AwaitExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("cmd"),
                            IdentifierName("ExecuteNonQueryAsync"))))));

        return Block(statements);
    }

    private string MapReturnType(string? returnType)
    {
        if (string.IsNullOrEmpty(returnType) || returnType == "void")
            return "Task";

        // Для простоты пока возвращаем Task
        // В будущем можно добавить маппинг на конкретные типы
        return "Task";
    }
}
