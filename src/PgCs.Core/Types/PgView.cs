using PgCs.Core.Parsing.SqlMetadata;
using PgCs.Core.Types.Base;

namespace PgCs.Core.Types;

/// <summary>
/// Определение представления PostgreSQL (CREATE VIEW / CREATE MATERIALIZED VIEW)
/// </summary>
/// <remarks>
/// Представление - виртуальная таблица, основанная на результате SQL запроса.
/// Материализованное представление физически хранит данные и требует обновления (REFRESH).
/// </remarks>
public sealed record PgView : PgObject
{
    /// <summary>
    /// SQL запрос, определяющий представление (SELECT ...)
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// Список колонок представления
    /// </summary>
    /// <remarks>
    /// Может быть определён явно или выведен из запроса.
    /// </remarks>
    public IReadOnlyList<PgColumn> Columns { get; init; } = [];

    /// <summary>
    /// Является ли представление материализованным (MATERIALIZED VIEW)
    /// </summary>
    /// <remarks>
    /// Материализованные представления:
    /// - Физически хранят данные
    /// - Требуют периодического обновления (REFRESH MATERIALIZED VIEW)
    /// - Быстрее обычных представлений для сложных запросов
    /// - Могут иметь индексы
    /// </remarks>
    public bool IsMaterialized { get; init; }

    /// <summary>
    /// Опция безопасности представления (WITH CHECK OPTION)
    /// </summary>
    /// <remarks>
    /// Определяет, можно ли изменять данные через представление.
    /// Гарантирует, что вставляемые/обновляемые строки удовлетворяют условию WHERE представления.
    /// </remarks>
    public bool WithCheckOption { get; init; }

    /// <summary>
    /// Использует ли представление SECURITY BARRIER
    /// </summary>
    /// <remarks>
    /// Защищает от утечки данных через побочные эффекты пользовательских функций.
    /// Важно для представлений с политиками безопасности (RLS).
    /// </remarks>
    public bool IsSecurityBarrier { get; init; }

    /// <summary>
    /// Параметры хранения для материализованного представления (WITH)
    /// </summary>
    /// <example>fillfactor=70, autovacuum_enabled=true</example>
    public IReadOnlyDictionary<string, string>? StorageParameters { get; init; }

    /// <summary>
    /// Табличное пространство для материализованного представления (TABLESPACE)
    /// </summary>
    public string? Tablespace { get; init; }
}
