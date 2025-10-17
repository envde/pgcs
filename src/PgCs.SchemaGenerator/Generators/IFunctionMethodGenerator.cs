using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
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
    ValueTask<IReadOnlyList<GeneratedCode>> GenerateAsync(
        IReadOnlyList<FunctionDefinition> functions,
        SchemaGenerationOptions options);
}
