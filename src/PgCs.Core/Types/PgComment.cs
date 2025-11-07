using PgCs.Core.Types.Base;

namespace PgCs.Core.Types;

/// <summary>
/// PostgreSQL комментарий к объекту базы данных (COMMENT ON)
/// </summary>
/// <remarks>
/// Представляет команду COMMENT ON в PostgreSQL, которая добавляет описание к объектам БД.
/// 
/// <para>
/// <b>НЕ путать с Comment:</b>
/// <list type="bullet">
/// <item><description>PgComment - это PostgreSQL объект (COMMENT ON TABLE ... IS '...')</description></item>
/// <item><description>Comment - это служебный комментарий в SQL коде (-- comment: ...)</description></item>
/// </list>
/// </para>
/// 
/// Примеры использования:
/// <code>
/// COMMENT ON TABLE users IS 'Таблица пользователей системы';
/// COMMENT ON COLUMN users.email IS 'Email адрес пользователя';
/// COMMENT ON FUNCTION calculate_total(int) IS 'Вычисляет общую сумму заказа';
/// </code>
/// 
/// См. документацию PostgreSQL: https://www.postgresql.org/docs/current/sql-comment.html
/// </remarks>
public sealed record PgComment : PgObject
{
    /// <summary>
    /// Тип объекта, к которому применяется комментарий
    /// </summary>
    /// <example>Table, Column, Function, View и т.д.</example>
    public required PgCommentObjectType ObjectType { get; init; }

    /// <summary>
    /// Текст комментария
    /// </summary>
    /// <remarks>
    /// NULL означает удаление комментария (COMMENT ON ... IS NULL)
    /// </remarks>
    /// <example>Таблица пользователей системы</example>
    public string? CommentText { get; init; }

    /// <summary>
    /// Для колонок: имя таблицы, которой принадлежит колонка
    /// </summary>
    /// <remarks>
    /// Заполняется только для ObjectType = Column.
    /// Полезно для быстрого поиска комментариев к колонкам конкретной таблицы.
    /// </remarks>
    /// <example>users, products, orders</example>
    public string? TableName { get; init; }

    /// <summary>
    /// Для колонок: имя колонки (без префикса таблицы)
    /// </summary>
    /// <remarks>
    /// Заполняется только для ObjectType = Column.
    /// Позволяет избежать парсинга ObjectName для получения имени колонки.
    /// </remarks>
    /// <example>email, user_id, created_at</example>
    public string? ColumnName { get; init; }

    /// <summary>
    /// Для функций: сигнатура аргументов
    /// </summary>
    /// <remarks>
    /// Заполняется для ObjectType = Function, Procedure, Aggregate.
    /// PostgreSQL требует сигнатуру для однозначной идентификации перегруженных функций.
    /// </remarks>
    /// <example>(integer, text), (numeric, numeric), ()</example>
    public string? FunctionSignature { get; init; }
}
