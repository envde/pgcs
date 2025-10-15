namespace PgCs.Common.CodeGeneration.Models;

/// <summary>
/// Сгенерированная модель C#
/// </summary>
public sealed record GeneratedModel
{
    /// <summary>
    /// Имя класса модели
    /// </summary>
    public required string ClassName { get; init; }

    /// <summary>
    /// Пространство имен
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Свойства модели
    /// </summary>
    public required IReadOnlyList<GeneratedProperty> Properties { get; init; }

    /// <summary>
    /// Используемые пространства имен
    /// </summary>
    public IReadOnlyList<string> Usings { get; init; } = [];

    /// <summary>
    /// Атрибуты класса
    /// </summary>
    public IReadOnlyList<string> ClassAttributes { get; init; } = [];

    /// <summary>
    /// XML комментарий класса
    /// </summary>
    public string? XmlComment { get; init; }

    /// <summary>
    /// Является ли класс record
    /// </summary>
    public bool IsRecord { get; init; } = true;

    /// <summary>
    /// Является ли класс partial
    /// </summary>
    public bool IsPartial { get; init; } = true;

    /// <summary>
    /// Базовый класс или интерфейсы
    /// </summary>
    public IReadOnlyList<string> BaseTypesAndInterfaces { get; init; } = [];

    /// <summary>
    /// Дополнительные члены класса (методы, конструкторы)
    /// </summary>
    public IReadOnlyList<string> AdditionalMembers { get; init; } = [];

    /// <summary>
    /// Использовать primary constructor (C# 12+)
    /// </summary>
    public bool UsePrimaryConstructor { get; init; }
}