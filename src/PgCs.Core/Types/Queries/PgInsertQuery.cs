using PgCs.Core.Types.Base;
using PgCs.Core.Types.Queries.Components;
using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries;

/// <summary>
/// INSERT запрос PostgreSQL
/// Вставка новых строк в таблицу
/// </summary>
public sealed record PgInsertQuery : PgQuery
{
    /// <summary>
    /// Целевая таблица для вставки
    /// </summary>
    public required PgTableReference Table { get; init; }

    /// <summary>
    /// Список колонок для вставки (опционально)
    /// Если не указан, используются все колонки таблицы в порядке определения
    /// </summary>
    public IReadOnlyList<string> Columns { get; init; } = [];

    /// <summary>
    /// VALUES клауза с данными для вставки
    /// Пример: VALUES (1, 'John'), (2, 'Jane')
    /// </summary>
    public IReadOnlyList<IReadOnlyList<PgExpression>>? ValuesRows { get; init; }

    /// <summary>
    /// SELECT запрос как источник данных (альтернатива VALUES)
    /// Пример: INSERT INTO users SELECT * FROM temp_users
    /// </summary>
    public PgSelectQuery? SelectQuery { get; init; }

    /// <summary>
    /// Вставка значений по умолчанию: INSERT INTO table DEFAULT VALUES
    /// </summary>
    public bool IsDefaultValues { get; init; }

    /// <summary>
    /// Перезаписывать существующие данные: OVERRIDING SYSTEM/USER VALUE
    /// PostgreSQL специфичная опция для IDENTITY колонок
    /// </summary>
    public PgOverridingValue? OverridingValue { get; init; }

    /// <summary>
    /// ON CONFLICT клауза для обработки конфликтов (UPSERT)
    /// </summary>
    public PgConflictClause? ConflictClause { get; init; }

    /// <summary>
    /// RETURNING клауза для возврата вставленных данных
    /// Пример: RETURNING id, created_at
    /// </summary>
    public IReadOnlyList<PgSelectItem> ReturningClause { get; init; } = [];
}

/// <summary>
/// Перезапись значений IDENTITY колонок
/// </summary>
public enum PgOverridingValue
{
    /// <summary>Перезаписать системные значения: OVERRIDING SYSTEM VALUE</summary>
    System,

    /// <summary>Перезаписать пользовательские значения: OVERRIDING USER VALUE</summary>
    User
}
