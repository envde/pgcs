using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.SchemaGenerator.Services;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SchemaTypeKind = PgCs.Common.SchemaAnalyzer.Models.Types.TypeKind;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# типов для пользовательских типов PostgreSQL с использованием Roslyn
/// </summary>
public sealed class CustomTypeGenerator : ICustomTypeGenerator
{
    private readonly SyntaxBuilder _syntaxBuilder;

    public CustomTypeGenerator(SyntaxBuilder syntaxBuilder)
    {
        _syntaxBuilder = syntaxBuilder;
    }

    public async ValueTask<IReadOnlyList<GeneratedCode>> GenerateAsync(
        IReadOnlyList<TypeDefinition> types,
        SchemaGenerationOptions options)
    {
        var generatedCode = new List<GeneratedCode>();

        foreach (var type in types)
        {
            var code = type.Kind switch
            {
                SchemaTypeKind.Enum => GenerateEnumType(type, options),
                SchemaTypeKind.Domain => GenerateDomainType(type, options),
                SchemaTypeKind.Composite => GenerateCompositeType(type, options),
                _ => null
            };

            if (code != null)
            {
                generatedCode.Add(code);
            }
        }

        return await ValueTask.FromResult(generatedCode);
    }

    /// <summary>
    /// Генерирует enum тип с использованием Roslyn
    /// </summary>
    private GeneratedCode GenerateEnumType(TypeDefinition type, SchemaGenerationOptions options)
    {
        // Создаем члены enum
        var members = new List<EnumMemberDeclarationSyntax>();
        
        foreach (var value in type.EnumValues ?? [])
        {
            var memberName = ConvertEnumValueToIdentifier(value);
            var member = EnumMemberDeclaration(memberName);
            
            if (options.GenerateXmlDocumentation)
            {
                member = member.WithLeadingTrivia(
                    CreateXmlComment($"Значение '{value}'"));
            }
            
            members.Add(member);
        }

        // Создаем enum declaration
        var enumDeclaration = EnumDeclaration(type.Name)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(members.ToArray());

        if (options.GenerateXmlDocumentation && !string.IsNullOrWhiteSpace(type.Comment))
        {
            enumDeclaration = enumDeclaration.WithLeadingTrivia(
                CreateXmlComment(type.Comment));
        }

        // Создаем compilation unit
        var compilationUnit = _syntaxBuilder.BuildEnumCompilationUnit(
            options.RootNamespace,
            enumDeclaration);

        var sourceCode = compilationUnit.NormalizeWhitespace().ToFullString();

        return new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = type.Name,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.EnumType
        };
    }

    /// <summary>
    /// Генерирует domain тип как type alias с использованием Roslyn
    /// </summary>
    private GeneratedCode GenerateDomainType(TypeDefinition type, SchemaGenerationOptions options)
    {
        // Domain типы генерируем как record с единственным свойством Value
        var baseTypeName = type.DomainInfo?.BaseType ?? "string";
        
        var recordDeclaration = RecordDeclaration(
                Token(SyntaxKind.RecordKeyword),
                type.Name)
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));

        // Добавляем свойство Value
        var valueProperty = PropertyDeclaration(
                ParseTypeName(baseTypeName),
                "Value")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddModifiers(Token(SyntaxKind.RequiredKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        recordDeclaration = recordDeclaration.AddMembers(valueProperty);

        if (options.GenerateXmlDocumentation && !string.IsNullOrWhiteSpace(type.Comment))
        {
            recordDeclaration = recordDeclaration.WithLeadingTrivia(
                CreateXmlComment(type.Comment));
        }

        // Создаем compilation unit
        var compilationUnit = CompilationUnit()
            .AddMembers(
                FileScopedNamespaceDeclaration(IdentifierName(options.RootNamespace))
                    .AddMembers(recordDeclaration))
            .NormalizeWhitespace();

        var sourceCode = compilationUnit.ToFullString();

        return new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = type.Name,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.DomainType
        };
    }

    /// <summary>
    /// Генерирует composite тип как record с использованием Roslyn
    /// </summary>
    private GeneratedCode GenerateCompositeType(TypeDefinition type, SchemaGenerationOptions options)
    {
        var recordDeclaration = RecordDeclaration(
                Token(SyntaxKind.RecordKeyword),
                type.Name)
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));

        // Добавляем свойства для каждого атрибута composite типа
        foreach (var attribute in type.CompositeAttributes ?? [])
        {
            var propertyType = attribute.DataType;
            var property = PropertyDeclaration(
                    ParseTypeName(propertyType),
                    attribute.Name)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddModifiers(Token(SyntaxKind.RequiredKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

            recordDeclaration = recordDeclaration.AddMembers(property);
        }

        if (options.GenerateXmlDocumentation && !string.IsNullOrWhiteSpace(type.Comment))
        {
            recordDeclaration = recordDeclaration.WithLeadingTrivia(
                CreateXmlComment(type.Comment));
        }

        // Создаем compilation unit
        var compilationUnit = CompilationUnit()
            .AddMembers(
                FileScopedNamespaceDeclaration(IdentifierName(options.RootNamespace))
                    .AddMembers(recordDeclaration))
            .NormalizeWhitespace();

        var sourceCode = compilationUnit.ToFullString();

        return new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = type.Name,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.CompositeType
        };
    }

    /// <summary>
    /// Конвертирует значение enum в валидный C# идентификатор
    /// </summary>
    private static string ConvertEnumValueToIdentifier(string value)
    {
        // Убираем недопустимые символы и делаем PascalCase
        var identifier = new string(value
            .Select((c, i) => i == 0 ? char.ToUpper(c) : c)
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray());

        // Если начинается с цифры, добавляем префикс
        if (char.IsDigit(identifier[0]))
        {
            identifier = "Value_" + identifier;
        }

        return identifier;
    }

    /// <summary>
    /// Создает XML комментарий
    /// </summary>
    private static SyntaxTriviaList CreateXmlComment(string comment)
    {
        var lines = comment.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var triviaList = TriviaList();

        triviaList = triviaList.Add(Comment("/// <summary>"));
        foreach (var line in lines)
        {
            triviaList = triviaList.Add(Comment($"/// {line.Trim()}"));
        }
        triviaList = triviaList.Add(Comment("/// </summary>"));
        triviaList = triviaList.Add(CarriageReturnLineFeed);

        return triviaList;
    }
}
