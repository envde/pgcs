using System.Text;
using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.SchemaGenerator.Services;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# моделей для таблиц PostgreSQL
/// </summary>
public sealed class TableModelGenerator : ITableModelGenerator
{
    private readonly SyntaxBuilder _syntaxBuilder;

    public TableModelGenerator(SyntaxBuilder syntaxBuilder)
    {
        _syntaxBuilder = syntaxBuilder;
    }

    public async ValueTask<IReadOnlyList<GeneratedCode>> GenerateAsync(
        IReadOnlyList<TableDefinition> tables,
        SchemaGenerationOptions options)
    {
        var code = new List<GeneratedCode>();

        foreach (var table in tables)
        {
            var generatedCode = await GenerateTableCodeAsync(table, options);
            code.Add(generatedCode);
        }

        return code;
    }

    private async ValueTask<GeneratedCode> GenerateTableCodeAsync(
        TableDefinition table,
        SchemaGenerationOptions options)
    {
        // Строим класс
        var classDeclaration = _syntaxBuilder.BuildTableClass(table, options);

        // Собираем необходимые usings
        var usings = _syntaxBuilder.GetRequiredUsings(table.Columns);

        // Создаем compilation unit
        var compilationUnit = _syntaxBuilder.BuildCompilationUnit(
            options.RootNamespace,
            classDeclaration,
            usings);

        // Генерируем исходный код
        var sourceCode = compilationUnit.ToFullString();

        // Определяем имя класса
        var className = classDeclaration.Identifier.Text;

        // Создаем GeneratedCode
        return new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = className,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.TableModel
        };
    }
}
