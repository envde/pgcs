using PgCs.Core.Parser.Metadata;
using PgCs.Core.Types.Base;

namespace PgCs.Core.Types;

/// <summary>
/// Определение таблицы PostgreSQL (CREATE TABLE)
/// </summary>
public sealed record PgTable : PgObject
{
    /// <summary>
    /// Список колонок таблицы
    /// </summary>
    public required IReadOnlyList<PgColumn> Columns { get; init; }

    /// <summary>
    /// Является ли таблица партиционированной (PARTITION BY)
    /// </summary>
    /// <remarks>
    /// Партиционированные таблицы физически разделены на части для улучшения производительности.
    /// </remarks>
    public bool IsPartitioned { get; init; }

    /// <summary>
    /// Информация о стратегии партиционирования (если таблица партиционирована)
    /// </summary>
    public PgPartitionInfo? PartitionInfo { get; init; }

    /// <summary>
    /// Является ли таблица партицией другой таблицы (PARTITION OF)
    /// </summary>
    public bool IsPartition { get; init; }

    /// <summary>
    /// Имя родительской таблицы (если это партиция)
    /// </summary>
    public string? ParentTableName { get; init; }

    /// <summary>
    /// Является ли таблица временной (TEMPORARY/TEMP)
    /// </summary>
    /// <remarks>
    /// Временные таблицы автоматически удаляются после завершения сессии.
    /// </remarks>
    public bool IsTemporary { get; init; }

    /// <summary>
    /// Является ли таблица незалоггированной (UNLOGGED)
    /// </summary>
    /// <remarks>
    /// Незалоггированные таблицы быстрее, но данные теряются при сбое сервера.
    /// </remarks>
    public bool IsUnlogged { get; init; }

    /// <summary>
    /// Табличное пространство, в котором хранится таблица (TABLESPACE)
    /// </summary>
    public string? Tablespace { get; init; }

    /// <summary>
    /// Параметры хранения таблицы (WITH)
    /// </summary>
    /// <example>fillfactor=70, autovacuum_enabled=false</example>
    public IReadOnlyDictionary<string, string>? StorageParameters { get; init; }

    /// <summary>
    /// Список таблиц, от которых наследуется данная таблица (INHERITS)
    /// </summary>
    /// <remarks>
    /// PostgreSQL поддерживает наследование таблиц - дочерняя таблица включает все колонки родительской.
    /// </remarks>
    public IReadOnlyList<string>? InheritsFrom { get; init; }

    /// <summary>
    /// Имеет ли таблица OID колонку (WITH OIDS)
    /// </summary>
    /// <remarks>
    /// Устаревшая функция, не рекомендуется к использованию в PostgreSQL 12+
    /// </remarks>
    public bool WithOids { get; init; }
}
