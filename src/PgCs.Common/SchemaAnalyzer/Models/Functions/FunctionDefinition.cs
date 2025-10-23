using PgCs.Common.SchemaAnalyzer.Models.Tables;

namespace PgCs.Common.SchemaAnalyzer.Models.Functions;

/// <summary>
/// Определение функции или процедуры
/// </summary>
public sealed record FunctionDefinition
{
    /// <summary>
    /// Имя функции
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Схема функции (если отличается от public)
    /// </summary>
    public string? Schema { get; init; }
    
    /// <summary>
    /// Список параметров функции
    /// </summary>
    public required IReadOnlyList<FunctionParameter> Parameters { get; init; }
    
    /// <summary>
    /// Тип возвращаемого значения (для скалярных функций)
    /// </summary>
    public string? ReturnType { get; init; }
    
    /// <summary>
    /// Возвращает ли функция таблицу (TABLE или SETOF)
    /// </summary>
    public bool ReturnsTable { get; init; }
    
    /// <summary>
    /// Список колонок возвращаемой таблицы (для RETURNS TABLE)
    /// </summary>
    public IReadOnlyList<ColumnDefinition>? ReturnTableColumns { get; init; }
    
    /// <summary>
    /// Язык реализации функции (SQL, PL/pgSQL, C и т.д.)
    /// </summary>
    public required string Language { get; init; }
    
    /// <summary>
    /// Тело функции (SQL код или путь к библиотеке)
    /// </summary>
    public required string Body { get; init; }
    
    /// <summary>
    /// Волатильность функции (Volatile, Stable, Immutable)
    /// </summary>
    public FunctionVolatility Volatility { get; init; } = FunctionVolatility.Volatile;
    
    /// <summary>
    /// Является ли функция агрегатной
    /// </summary>
    public bool IsAggregate { get; init; }
    
    /// <summary>
    /// Является ли функция триггерной
    /// </summary>
    public bool IsTrigger { get; init; }
    
    /// <summary>
    /// Комментарий к функции (COMMENT ON FUNCTION)
    /// </summary>
    public string? Comment { get; init; }
    
    /// <summary>
    /// Исходный SQL код создания функции
    /// </summary>
    public string? RawSql { get; init; }
}