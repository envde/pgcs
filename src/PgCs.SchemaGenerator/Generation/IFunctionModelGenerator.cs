using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Generation;

/// <summary>
/// Интерфейс генератора моделей параметров функций
/// </summary>
internal interface IFunctionModelGenerator
{
    /// <summary>
    /// Генерирует модель параметров для функции
    /// </summary>
    GeneratedModel? Generate(FunctionDefinition function, SchemaGenerationOptions options);
}
