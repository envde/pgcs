namespace PgCs.Core.Schema.Common;

/// <summary>
/// Атрибут (поле) композитного типа PostgreSQL
/// Композитный тип состоит из нескольких именованных атрибутов
/// </summary>
public sealed record CompositeTypeAttribute
{
    /// <summary>
    /// Имя атрибута (поля) композитного типа
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Тип данных PostgreSQL атрибута (varchar, integer, timestamp и т.д.)
    /// </summary>
    public required string DataType { get; init; }
    
    /// <summary>
    /// Максимальная длина для строковых типов (VARCHAR(n), CHAR(n))
    /// </summary>
    public int? MaxLength { get; init; }
    
    /// <summary>
    /// Точность для числовых типов (NUMERIC(precision, scale))
    /// </summary>
    public int? NumericPrecision { get; init; }
    
    /// <summary>
    /// Масштаб для числовых типов (NUMERIC(precision, scale))
    /// </summary>
    public int? NumericScale { get; init; }
    
    /// <summary>
    /// Является ли тип массивом (например, integer[])
    /// </summary>
    public bool IsArray { get; init; }
}