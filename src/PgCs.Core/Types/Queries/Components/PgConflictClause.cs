using PgCs.Core.Types.Base;
using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// ON CONFLICT клауза в INSERT запросе (UPSERT)
/// PostgreSQL специфичная функциональность для обработки конфликтов уникальности
/// </summary>
public sealed record PgConflictClause
{
    /// <summary>
    /// Целевой конфликт: колонки или constraint для обнаружения конфликта
    /// Примеры: ON CONFLICT (email), ON CONFLICT ON CONSTRAINT users_email_key
    /// </summary>
    public PgConflictTarget? Target { get; init; }

    /// <summary>
    /// Действие при конфликте (DO NOTHING или DO UPDATE)
    /// </summary>
    public required PgConflictAction Action { get; init; }

    /// <summary>
    /// SET клаузы для DO UPDATE
    /// </summary>
    public IReadOnlyList<PgSetClause> UpdateSetClauses { get; init; } = [];

    /// <summary>
    /// WHERE условие для DO UPDATE
    /// </summary>
    public PgExpression? UpdateWhereClause { get; init; }
}

/// <summary>
/// Целевой конфликт для ON CONFLICT
/// </summary>
public sealed record PgConflictTarget
{
    /// <summary>
    /// Список колонок для обнаружения конфликта
    /// Пример: (email, username)
    /// </summary>
    public IReadOnlyList<string> Columns { get; init; } = [];

    /// <summary>
    /// Имя constraint для обнаружения конфликта
    /// Пример: users_email_key
    /// </summary>
    public string? ConstraintName { get; init; }

    /// <summary>
    /// WHERE условие для частичного индекса
    /// Пример: ON CONFLICT (email) WHERE deleted_at IS NULL
    /// </summary>
    public PgExpression? WhereClause { get; init; }
}
