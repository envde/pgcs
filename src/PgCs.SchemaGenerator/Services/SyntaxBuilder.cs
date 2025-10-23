using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.Common.Services;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PgCs.SchemaGenerator.Services;

/// <summary>
/// Строитель синтаксических деревьев C# с использованием Roslyn
/// </summary>
public sealed class SyntaxBuilder
{
    private readonly ITypeMapper _typeMapper;
    private readonly INameConverter _nameConverter;

    public SyntaxBuilder(ITypeMapper typeMapper, INameConverter nameConverter)
    {
        _typeMapper = typeMapper;
        _nameConverter = nameConverter;
    }

    /// <summary>
    /// Создает compilation unit (файл) с namespace и классом
    /// </summary>
    public CompilationUnitSyntax BuildCompilationUnit(
        string namespaceName,
        ClassDeclarationSyntax classDeclaration,
        IEnumerable<string> usings)
    {
        // Создаем using директивы
        var usingDirectives = usings
            .Distinct()
            .OrderBy(u => u)
            .Select(u => UsingDirective(IdentifierName(u)))
            .ToArray();

        // Создаем file-scoped namespace (C# 10+)
        var fileScopedNamespace = FileScopedNamespaceDeclaration(IdentifierName(namespaceName))
            .AddMembers(classDeclaration);

        return CompilationUnit()
            .AddUsings(usingDirectives)
            .AddMembers(fileScopedNamespace)
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Создает класс для таблицы
    /// </summary>
    public ClassDeclarationSyntax BuildTableClass(
        TableDefinition table,
        SchemaGenerationOptions options)
    {
        var className = _nameConverter.ToClassName(table.Name);

        var classDeclaration = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddModifiers(Token(SyntaxKind.SealedKeyword))
            .AddModifiers(Token(SyntaxKind.RecordKeyword));

        // Добавляем XML комментарий
        if (!string.IsNullOrWhiteSpace(table.Comment))
        {
            classDeclaration = classDeclaration.WithLeadingTrivia(
                CreateXmlComment(table.Comment));
        }

        // Добавляем свойства для каждой колонки
        foreach (var column in table.Columns)
        {
            var property = BuildProperty(column);
            classDeclaration = classDeclaration.AddMembers(property);
        }

        return classDeclaration;
    }

    /// <summary>
    /// Создает свойство для колонки
    /// </summary>
    public PropertyDeclarationSyntax BuildProperty(ColumnDefinition column)
    {
        var propertyName = _nameConverter.ToPropertyName(column.Name);
        var propertyType = _typeMapper.MapType(column.DataType, column.IsNullable, column.IsArray);

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

        // Добавляем XML комментарий
        if (!string.IsNullOrWhiteSpace(column.Comment))
        {
            property = property.WithLeadingTrivia(CreateXmlComment(column.Comment));
        }

        return property;
    }

    /// <summary>
    /// Создает enum для PostgreSQL enum типа
    /// </summary>
    public EnumDeclarationSyntax BuildEnum(TypeDefinition enumType)
    {
        var enumName = _nameConverter.ToClassName(enumType.Name);

        var enumDeclaration = EnumDeclaration(enumName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword));

        // Добавляем XML комментарий
        if (!string.IsNullOrWhiteSpace(enumType.Comment))
        {
            enumDeclaration = enumDeclaration.WithLeadingTrivia(
                CreateXmlComment(enumType.Comment));
        }

        // Добавляем значения enum
        if (enumType.EnumValues != null)
        {
            foreach (var value in enumType.EnumValues)
            {
                var memberName = _nameConverter.ToEnumMemberName(value);
                var member = EnumMemberDeclaration(Identifier(memberName));
                enumDeclaration = enumDeclaration.AddMembers(member);
            }
        }

        return enumDeclaration;
    }

    /// <summary>
    /// Создает compilation unit для enum
    /// </summary>
    public CompilationUnitSyntax BuildEnumCompilationUnit(
        string namespaceName,
        EnumDeclarationSyntax enumDeclaration)
    {
        var fileScopedNamespace = FileScopedNamespaceDeclaration(IdentifierName(namespaceName))
            .AddMembers(enumDeclaration);

        return CompilationUnit()
            .AddMembers(fileScopedNamespace)
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Создает XML комментарий для документации используя Roslyn XML API
    /// </summary>
    private static SyntaxTriviaList CreateXmlComment(string comment)
    {
        var lines = comment.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var summaryContent = new List<XmlNodeSyntax>();

        foreach (var line in lines)
        {
            summaryContent.Add(XmlText(XmlTextLiteral(line.Trim())));
            if (line != lines.Last())
            {
                summaryContent.Add(XmlText(XmlTextNewLine(Environment.NewLine, continueXmlDocumentationComment: false)));
            }
        }

        // Создаем DocumentationComment с <summary> элементом
        return TriviaList(
            Trivia(
                DocumentationComment(
                    XmlSummaryElement(summaryContent.ToArray()))));
    }

    /// <summary>
    /// Собирает список required usings для типов колонок
    /// </summary>
    public IEnumerable<string> GetRequiredUsings(IEnumerable<ColumnDefinition> columns)
    {
        var usings = new HashSet<string>();

        foreach (var column in columns)
        {
            var requiredNamespace = _typeMapper.GetRequiredNamespace(column.DataType);
            if (requiredNamespace != null)
            {
                usings.Add(requiredNamespace);
            }
        }

        return usings;
    }
}
