using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;

namespace PgCs.Core.Schema.Analyzer;

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
    /// Список всех партиций в схеме
    /// </summary>
    public required IReadOnlyList<PartitionDefinition> Partitions { get; init; }
    
    /// <summary>
    /// Комментарии для всех композитных типов
    /// </summary>
    public required IReadOnlyList<CompositeTypeCommentDefinition> CompositeTypeComments { get; init; }
    
    /// <summary>
    /// Комментарии к таблицам
    /// </summary>
    public required IReadOnlyList<TableCommentDefinition> TableComments { get; init; }
    
    /// <summary>
    /// Комментарии к колонкам таблиц
    /// </summary>
    public required IReadOnlyList<ColumnCommentDefinition> ColumnComments { get; init; }
    
    /// <summary>
    /// Комментарии для индексов
    /// </summary>
    public required IReadOnlyList<IndexCommentDefinition> IndexComments { get; init; }
    
    /// <summary>
    /// Комментарии к триггерам
    /// </summary>
    public required IReadOnlyList<TriggerCommentDefinition> TriggerComments { get; init; }
    
    /// <summary>
    /// Комментарии к функциям
    /// </summary>
    public required IReadOnlyList<FunctionCommentDefinition> FunctionComments { get; init; }
    
    /// <summary>
    /// Комментарии к ограничениям целостности
    /// </summary>
    public required IReadOnlyList<ConstraintCommentDefinition> ConstraintComments { get; init; }
    
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