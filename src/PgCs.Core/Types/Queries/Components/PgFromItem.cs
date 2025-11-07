using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// Элемент в FROM клаузе
/// Может быть: таблица, подзапрос, функция, VALUES, или JOIN
/// </summary>
public abstract record PgFromItem
{
    /// <summary>
    /// Алиас для элемента FROM (опционально)
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// Алиасы для колонок (опционально)
    /// Пример: FROM users AS u(id, name, email)
    /// </summary>
    public IReadOnlyList<string> ColumnAliases { get; init; } = [];
}

/// <summary>
/// Обычная таблица в FROM клаузе
/// </summary>
public sealed record PgTableFromItem : PgFromItem
{
    /// <summary>
    /// Ссылка на таблицу
    /// </summary>
    public required PgTableReference Table { get; init; }
}

/// <summary>
/// Подзапрос в FROM клаузе (derived table)
/// Пример: FROM (SELECT * FROM users WHERE active) AS u
/// </summary>
public sealed record PgSubqueryFromItem : PgFromItem
{
    /// <summary>
    /// Подзапрос
    /// </summary>
    public required PgSelectQuery Query { get; init; }

    /// <summary>
    /// Является ли это LATERAL подзапрос
    /// </summary>
    public bool IsLateral { get; init; }
}

/// <summary>
/// Вызов функции в FROM клаузе
/// Пример: FROM generate_series(1, 10) AS numbers(n)
/// </summary>
public sealed record PgFunctionFromItem : PgFromItem
{
    /// <summary>
    /// Вызов функции
    /// </summary>
    public required PgFunctionCall Function { get; init; }

    /// <summary>
    /// WITH ORDINALITY - добавляет порядковый номер строки
    /// </summary>
    public bool WithOrdinality { get; init; }
}

/// <summary>
/// VALUES конструктор в FROM клаузе
/// Пример: FROM (VALUES (1, 'a'), (2, 'b')) AS t(id, name)
/// </summary>
public sealed record PgValuesFromItem : PgFromItem
{
    /// <summary>
    /// Строки значений
    /// </summary>
    public required IReadOnlyList<IReadOnlyList<PgExpression>> Rows { get; init; }
}

/// <summary>
/// JOIN выражение в FROM клаузе
/// </summary>
public sealed record PgJoinFromItem : PgFromItem
{
    /// <summary>
    /// Левая часть JOIN
    /// </summary>
    public required PgFromItem LeftItem { get; init; }

    /// <summary>
    /// JOIN клауза
    /// </summary>
    public required PgJoinClause JoinClause { get; init; }
}
