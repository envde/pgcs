
namespace PgCs.Common.SchemaAnalyzer.Models;

/// <summary>
/// Информация о типе DOMAIN
/// </summary>
public sealed record DomainTypeInfo
{
    public required string BaseType { get; init; }
    public string? DefaultValue { get; init; }
    public bool IsNotNull { get; init; }
    public IReadOnlyList<string> CheckConstraints { get; init; } = [];
}