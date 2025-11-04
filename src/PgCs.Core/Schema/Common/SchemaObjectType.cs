namespace PgCs.Core.Schema.Common;

/// <summary>
/// Типы объектов базы данных для анализа
/// </summary>
public enum SchemaObjectType
{
    /// <summary>
    /// Не анализировать объекты
    /// </summary>
    None,

    /// <summary>
    /// Таблицы
    /// </summary>
    Tables,

    /// <summary>
    /// Представления (VIEW)
    /// </summary>
    Views,

    /// <summary>
    /// Пользовательские типы данных
    /// </summary>
    Types,

    /// <summary>
    /// Функции и процедуры
    /// </summary>
    Functions,

    /// <summary>
    /// Комментарий (COMMENT)
    /// </summary>
    Comments,

    /// <summary>
    /// Индексы
    /// </summary>
    Indexes,

    /// <summary>
    /// Триггеры
    /// </summary>
    Triggers,

    /// <summary>
    /// Ограничения целостности (constraints)
    /// </summary>
    Constraints,

    /// <summary>
    /// Колонки в таблице
    /// </summary>
    Columns,

    /// <summary>
    /// Разделы таблиц (partitions)
    /// </summary>
    Partitions,
}