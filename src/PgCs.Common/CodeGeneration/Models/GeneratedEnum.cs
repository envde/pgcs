namespace PgCs.Common.CodeGeneration.Models;

/// <summary>
/// Сгенерированное перечисление
/// </summary>
public sealed record GeneratedEnum
{
    /// <summary>
    /// Имя перечисления
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Пространство имен
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Значения перечисления
    /// </summary>
    public required IReadOnlyList<EnumValue> Values { get; init; }

    /// <summary>
    /// XML комментарий
    /// </summary>
    public string? XmlComment { get; init; }

    /// <summary>
    /// Атрибуты перечисления
    /// </summary>
    public IReadOnlyList<string> Attributes { get; init; } = [];

    /// <summary>
    /// Базовый тип (int, byte, long и т.д.)
    /// </summary>
    public string? UnderlyingType { get; init; }

    /// <summary>
    /// Является ли флагами ([Flags])
    /// </summary>
    public bool IsFlags { get; init; }

    /// <summary>
    /// Оригинальное имя типа из PostgreSQL
    /// </summary>
    public string? PostgresTypeName { get; init; }
}