namespace PgCs.Common.CodeGeneration.Models;

/// <summary>
/// Значение перечисления
/// </summary>
public sealed record EnumValue
{
    /// <summary>
    /// Имя значения
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Числовое значение (опционально)
    /// </summary>
    public int? Value { get; init; }

    /// <summary>
    /// XML комментарий
    /// </summary>
    public string? XmlComment { get; init; }

    /// <summary>
    /// Атрибуты значения
    /// </summary>
    public IReadOnlyList<string> Attributes { get; init; } = [];

    /// <summary>
    /// Оригинальное значение из БД
    /// </summary>
    public string? OriginalDbValue { get; init; }

    /// <summary>
    /// Порядковый номер
    /// </summary>
    public int Order { get; init; }
}