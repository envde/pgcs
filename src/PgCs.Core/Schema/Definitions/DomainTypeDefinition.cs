using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение доменного типа PostgreSQL (CREATE DOMAIN)
/// Домен - это базовый тип с дополнительными ограничениями
/// </summary>
public sealed record DomainTypeDefinition : DefinitionBase
{
    /// <summary>
    /// Имя домена
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Базовый тип данных PostgreSQL, на котором основан домен (integer, varchar и т.д.)
    /// </summary>
    public required string BaseType { get; init; }
    
    /// <summary>
    /// Значение по умолчанию для домена (DEFAULT expression)
    /// </summary>
    public string? DefaultValue { get; init; }
    
    /// <summary>
    /// Запрещено ли NULL значение (NOT NULL constraint)
    /// </summary>
    public bool IsNotNull { get; init; }
    
    /// <summary>
    /// Список CHECK ограничений, применяемых к домену
    /// Каждое ограничение должно быть истинным для значений домена
    /// </summary>
    public IReadOnlyList<string> CheckConstraints { get; init; } = [];
    
    /// <summary>
    /// Максимальная длина для строковых доменов (VARCHAR(n))
    /// </summary>
    public int? MaxLength { get; init; }
    
    /// <summary>
    /// Точность для числовых доменов (NUMERIC(precision, scale))
    /// </summary>
    public int? NumericPrecision { get; init; }
    
    /// <summary>
    /// Масштаб для числовых доменов (NUMERIC(precision, scale))
    /// </summary>
    public int? NumericScale { get; init; }
    
    /// <summary>
    /// Collation (правило сортировки) для строковых доменов
    /// </summary>
    public string? Collation { get; init; }
}