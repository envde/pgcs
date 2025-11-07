namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Подзапрос (subquery) внутри выражения
/// Может использоваться в SELECT, WHERE, FROM, и других клаузах
/// Примеры: (SELECT MAX(price) FROM products), EXISTS (SELECT 1 FROM ...)
/// </summary>
public sealed record PgSubqueryExpression : PgExpression
{
    /// <summary>
    /// SELECT запрос внутри подзапроса
    /// </summary>
    public required PgSelectQuery Query { get; init; }

    /// <summary>
    /// Тип подзапроса
    /// </summary>
    public PgSubqueryType SubqueryType { get; init; } = PgSubqueryType.Scalar;
}

/// <summary>
/// Типы подзапросов
/// </summary>
public enum PgSubqueryType
{
    /// <summary>Скалярный подзапрос - возвращает одно значение</summary>
    Scalar,

    /// <summary>Подзапрос в EXISTS клаузе</summary>
    Exists,

    /// <summary>Подзапрос в IN клаузе</summary>
    In,

    /// <summary>Подзапрос в FROM клаузе (derived table)</summary>
    Table,

    /// <summary>Подзапрос в ANY/SOME операторе</summary>
    Any,

    /// <summary>Подзапрос в ALL операторе</summary>
    All
}
