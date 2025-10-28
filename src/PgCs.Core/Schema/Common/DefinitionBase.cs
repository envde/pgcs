namespace PgCs.Core.Schema.Common;

/// <summary>
/// Базовый класс для определений типов данных PostgreSQL
/// </summary>
public abstract record DefinitionBase
{
    /// <summary>
    /// Схема
    /// </summary>
    public string? Schema { get; init; }
    /// <summary>
    /// SQL комментарий, который задается символом "--", строкой выше или inline. Сохраняется в это поле.
    /// </summary>
    public string? SqlComment { get; init; }
    /// <summary>
    /// Исходный SQL текст объекта
    /// </summary>
    public string? RawSql { get; init; }
}