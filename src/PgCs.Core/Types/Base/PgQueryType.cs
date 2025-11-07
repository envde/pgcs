namespace PgCs.Core.Types.Base;

/// <summary>
/// Типы SQL запросов PostgreSQL
/// </summary>
public enum PgQueryType
{
    /// <summary>SELECT запрос (выборка данных)</summary>
    Select,

    /// <summary>INSERT запрос (вставка данных)</summary>
    Insert,

    /// <summary>UPDATE запрос (обновление данных)</summary>
    Update,

    /// <summary>DELETE запрос (удаление данных)</summary>
    Delete,

    /// <summary>WITH запрос (Common Table Expression)</summary>
    With,

    /// <summary>VALUES запрос (литеральные значения как таблица)</summary>
    Values
}
