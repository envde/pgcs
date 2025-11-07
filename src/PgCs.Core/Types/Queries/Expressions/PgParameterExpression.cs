namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Параметр в подготовленном запросе
/// Примеры: $1, $2, :name, @param
/// </summary>
public sealed record PgParameterExpression : PgExpression
{
    /// <summary>
    /// Имя или номер параметра
    /// Примеры: "1", "2" для $1, $2 или "name" для :name
    /// </summary>
    public required string ParameterName { get; init; }

    /// <summary>
    /// Стиль параметра
    /// </summary>
    public PgParameterStyle Style { get; init; } = PgParameterStyle.Positional;
}

/// <summary>
/// Стили параметров в PostgreSQL
/// </summary>
public enum PgParameterStyle
{
    /// <summary>Позиционный параметр: $1, $2, $3</summary>
    Positional,

    /// <summary>Именованный параметр: :name, @param</summary>
    Named
}
