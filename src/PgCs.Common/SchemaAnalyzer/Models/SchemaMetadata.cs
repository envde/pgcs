using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Views;

namespace PgCs.Common.SchemaAnalyzer.Models;

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
    /// Список всех пользовательских типов данных (ENUM, Composite, Domain, Range)
    /// </summary>
    public required IReadOnlyList<TypeDefinition> Types { get; init; }
    
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
    public IReadOnlyDictionary<string, string>? Comments { get; init; }
    
    /// <summary>
    /// Warnings и errors собранные во время parsing schema
    /// </summary>
    public IReadOnlyList<ValidationIssue>? ValidationIssues { get; init; }
    
    /// <summary>
    /// Путь к исходному файлу схемы (если анализировался файл или директория)
    /// </summary>
    public string? SourceFile { get; init; }
    
    /// <summary>
    /// Время анализа схемы (UTC)
    /// </summary>
    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;
}