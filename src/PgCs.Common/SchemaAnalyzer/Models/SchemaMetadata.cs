namespace PgCs.Common.SchemaAnalyzer.Models;

/// <summary>
/// Полные метаданные схемы базы данных
/// </summary>
public sealed record SchemaMetadata
{
    public required IReadOnlyList<TableDefinition> Tables { get; init; }
    public required IReadOnlyList<ViewDefinition> Views { get; init; }
    public required IReadOnlyList<TypeDefinition> Types { get; init; }
    public required IReadOnlyList<FunctionDefinition> Functions { get; init; }
    public required IReadOnlyList<IndexDefinition> Indexes { get; init; }
    public required IReadOnlyList<TriggerDefinition> Triggers { get; init; }
    public required IReadOnlyList<ConstraintDefinition> Constraints { get; init; }
    public IReadOnlyDictionary<string, string>? Comments { get; init; }
    public string? SourceFile { get; init; }
    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;
}