using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaGenerator.Models.Options;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# моделей для таблиц PostgreSQL
/// </summary>
public interface ITableModelGenerator
{
    /// <summary>
    /// Генерирует C# классы на основе определений таблиц
    /// </summary>
    IReadOnlyList<GeneratedCode> Generate( IReadOnlyList<TableDefinition> tables, SchemaGenerationOptions options);
}
