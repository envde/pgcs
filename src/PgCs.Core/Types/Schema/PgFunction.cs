using PgCs.Core.Parser.Metadata;
using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Schema;

/// <summary>
/// Определение функции или процедуры PostgreSQL
/// </summary>
/// <remarks>
/// CREATE FUNCTION - возвращает значение
/// CREATE PROCEDURE - не возвращает значение, поддерживает управление транзакциями
/// </remarks>
public sealed record PgFunction : PgObject
{
    /// <summary>
    /// Список параметров функции
    /// </summary>
    public IReadOnlyList<PgFunctionParameter> Parameters { get; init; } = [];

    /// <summary>
    /// Тип возвращаемого значения для скалярных функций
    /// </summary>
    /// <example>integer, text, uuid, my_custom_type</example>
    /// <remarks>
    /// Null для процедур (PROCEDURE) или функций, возвращающих TABLE/SETOF
    /// </remarks>
    public string? ReturnType { get; init; }

    /// <summary>
    /// Возвращает ли функция набор строк (RETURNS SETOF / RETURNS TABLE)
    /// </summary>
    public bool ReturnsSet { get; init; }

    /// <summary>
    /// Список колонок возвращаемой таблицы (для RETURNS TABLE)
    /// </summary>
    public IReadOnlyList<PgColumn>? ReturnTableColumns { get; init; }

    /// <summary>
    /// Язык реализации функции
    /// </summary>
    /// <example>sql, plpgsql, plpython3u, c</example>
    public required string Language { get; init; }

    /// <summary>
    /// Тело функции (SQL код или код на процедурном языке)
    /// </summary>
    /// <remarks>
    /// Для C функций - путь к библиотеке и имя функции
    /// </remarks>
    public required string Body { get; init; }

    /// <summary>
    /// Волатильность функции: VOLATILE, STABLE или IMMUTABLE
    /// </summary>
    public PgFunctionVolatility Volatility { get; init; } = PgFunctionVolatility.Volatile;

    /// <summary>
    /// Является ли это процедурой (CREATE PROCEDURE)
    /// </summary>
    /// <remarks>
    /// Процедуры не возвращают значение и могут управлять транзакциями (COMMIT/ROLLBACK)
    /// </remarks>
    public bool IsProcedure { get; init; }

    /// <summary>
    /// Является ли функция строгой (STRICT / RETURNS NULL ON NULL INPUT)
    /// </summary>
    /// <remarks>
    /// Строгие функции возвращают NULL при любом NULL аргументе без вызова тела функции
    /// </remarks>
    public bool IsStrict { get; init; }

    /// <summary>
    /// Стоимость выполнения функции для планировщика запросов
    /// </summary>
    /// <remarks>
    /// По умолчанию: 100 для SQL/plpgsql, 1 для C функций
    /// Используется планировщиком для оценки стоимости запроса
    /// </remarks>
    public int? Cost { get; init; }

    /// <summary>
    /// Оценка количества возвращаемых строк (для SETOF функций)
    /// </summary>
    public int? Rows { get; init; }

    /// <summary>
    /// Режим безопасности: SECURITY DEFINER или SECURITY INVOKER (по умолчанию)
    /// </summary>
    /// <remarks>
    /// SECURITY DEFINER - выполняется с правами владельца функции
    /// SECURITY INVOKER - выполняется с правами вызывающего пользователя
    /// </remarks>
    public bool IsSecurityDefiner { get; init; }

    /// <summary>
    /// Режим параллельного выполнения
    /// </summary>
    /// <example>SAFE, RESTRICTED, UNSAFE</example>
    /// <remarks>
    /// SAFE - можно выполнять параллельно
    /// RESTRICTED - можно выполнять в параллельном режиме, но не в параллельных workers
    /// UNSAFE - нельзя выполнять параллельно (по умолчанию)
    /// </remarks>
    public string? ParallelMode { get; init; }

    /// <summary>
    /// Является ли функция window-функцией
    /// </summary>
    public bool IsWindow { get; init; }

    /// <summary>
    /// Является ли функция триггерной (RETURNS TRIGGER)
    /// </summary>
    public bool IsTrigger { get; init; }
}
