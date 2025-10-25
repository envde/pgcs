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
public sealed class CustomTypeGenerator(SyntaxBuilder syntaxBuilder) : ICustomTypeGenerator
{
    public IReadOnlyList<GeneratedCode> Generate(
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

        return generatedCode;
    }

    /// <summary>
    /// Генерирует enum тип с использованием Roslyn
    /// </summary>
    private GeneratedCode GenerateEnumType(TypeDefinition type, SchemaGenerationOptions options)
    {
        // Конвертируем имя типа в PascalCase (user_status → UserStatus)
        var enumName = ConvertEnumValueToIdentifier(type.Name);
        
        // Создаем члены enum
        var members = new List<EnumMemberDeclarationSyntax>();
        
        foreach (var value in type.EnumValues)
        {
            var memberName = ConvertEnumValueToIdentifier(value);
            var member = EnumMemberDeclaration(memberName);
            
            if (options.GenerateXmlDocumentation)
            {
                member = member.WithLeadingTrivia(TriviaList(
                    Comment($"/// <summary>Значение '{value}'</summary>"),
                    CarriageReturnLineFeed));
            }
            
            members.Add(member);
        }

        // Создаем enum declaration с PascalCase именем
        var enumDeclaration = EnumDeclaration(enumName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(members.ToArray());

        if (options.GenerateXmlDocumentation && !string.IsNullOrWhiteSpace(type.Comment))
        {
            enumDeclaration = enumDeclaration.WithLeadingTrivia(
                TriviaList(
                    Comment("/// <summary>"),
                    Comment($"{Environment.NewLine}/// {type.Comment}{Environment.NewLine}"),
                    Comment("/// </summary>"),
                    CarriageReturnLineFeed));
        }

        // Создаем compilation unit
        var compilationUnit = syntaxBuilder.BuildEnumCompilationUnit(
            options.RootNamespace,
            enumDeclaration);

        var sourceCode = compilationUnit.ToFullString();

        return new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = enumName, // Используем PascalCase имя enum
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

        // Создаем compilation unit используя общий helper (domain type)
        var usings = new[] { "System" };
        var compilationUnit = RoslynSyntaxHelpers.BuildCompilationUnit(
            options.RootNamespace,
            recordDeclaration,
            usings);

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
        foreach (var attribute in type.CompositeAttributes)
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

        // Создаем compilation unit используя общий helper (composite type)
        var usings = new[] { "System" };
        var compilationUnit = RoslynSyntaxHelpers.BuildCompilationUnit(
            options.RootNamespace,
            recordDeclaration,
            usings);

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
        // Убираем недопустимые символы
        var cleaned = new string(value
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray());

        // Если начинается с цифры, добавляем префикс
        if (char.IsDigit(cleaned[0]))
        {
            cleaned = "Value_" + cleaned;
        }

        // Обрабатываем snake_case: user_status → UserStatus
        if (cleaned.Contains('_'))
        {
            var parts = cleaned.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(part =>
                char.ToUpperInvariant(part[0]) + (part.Length > 1 ? part[1..].ToLowerInvariant() : "")));
        }

        // Простое имя - делаем PascalCase
        return cleaned.Length > 0 ? char.ToUpperInvariant(cleaned[0]) + (cleaned.Length > 1 ? cleaned[1..] : "") : cleaned;
    }

    /// <summary>
    /// Создает XML комментарий
    /// </summary>
    private static SyntaxTriviaList CreateXmlComment(string comment)
    {
        return RoslynSyntaxHelpers.CreateXmlComment(comment, addTrailingNewLine: true);
    }
}
