using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Tables;

namespace PgCs.Common.SchemaAnalyzer.Models.Functions;

/// <summary>
/// Определение функции или процедуры
/// </summary>
public sealed record FunctionDefinition
{
    public required string Name { get; init; }
    public string? Schema { get; init; }
    public required IReadOnlyList<FunctionParameter> Parameters { get; init; }
    public string? ReturnType { get; init; }
    public bool ReturnsTable { get; init; }
    public IReadOnlyList<ColumnDefinition>? ReturnTableColumns { get; init; }
    public required string Language { get; init; }
    public required string Body { get; init; }
    public FunctionVolatility Volatility { get; init; } = FunctionVolatility.Volatile;
    public bool IsAggregate { get; init; }
    public bool IsTrigger { get; init; }
    public string? Comment { get; init; }
    public string? RawSql { get; init; }
}