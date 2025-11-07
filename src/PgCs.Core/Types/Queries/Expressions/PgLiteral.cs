using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Литеральное значение в SQL выражении
/// Примеры: 42, 'text', true, NULL, '2025-11-07'::date
/// </summary>
public sealed record PgLiteral : PgExpression
{
    /// <summary>
    /// Значение литерала
    /// Может быть: string, int, long, decimal, bool, DateTime, или null
    /// </summary>
    public required object? Value { get; init; }

    /// <summary>
    /// Тип литерала
    /// </summary>
    public required PgLiteralType LiteralType { get; init; }

    /// <summary>
    /// Явное приведение типа через :: оператор
    /// Пример: '123'::integer, 'true'::boolean
    /// </summary>
    public string? ExplicitCast { get; init; }
}
