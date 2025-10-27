namespace PgCs.Core.Definitions.Schema.Base;

/// <summary>
/// Атрибут композитного типа
/// </summary>
public sealed record CompositeTypeAttribute
{
    /// <summary>
    /// Имя атрибута (поля) композитного типа
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Тип данных PostgreSQL атрибута
    /// </summary>
    public required string DataType { get; init; }
    
    /// <summary>
    /// Максимальная длина для строковых типов
    /// </summary>
    public int? MaxLength { get; init; }
}