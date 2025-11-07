namespace PgCs.Core.Types.Base;

/// <summary>
/// Позиция NULL значений в сортировке ORDER BY
/// PostgreSQL позволяет явно указывать где размещать NULL: NULLS FIRST или NULLS LAST
/// </summary>
public enum PgNullsOrdering
{
    /// <summary>NULL значения в начале результата: NULLS FIRST</summary>
    First,

    /// <summary>NULL значения в конце результата: NULLS LAST</summary>
    Last,

    /// <summary>Позиция по умолчанию (зависит от направления сортировки)</summary>
    Default
}
