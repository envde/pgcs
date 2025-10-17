using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Generation;

/// <summary>
/// Интерфейс генератора моделей типов
/// </summary>
internal interface ITypeModelGenerator
{
    /// <summary>
    /// Генерирует модель для пользовательского типа
    /// </summary>
    GeneratedModel Generate(TypeDefinition type, SchemaGenerationOptions options);
}
