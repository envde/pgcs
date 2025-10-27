namespace PgCs.Core.Schema.Common;

/// <summary>
/// Информация о типе DOMAIN
/// </summary>
public sealed record DomainTypeInfo
{
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