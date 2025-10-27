namespace PgCs.Core.Schema.Common;

/// <summary>
/// Базовый класс для определений типов данных PostgreSQL
/// </summary>
public abstract record DefinitionBase
{
    public required string Name { get; init; }
    public string? Schema { get; init; }
    public string? Comment { get; init; }
    public string? RawSql { get; init; }
}