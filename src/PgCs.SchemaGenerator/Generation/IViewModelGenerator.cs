using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Generation;

/// <summary>
/// Интерфейс генератора моделей представлений
/// </summary>
internal interface IViewModelGenerator
{
    /// <summary>
    /// Генерирует модель для представления
    /// </summary>
    GeneratedModel Generate(ViewDefinition view, SchemaGenerationOptions options);
}
