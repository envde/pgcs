using System.Text;
using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.SchemaGenerator.Services;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# типов для пользовательских типов PostgreSQL
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
        var code = new List<GeneratedCode>();
        var enumCount = 0;
        var domainCount = 0;
        var compositeCount = 0;

        foreach (var type in types)
        {
            switch (type.Kind)
            {
                case TypeKind.Enum:
                    var enumCode = await GenerateEnumCodeAsync(type, options);
                    code.Add(enumCode);
                    enumCount++;
                    break;

                case TypeKind.Composite:
                    var compositeCode = await GenerateCompositeCodeAsync(type, options);
                    code.Add(compositeCode);
                    compositeCount++;
                    break;

                case TypeKind.Domain:
                    // Domain типы пока пропускаем, они обычно маппятся на базовые типы
                    domainCount++;
                    break;
            }
        }

        return code;
    }

    private async ValueTask<GeneratedCode> GenerateEnumCodeAsync(
        TypeDefinition enumType,
        SchemaGenerationOptions options)
    {
        // Строим enum
        var enumDeclaration = _syntaxBuilder.BuildEnum(enumType);

        // Создаем compilation unit
        var compilationUnit = _syntaxBuilder.BuildEnumCompilationUnit(
            options.RootNamespace,
            enumDeclaration);

        // Генерируем исходный код
        var sourceCode = compilationUnit.ToFullString();

        // Определяем имя enum
        var enumName = enumDeclaration.Identifier.Text;

        return new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = enumName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.EnumType
        };
    }

    private async ValueTask<GeneratedCode> GenerateCompositeCodeAsync(
        TypeDefinition compositeType,
        SchemaGenerationOptions options)
    {
        // Конвертируем composite type в TableDefinition для переиспользования логики
        var columns = compositeType.CompositeAttributes?.Select(attr => new ColumnDefinition
        {
            Name = attr.Name,
            DataType = attr.DataType,
            IsNullable = true, // Composite атрибуты всегда nullable
            IsPrimaryKey = false,
            IsUnique = false,
            IsArray = false
        }).ToList() ?? new List<ColumnDefinition>();

        var tableDefinition = new TableDefinition
        {
            Name = compositeType.Name,
            Schema = compositeType.Schema,
            Columns = columns,
            Comment = compositeType.Comment,
            Constraints = [],
            Indexes = []
        };

        // Строим класс
        var classDeclaration = _syntaxBuilder.BuildTableClass(tableDefinition, options);

        // Собираем необходимые usings
        var usings = _syntaxBuilder.GetRequiredUsings(columns);

        // Создаем compilation unit
        var compilationUnit = _syntaxBuilder.BuildCompilationUnit(
            options.RootNamespace,
            classDeclaration,
            usings);

        // Генерируем исходный код
        var sourceCode = compilationUnit.ToFullString();

        // Определяем имя файла и путь
        var className = classDeclaration.Identifier.Text;
        var fileName = $"{className}.cs";
        var relativePath = options.FileOrganization switch
        {
            FileOrganization.BySchema => Path.Combine(
                "Types",
                compositeType.Schema ?? "public",
                fileName),
            FileOrganization.ByType => Path.Combine(
                "Types",
                fileName),
            FileOrganization.Flat => fileName,
            _ => fileName
        };

        return new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = className,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.CompositeType
        };
    }
}
