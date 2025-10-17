using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaGenerator.Models.Options;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# типов для пользовательских типов PostgreSQL (enum, domain, composite)
/// </summary>
public interface ICustomTypeGenerator
{
    /// <summary>
    /// Генерирует C# типы на основе определений пользовательских типов
    /// </summary>
    ValueTask<IReadOnlyList<GeneratedCode>> GenerateAsync(
        IReadOnlyList<TypeDefinition> types,
        SchemaGenerationOptions options);
}
