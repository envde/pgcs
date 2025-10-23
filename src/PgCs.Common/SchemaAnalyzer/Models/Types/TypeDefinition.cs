namespace PgCs.Common.SchemaAnalyzer.Models.Types;

/// <summary>
/// Определение пользовательского типа данных
/// </summary>
public sealed record TypeDefinition
{
    /// <summary>
    /// Имя типа
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Схема типа (если отличается от public)
    /// </summary>
    public string? Schema { get; init; }
    
    /// <summary>
    /// Вид типа (Enum, Composite, Domain, Range)
    /// </summary>
    public required TypeKind Kind { get; init; }
    
    /// <summary>
    /// Список значений для ENUM типа
    /// </summary>
    public IReadOnlyList<string> EnumValues { get; init; } = [];
    
    /// <summary>
    /// Список атрибутов для Composite типа
    /// </summary>
    public IReadOnlyList<CompositeTypeAttribute> CompositeAttributes { get; init; } = [];
    
    /// <summary>
    /// Информация о Domain типе
    /// </summary>
    public DomainTypeInfo? DomainInfo { get; init; }
    
    /// <summary>
    /// Комментарий к типу (COMMENT ON TYPE)
    /// </summary>
    public string? Comment { get; init; }
    
    /// <summary>
    /// Исходный SQL код создания типа
    /// </summary>
    public string? RawSql { get; init; }
}