using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaGenerator.Models.Options;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# методов для функций PostgreSQL
/// </summary>
public interface IFunctionMethodGenerator
{
    /// <summary>
    /// Генерирует C# методы на основе определений функций
    /// </summary>
    IReadOnlyList<GeneratedCode> Generate( IReadOnlyList<FunctionDefinition> functions, SchemaGenerationOptions options);
}
