using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.SchemaGenerator.Services;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# моделей для представлений PostgreSQL
/// </summary>
public sealed class ViewModelGenerator(SyntaxBuilder syntaxBuilder) : IViewModelGenerator
{
    public IReadOnlyList<GeneratedCode> Generate(IReadOnlyList<ViewDefinition> views, SchemaGenerationOptions options)
    {
        var code = new List<GeneratedCode>();

        foreach (var view in views)
        {
            var generatedCode = GenerateViewCode(view, options);
            code.Add(generatedCode);
        }

        return code;
    }

    private GeneratedCode GenerateViewCode( ViewDefinition view, SchemaGenerationOptions options)
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
        var classDeclaration = syntaxBuilder.BuildTableClass(tableDefinition, options);

        // Собираем необходимые usings
        var usings = syntaxBuilder.GetRequiredUsings(view.Columns);

        // Создаем compilation unit
        var compilationUnit = syntaxBuilder.BuildCompilationUnit(
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
            CodeType = GeneratedFileType.ViewModel
        };
    }
}
