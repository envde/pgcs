using Microsoft.CodeAnalysis.CSharp;
using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.SchemaGenerator.Services;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# моделей для таблиц PostgreSQL с использованием Roslyn
/// </summary>
public sealed class TableModelGenerator(SyntaxBuilder syntaxBuilder) : ITableModelGenerator
{
    public IReadOnlyList<GeneratedCode> Generate( IReadOnlyList<TableDefinition> tables, SchemaGenerationOptions options)
    {
        var code = new List<GeneratedCode>();

        foreach (var table in tables)
        {
            var generatedCode = GenerateTableCode(table, options);
            code.Add(generatedCode);
        }

        return code;
    }

    private GeneratedCode GenerateTableCode( TableDefinition table, SchemaGenerationOptions options)
    {
        // Используем SyntaxBuilder (Roslyn) для построения класса
        var classDeclaration = syntaxBuilder.BuildTableClass(table, options);

        // Получаем необходимые using директивы
        var usings = GetRequiredUsings(table, options);

        // Создаём compilation unit
        var compilationUnit = syntaxBuilder.BuildCompilationUnit(
            options.RootNamespace,
            classDeclaration,
            usings);

        var syntaxTree = CSharpSyntaxTree.Create(compilationUnit);
        var sourceCode = syntaxTree.ToString();

        var code = new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = table.Name,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.TableModel
        };

        return code;
    }

    private static IEnumerable<string> GetRequiredUsings(
        TableDefinition table,
        SchemaGenerationOptions options)
    {
        var usings = new HashSet<string> { "System" };

        // Проверяем типы колонок для определения необходимых using
        foreach (var column in table.Columns)
        {
            if (column.DataType.Contains("DateTime"))
                usings.Add("System");
            if (column.DataType.Contains("Guid"))
                usings.Add("System");
            if (column.DataType.Contains("List"))
                usings.Add("System.Collections.Generic");
        }

        if (options.GenerateValidationAttributes)
        {
            usings.Add("System.ComponentModel.DataAnnotations");
            usings.Add("System.ComponentModel.DataAnnotations.Schema");
        }

        return usings;
    }
}
