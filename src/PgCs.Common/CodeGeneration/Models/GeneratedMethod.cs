namespace PgCs.Common.CodeGeneration.Models;

/// <summary>
/// Сгенерированный метод
/// </summary>
public sealed record GeneratedMethod
{
    /// <summary>
    /// Имя метода
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип возвращаемого значения
    /// </summary>
    public required string ReturnType { get; init; }

    /// <summary>
    /// Параметры метода
    /// </summary>
    public required IReadOnlyList<MethodParameter> Parameters { get; init; }

    /// <summary>
    /// Тело метода (код)
    /// </summary>
    public required string Body { get; init; }

    /// <summary>
    /// Модификаторы доступа
    /// </summary>
    public string AccessModifiers { get; init; } = "public";

    /// <summary>
    /// Является ли асинхронным
    /// </summary>
    public bool IsAsync { get; init; }

    /// <summary>
    /// Является ли статическим
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// Является ли виртуальным
    /// </summary>
    public bool IsVirtual { get; init; }

    /// <summary>
    /// XML комментарий
    /// </summary>
    public string? XmlComment { get; init; }

    /// <summary>
    /// Атрибуты метода
    /// </summary>
    public IReadOnlyList<string> Attributes { get; init; } = [];

    /// <summary>
    /// Generic параметры типов
    /// </summary>
    public IReadOnlyList<string> GenericParameters { get; init; } = [];

    /// <summary>
    /// Generic ограничения (where T : ...)
    /// </summary>
    public IReadOnlyList<string> GenericConstraints { get; init; } = [];

    /// <summary>
    /// SQL запрос, который выполняет метод
    /// </summary>
    public string? SqlQuery { get; init; }
}