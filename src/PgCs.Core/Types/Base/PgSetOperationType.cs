namespace PgCs.Core.Types.Base;

/// <summary>
/// Типы операций над множествами в SQL запросах
/// Используется для объединения нескольких SELECT запросов
/// </summary>
public enum PgSetOperationType
{
    /// <summary>Объединение результатов: UNION (без дубликатов)</summary>
    Union,

    /// <summary>Объединение результатов со всеми дубликатами: UNION ALL</summary>
    UnionAll,

    /// <summary>Пересечение результатов: INTERSECT</summary>
    Intersect,

    /// <summary>Пересечение результатов со всеми дубликатами: INTERSECT ALL</summary>
    IntersectAll,

    /// <summary>Разность результатов: EXCEPT</summary>
    Except,

    /// <summary>Разность результатов со всеми дубликатами: EXCEPT ALL</summary>
    ExceptAll
}
