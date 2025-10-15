namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Параметр функции
/// </summary>
public sealed record FunctionParameter
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
    public ParameterMode Mode { get; init; } = ParameterMode.In;
    public string? DefaultValue { get; init; }
}