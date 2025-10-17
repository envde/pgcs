namespace PgCs.Common.Generation.Models;

/// <summary>
/// Свойство генерируемой модели
/// </summary>
public sealed record ModelProperty
{
    /// <summary>
    /// Имя свойства C#
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип C#
    /// </summary>
    public required string CSharpType { get; init; }

    /// <summary>
    /// Является nullable (для reference types через аннотацию, для value types через ?)
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Является обязательным (required modifier)
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Значение по умолчанию (выражение C#)
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// XML документация свойства
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Атрибуты свойства (например, [Required], [MaxLength(100)])
    /// </summary>
    public IReadOnlyList<string> Attributes { get; init; } = [];

    /// <summary>
    /// Исходное имя колонки в базе данных
    /// </summary>
    public string? SourceColumnName { get; init; }

    /// <summary>
    /// Тип PostgreSQL (для справки)
    /// </summary>
    public string? PostgresType { get; init; }
}
