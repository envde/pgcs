namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// CASE выражение в PostgreSQL
/// Два формата: простой (CASE x WHEN ... THEN ...) и поисковый (CASE WHEN condition THEN ...)
/// </summary>
public sealed record PgCaseExpression : PgExpression
{
    /// <summary>
    /// Выражение для простого CASE (опционально)
    /// Пример: CASE status WHEN 'active' THEN ...
    /// Если null, то это поисковый CASE
    /// </summary>
    public PgExpression? CaseOperand { get; init; }

    /// <summary>
    /// Список WHEN ... THEN клауз
    /// </summary>
    public required IReadOnlyList<PgCaseWhen> WhenClauses { get; init; }

    /// <summary>
    /// ELSE клауза (значение по умолчанию)
    /// Если не указано, то по умолчанию NULL
    /// </summary>
    public PgExpression? ElseClause { get; init; }
}

/// <summary>
/// WHEN ... THEN клауза в CASE выражении
/// </summary>
public sealed record PgCaseWhen
{
    /// <summary>
    /// Условие в WHEN клаузе
    /// Для простого CASE это просто значение для сравнения
    /// Для поискового CASE это boolean выражение
    /// </summary>
    public required PgExpression Condition { get; init; }

    /// <summary>
    /// Результат в THEN клаузе
    /// </summary>
    public required PgExpression Result { get; init; }
}
