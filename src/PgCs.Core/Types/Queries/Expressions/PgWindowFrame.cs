namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Фрейм окна для оконных функций
/// Определяет набор строк относительно текущей строки
/// </summary>
public sealed record PgWindowFrame
{
    /// <summary>
    /// Тип фрейма: ROWS, RANGE, или GROUPS
    /// </summary>
    public required PgWindowFrameType FrameType { get; init; }

    /// <summary>
    /// Начало фрейма
    /// </summary>
    public required PgWindowFrameBound StartBound { get; init; }

    /// <summary>
    /// Конец фрейма (опционально, по умолчанию CURRENT ROW)
    /// </summary>
    public PgWindowFrameBound? EndBound { get; init; }

    /// <summary>
    /// Режим исключения строк: EXCLUDE CURRENT ROW, EXCLUDE GROUP, etc.
    /// </summary>
    public PgWindowFrameExclusion? Exclusion { get; init; }
}

/// <summary>
/// Тип фрейма окна
/// </summary>
public enum PgWindowFrameType
{
    /// <summary>Физические строки: ROWS</summary>
    Rows,

    /// <summary>Логический диапазон по значению: RANGE</summary>
    Range,

    /// <summary>Группы одинаковых значений: GROUPS</summary>
    Groups
}

/// <summary>
/// Граница фрейма окна
/// </summary>
public sealed record PgWindowFrameBound
{
    /// <summary>
    /// Тип границы
    /// </summary>
    public required PgWindowFrameBoundType BoundType { get; init; }

    /// <summary>
    /// Смещение для PRECEDING/FOLLOWING
    /// Пример: 5 PRECEDING, 10 FOLLOWING
    /// </summary>
    public PgExpression? Offset { get; init; }
}

/// <summary>
/// Типы границ фрейма окна
/// </summary>
public enum PgWindowFrameBoundType
{
    /// <summary>От начала раздела: UNBOUNDED PRECEDING</summary>
    UnboundedPreceding,

    /// <summary>N строк/значений назад: N PRECEDING</summary>
    Preceding,

    /// <summary>Текущая строка: CURRENT ROW</summary>
    CurrentRow,

    /// <summary>N строк/значений вперёд: N FOLLOWING</summary>
    Following,

    /// <summary>До конца раздела: UNBOUNDED FOLLOWING</summary>
    UnboundedFollowing
}

/// <summary>
/// Режим исключения строк из фрейма
/// </summary>
public enum PgWindowFrameExclusion
{
    /// <summary>Исключить текущую строку: EXCLUDE CURRENT ROW</summary>
    CurrentRow,

    /// <summary>Исключить группу текущей строки: EXCLUDE GROUP</summary>
    Group,

    /// <summary>Исключить связи: EXCLUDE TIES</summary>
    Ties,

    /// <summary>Не исключать ничего: EXCLUDE NO OTHERS (по умолчанию)</summary>
    NoOthers
}
