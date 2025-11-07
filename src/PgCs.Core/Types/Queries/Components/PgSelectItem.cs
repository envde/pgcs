using PgCs.Core.Types.Queries.Expressions;

namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// Элемент в SELECT клаузе
/// Может быть: колонка, выражение, *, table.*
/// </summary>
public sealed record PgSelectItem
{
    /// <summary>
    /// Выражение для выбора
    /// </summary>
    public required PgExpression Expression { get; init; }

    /// <summary>
    /// Алиас для результата (опционально)
    /// Пример: SELECT name AS user_name
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// Использовано ли ключевое слово AS перед алиасом
    /// </summary>
    public bool HasExplicitAs { get; init; }
}
