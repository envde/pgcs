using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение Domain типа
/// </summary>
public sealed record DomainTypeDefinition : DefinitionBase
{
    /// <summary>
    /// Имя домена
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Базовый тип данных PostgreSQL, на котором основан домен
    /// </summary>
    public required string BaseType { get; init; }
    
    /// <summary>
    /// Значение по умолчанию для домена
    /// </summary>
    public string? DefaultValue { get; init; }
    
    /// <summary>
    /// Запрещено ли NULL значение (NOT NULL)
    /// </summary>
    public bool IsNotNull { get; init; }
    
    /// <summary>
    /// Список CHECK ограничений, применяемых к домену
    /// </summary>
    public IReadOnlyList<string> CheckConstraints { get; init; } = [];
}