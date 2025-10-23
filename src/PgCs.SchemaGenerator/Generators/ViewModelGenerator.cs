using System.Text;
using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.SchemaGenerator.Services;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# моделей для представлений PostgreSQL
/// </summary>
public sealed class ViewModelGenerator : IViewModelGenerator
{
    private readonly SyntaxBuilder _syntaxBuilder;

    public ViewModelGenerator(SyntaxBuilder syntaxBuilder)
    {
        _syntaxBuilder = syntaxBuilder;
    }

    public async ValueTask<IReadOnlyList<GeneratedCode>> GenerateAsync(
        IReadOnlyList<ViewDefinition> views,
        SchemaGenerationOptions options)
    {
        var code = new List<GeneratedCode>();

        foreach (var view in views)
        {
            var generatedCode = await GenerateViewCodeAsync(view, options);
            code.Add(generatedCode);
        }

        return code;
    }

    private ValueTask<GeneratedCode> GenerateViewCodeAsync(
        ViewDefinition view,
        SchemaGenerationOptions options)
    {
        // Конвертируем ViewDefinition в TableDefinition для переиспользования логики
        var tableDefinition = new TableDefinition
        {
            Name = view.Name,
            Schema = view.Schema,
            Columns = view.Columns,
            Comment = view.Comment,
            Constraints = [],
            Indexes = []
        };

        // Строим класс
        var classDeclaration = _syntaxBuilder.BuildTableClass(tableDefinition, options);

        // Собираем необходимые usings
        var usings = _syntaxBuilder.GetRequiredUsings(view.Columns);

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
        return ValueTask.FromResult(new GeneratedCode
        {
            SourceCode = sourceCode,
            TypeName = className,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.ViewModel
        });
    }
}
