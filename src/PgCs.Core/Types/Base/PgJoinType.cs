namespace PgCs.Core.Types.Base;

/// <summary>
/// Типы JOIN операций в PostgreSQL
/// </summary>
public enum PgJoinType
{
    /// <summary>INNER JOIN - возвращает строки когда есть совпадение в обеих таблицах</summary>
    Inner,

    /// <summary>LEFT JOIN (LEFT OUTER JOIN) - возвращает все строки из левой таблицы</summary>
    Left,

    /// <summary>RIGHT JOIN (RIGHT OUTER JOIN) - возвращает все строки из правой таблицы</summary>
    Right,

    /// <summary>FULL JOIN (FULL OUTER JOIN) - возвращает все строки из обеих таблиц</summary>
    Full,

    /// <summary>CROSS JOIN - декартово произведение двух таблиц</summary>
    Cross,

    /// <summary>LEFT OUTER JOIN (синоним LEFT JOIN)</summary>
    LeftOuter,

    /// <summary>RIGHT OUTER JOIN (синоним RIGHT JOIN)</summary>
    RightOuter,

    /// <summary>FULL OUTER JOIN (синоним FULL JOIN)</summary>
    FullOuter
}
