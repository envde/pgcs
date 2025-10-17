using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;

namespace PgCs.SchemaGenerator.Services;

/// <summary>
/// Валидатор схемы PostgreSQL перед генерацией
/// </summary>
public interface ISchemaValidator
{
    /// <summary>
    /// Проверяет схему на валидность и наличие проблем
    /// </summary>
    IReadOnlyList<ValidationIssue> Validate(SchemaMetadata schemaMetadata);
}
