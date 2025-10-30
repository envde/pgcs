namespace PgCs.Core.Schema.Common;

/// <summary>
/// Базовый класс для всех определений объектов схемы базы данных PostgreSQL
/// </summary>
public abstract record DefinitionBase
{
    /// <summary>
    /// Имя схемы, в которой находится объект (например, "public", "app")
    /// Если null, используется схема по умолчанию
    /// </summary>
    public string? Schema { get; init; }
    
    /// <summary>
    /// SQL комментарий, извлечённый из исходного кода (-- comment)
    /// Может быть расположен строкой выше определения или inline
    /// </summary>
    public string? SqlComment { get; init; }
    
    /// <summary>
    /// Исходный SQL текст определения объекта
    /// Сохраняется для возможности точного воссоздания
    /// </summary>
    public string? RawSql { get; init; }
}