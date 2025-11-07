namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Приведение типа (CAST) в PostgreSQL
/// Два синтаксиса: CAST(x AS type) или x::type
/// </summary>
public sealed record PgCastExpression : PgExpression
{
    /// <summary>
    /// Выражение для приведения типа
    /// </summary>
    public required PgExpression Expression { get; init; }

    /// <summary>
    /// Целевой тип данных
    /// Примеры: "integer", "text", "timestamp", "jsonb"
    /// </summary>
    public required string TargetType { get; init; }

    /// <summary>
    /// Модификатор типа (опционально)
    /// Примеры: для VARCHAR(100) это "100", для NUMERIC(10,2) это "10,2"
    /// </summary>
    public string? TypeModifier { get; init; }

    /// <summary>
    /// Использован ли синтаксис :: вместо CAST
    /// true для x::integer, false для CAST(x AS integer)
    /// </summary>
    public bool IsPostgresSyntax { get; init; }
}
