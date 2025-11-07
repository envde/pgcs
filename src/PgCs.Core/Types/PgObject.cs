using PgCs.Core.Parsing.SqlMetadata;

namespace PgCs.Core.Types;

/// <summary>
/// Базовый класс для всех объектов PostgreSQL
/// </summary>
/// <remarks>
/// Все объекты БД (таблицы, представления, функции, типы) наследуются от этого класса.
/// Содержит общие свойства, присущие всем объектам PostgreSQL.
/// </remarks>
public abstract record PgObject
{
    /// <summary>
    /// Имя схемы, в которой находится объект
    /// </summary>
    /// <example>public, app, core</example>
    public string Schema { get; init; } = "public";

    /// <summary>
    /// Имя объекта
    /// </summary>
    /// <example>users, orders, get_user_by_id</example>
    public required string Name { get; init; }

    /// <summary>
    /// Полное квалифицированное имя объекта (schema.name)
    /// </summary>
    public string FullName => string.IsNullOrEmpty(Schema) ? Name : $"{Schema}.{Name}";

    /// <summary>
    /// Исходный SQL текст определения объекта (если доступен)
    /// </summary>
    /// <remarks>
    /// Сохраняется для возможности точного воспроизведения оригинального SQL.
    /// Может быть null если объект создан программно.
    /// </remarks>
    public string? SourceSql { get; init; }

    /// <summary>
    /// Комментарий к объекту PostgreSQL
    /// </summary>
    public SqlComment? SqlComment { get; init; }
}
