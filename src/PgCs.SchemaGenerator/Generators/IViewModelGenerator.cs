using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaGenerator.Models.Options;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# моделей для представлений PostgreSQL
/// </summary>
public interface IViewModelGenerator
{
    /// <summary>
    /// Генерирует C# классы на основе определений представлений
    /// </summary>
    IReadOnlyList<GeneratedCode> Generate( IReadOnlyList<ViewDefinition> views, SchemaGenerationOptions options);
}
