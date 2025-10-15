namespace PgCs.Common.CodeGeneration.Models;

/// <summary>
/// Сгенерированное свойство класса
/// </summary>
public sealed record GeneratedProperty
{
    /// <summary>
    /// Имя свойства
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип свойства
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Является ли nullable
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Является ли required (C# 11+)
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Атрибуты свойства
    /// </summary>
    public IReadOnlyList<string> Attributes { get; init; } = [];

    /// <summary>
    /// XML комментарий
    /// </summary>
    public string? XmlComment { get; init; }

    /// <summary>
    /// Значение по умолчанию (для init)
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Getter (для кастомной логики)
    /// </summary>
    public string? CustomGetter { get; init; }

    /// <summary>
    /// Setter (для кастомной логики)
    /// </summary>
    public string? CustomSetter { get; init; }

    /// <summary>
    /// Порядковый номер для генерации
    /// </summary>
    public int Order { get; init; }
}