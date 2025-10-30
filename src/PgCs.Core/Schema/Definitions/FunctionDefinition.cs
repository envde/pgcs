using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение функции или процедуры PostgreSQL (CREATE FUNCTION/PROCEDURE)
/// </summary>
public sealed record FunctionDefinition: DefinitionBase
{
    /// <summary>
    /// Имя функции
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Список параметров функции с их типами и режимами
    /// </summary>
    public required IReadOnlyList<FunctionParameter> Parameters { get; init; }
    
    /// <summary>
    /// Тип возвращаемого значения для скалярных функций (integer, text и т.д.)
    /// Null для процедур (PROCEDURE) или функций, возвращающих TABLE
    /// </summary>
    public string? ReturnType { get; init; }
    
    /// <summary>
    /// Возвращает ли функция таблицу (RETURNS TABLE или RETURNS SETOF)
    /// </summary>
    public bool ReturnsTable { get; init; }
    
    /// <summary>
    /// Список колонок возвращаемой таблицы (для RETURNS TABLE)
    /// </summary>
    public IReadOnlyList<TableColumn>? ReturnTableColumns { get; init; }
    
    /// <summary>
    /// Язык реализации функции (sql, plpgsql, c, python и т.д.)
    /// </summary>
    public required string Language { get; init; }
    
    /// <summary>
    /// Тело функции (SQL код, путь к библиотеке или код на процедурном языке)
    /// </summary>
    public required string Body { get; init; }
    
    /// <summary>
    /// Волатильность функции: VOLATILE, STABLE или IMMUTABLE
    /// Влияет на оптимизацию запросов и кэширование результатов
    /// </summary>
    public FunctionVolatility Volatility { get; init; } = FunctionVolatility.Volatile;
    
    /// <summary>
    /// Является ли функция агрегатной (CREATE AGGREGATE)
    /// </summary>
    public bool IsAggregate { get; init; }
    
    /// <summary>
    /// Является ли функция триггерной (RETURNS TRIGGER)
    /// </summary>
    public bool IsTrigger { get; init; }
    
    /// <summary>
    /// Является ли это процедурой (CREATE PROCEDURE, а не FUNCTION)
    /// Процедуры не возвращают значение и поддерживают транзакции
    /// </summary>
    public bool IsProcedure { get; init; }
    
    /// <summary>
    /// Является ли функция строгой (STRICT / RETURNS NULL ON NULL INPUT)
    /// Строгие функции возвращают NULL при любом NULL аргументе
    /// </summary>
    public bool IsStrict { get; init; }
    
    /// <summary>
    /// Стоимость выполнения функции для планировщика запросов
    /// По умолчанию: 100 для SQL/plpgsql, 1 для C
    /// </summary>
    public int? Cost { get; init; }
    
    /// <summary>
    /// Оценка количества возвращаемых строк (для функций SETOF)
    /// Используется планировщиком для оптимизации
    /// </summary>
    public int? Rows { get; init; }
    
    /// <summary>
    /// Режим безопасности: SECURITY DEFINER или SECURITY INVOKER
    /// DEFINER - выполняется с правами владельца функции
    /// </summary>
    public bool IsSecurityDefiner { get; init; }
    
    /// <summary>
    /// Является ли функция параллельно безопасной (PARALLEL SAFE/RESTRICTED/UNSAFE)
    /// </summary>
    public string? ParallelMode { get; init; }
}