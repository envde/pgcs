using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.Services;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PgCs.QueryGenerator.Services;

/// <summary>
/// Строитель синтаксических деревьев C# для методов запросов с использованием Roslyn
/// </summary>
public sealed class QuerySyntaxBuilder
{
    private readonly ITypeMapper _typeMapper;
    private readonly INameConverter _nameConverter;

    public QuerySyntaxBuilder(ITypeMapper typeMapper, INameConverter nameConverter)
    {
        _typeMapper = typeMapper;
        _nameConverter = nameConverter;
    }

    /// <summary>
    /// Создает compilation unit для модели результата
    /// </summary>
    public CompilationUnitSyntax BuildResultModelCompilationUnit(
        string namespaceName,
        string modelName,
        IReadOnlyList<ReturnColumn> columns)
    {
        var classDeclaration = BuildResultModelClass(modelName, columns);
        var usings = GetRequiredUsingsForColumns(columns);

        return BuildCompilationUnit(namespaceName, classDeclaration, usings);
    }

    /// <summary>
    /// Создает класс модели результата
    /// </summary>
    public ClassDeclarationSyntax BuildResultModelClass(
        string modelName,
        IReadOnlyList<ReturnColumn> columns)
    {
        var classDeclaration = ClassDeclaration(modelName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddModifiers(Token(SyntaxKind.SealedKeyword))
            .AddModifiers(Token(SyntaxKind.RecordKeyword));

        // Добавляем свойства для каждой колонки
        foreach (var column in columns)
        {
            var property = BuildProperty(column);
            classDeclaration = classDeclaration.AddMembers(property);
        }

        return classDeclaration;
    }

    /// <summary>
    /// Создает свойство для колонки результата
    /// </summary>
    public PropertyDeclarationSyntax BuildProperty(ReturnColumn column)
    {
        var propertyName = _nameConverter.ToPropertyName(column.Name);
        var propertyType = column.CSharpType;

        var property = PropertyDeclaration(
                ParseTypeName(propertyType),
                Identifier(propertyName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddModifiers(Token(SyntaxKind.RequiredKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        return property;
    }

    /// <summary>
    /// Создает класс модели параметров
    /// </summary>
    public ClassDeclarationSyntax BuildParameterModelClass(
        string modelName,
        IReadOnlyList<QueryParameter> parameters)
    {
        var classDeclaration = ClassDeclaration(modelName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddModifiers(Token(SyntaxKind.SealedKeyword))
            .AddModifiers(Token(SyntaxKind.RecordKeyword));

        // Добавляем свойства для каждого параметра
        foreach (var parameter in parameters)
        {
            var property = BuildParameterProperty(parameter);
            classDeclaration = classDeclaration.AddMembers(property);
        }

        return classDeclaration;
    }

    /// <summary>
    /// Создает свойство для параметра
    /// </summary>
    private PropertyDeclarationSyntax BuildParameterProperty(QueryParameter parameter)
    {
        var propertyName = _nameConverter.ToPropertyName(parameter.Name);
        var propertyType = parameter.CSharpType;

        var property = PropertyDeclaration(
                ParseTypeName(propertyType),
                Identifier(propertyName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddModifiers(Token(SyntaxKind.RequiredKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        return property;
    }

    /// <summary>
    /// Создает интерфейс репозитория
    /// </summary>
    public InterfaceDeclarationSyntax BuildRepositoryInterface(
        string interfaceName,
        IReadOnlyList<MethodDeclarationSyntax> methods)
    {
        var interfaceDeclaration = InterfaceDeclaration(interfaceName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword));

        foreach (var method in methods)
        {
            // Преобразуем метод в сигнатуру интерфейса (без тела)
            var interfaceMethod = method
                .WithBody(null)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            interfaceDeclaration = interfaceDeclaration.AddMembers(interfaceMethod);
        }

        return interfaceDeclaration;
    }

    /// <summary>
    /// Создает класс репозитория
    /// </summary>
    public ClassDeclarationSyntax BuildRepositoryClass(
        string className,
        string? interfaceName,
        IReadOnlyList<MethodDeclarationSyntax> methods)
    {
        var classDeclaration = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddModifiers(Token(SyntaxKind.SealedKeyword));

        // Добавляем реализацию интерфейса
        if (interfaceName != null)
        {
            classDeclaration = classDeclaration.AddBaseListTypes(
                SimpleBaseType(IdentifierName(interfaceName)));
        }

        // Добавляем поле для хранения connection string или data source
        var connectionField = FieldDeclaration(
            VariableDeclaration(ParseTypeName("NpgsqlDataSource"))
                .AddVariables(VariableDeclarator("_dataSource")))
            .AddModifiers(
                Token(SyntaxKind.PrivateKeyword),
                Token(SyntaxKind.ReadOnlyKeyword));

        classDeclaration = classDeclaration.AddMembers(connectionField);

        // Добавляем конструктор
        var constructor = BuildRepositoryConstructor(className);
        classDeclaration = classDeclaration.AddMembers(constructor);

        // Добавляем методы
        foreach (var method in methods)
        {
            classDeclaration = classDeclaration.AddMembers(method);
        }

        return classDeclaration;
    }

    /// <summary>
    /// Создает конструктор репозитория
    /// </summary>
    private ConstructorDeclarationSyntax BuildRepositoryConstructor(string className)
    {
        return ConstructorDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("dataSource"))
                    .WithType(ParseTypeName("NpgsqlDataSource")))
            .WithBody(Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("_dataSource"),
                        IdentifierName("dataSource")))));
    }

    /// <summary>
    /// Создает compilation unit с namespace
    /// </summary>
    private CompilationUnitSyntax BuildCompilationUnit(
        string namespaceName,
        MemberDeclarationSyntax memberDeclaration,
        IEnumerable<string> usings)
    {
        var usingDirectives = usings
            .Distinct()
            .OrderBy(u => u)
            .Select(u => UsingDirective(IdentifierName(u)))
            .ToArray();

        var fileScopedNamespace = FileScopedNamespaceDeclaration(IdentifierName(namespaceName))
            .AddMembers(memberDeclaration);

        return CompilationUnit()
            .AddUsings(usingDirectives)
            .AddMembers(fileScopedNamespace)
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Собирает список required usings для колонок
    /// </summary>
    private IEnumerable<string> GetRequiredUsingsForColumns(IEnumerable<ReturnColumn> columns)
    {
        var usings = new HashSet<string> { "System" };

        foreach (var column in columns)
        {
            var requiredNamespace = _typeMapper.GetRequiredNamespace(column.PostgresType);
            if (requiredNamespace != null)
            {
                usings.Add(requiredNamespace);
            }
        }

        return usings;
    }

    /// <summary>
    /// Создает XML комментарий для документации
    /// </summary>
    public SyntaxTriviaList CreateXmlComment(string summary, string? sql = null)
    {
        var triviaList = TriviaList();

        triviaList = triviaList.Add(Comment("/// <summary>"));
        triviaList = triviaList.Add(Comment($"/// {summary}"));
        triviaList = triviaList.Add(Comment("/// </summary>"));

        if (sql != null)
        {
            triviaList = triviaList.Add(Comment("/// <remarks>"));
            triviaList = triviaList.Add(Comment("/// SQL Query:"));
            foreach (var line in sql.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                triviaList = triviaList.Add(Comment($"/// {line.Trim()}"));
            }
            triviaList = triviaList.Add(Comment("/// </remarks>"));
        }

        triviaList = triviaList.Add(CarriageReturnLineFeed);

        return triviaList;
    }
}
