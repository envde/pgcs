using PgCs.Core.SchemaAnalyzer.Definitions;
using PgCs.Core.Validation;

namespace PgCs.Core.SchemaAnalyzer.Metadata;

/// <summary>
/// Полные метаданные схемы базы данных
/// </summary>
public sealed record SchemaMetadata
{
    /// <summary>
    /// Список всех таблиц в схеме
    /// </summary>
    public required IReadOnlyList<TableDefinition> Tables { get; init; }
    
    /// <summary>
    /// Список всех представлений (VIEW) в схеме
    /// </summary>
    public required IReadOnlyList<ViewDefinition> Views { get; init; }
    
    /// <summary>
    /// Список всех Enums
    /// </summary>
    public required IReadOnlyList<EnumTypeDefinition> Enums { get; init; }
    
    /// <summary>
    /// Список всех Composite Types
    /// </summary>
    public required IReadOnlyList<CompositeTypeDefinition> Composites { get; init; }
    
    /// <summary>
    /// Список всех Domain Types
    /// </summary>
    public required IReadOnlyList<DomainTypeDefinition> Domains { get; init; }
    
    /// <summary>
    /// Список всех функций и процедур в схеме
    /// </summary>
    public required IReadOnlyList<FunctionDefinition> Functions { get; init; }
    
    /// <summary>
    /// Список всех индексов в схеме
    /// </summary>
    public required IReadOnlyList<IndexDefinition> Indexes { get; init; }
    
    /// <summary>
    /// Список всех триггеров в схеме
    /// </summary>
    public required IReadOnlyList<TriggerDefinition> Triggers { get; init; }
    
    /// <summary>
    /// Список всех ограничений целостности (constraints) в схеме
    /// </summary>
    public required IReadOnlyList<ConstraintDefinition> Constraints { get; init; }
    
    /// <summary>
    /// Словарь комментариев к объектам базы данных (ключ: имя объекта, значение: комментарий)
    /// </summary>
    public required IReadOnlyDictionary<string, string> Comments { get; init; }
    
    /// <summary>
    /// Warnings и errors собранные во время parsing schema
    /// </summary>
    public required IReadOnlyList<ValidationIssue> ValidationIssues { get; init; }
    
    /// <summary>
    /// Путь к исходному файлу или файлам.
    /// </summary>
    public required IReadOnlyList<string> SourcePaths { get; init; }

    /// <summary>
    /// Время анализа схемы (UTC)
    /// </summary>
    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;
}