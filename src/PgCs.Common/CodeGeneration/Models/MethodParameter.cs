namespace PgCs.Common.CodeGeneration.Models;

/// <summary>
/// Параметр метода
/// </summary>
public sealed record MethodParameter
{
    /// <summary>
    /// Имя параметра
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип параметра
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Значение по умолчанию
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Модификаторы (ref, out, in, params)
    /// </summary>
    public string? Modifiers { get; init; }

    /// <summary>
    /// Атрибуты параметра
    /// </summary>
    public IReadOnlyList<string> Attributes { get; init; } = [];

    /// <summary>
    /// Порядковый номер
    /// </summary>
    public int Position { get; init; }
}