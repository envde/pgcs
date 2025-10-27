using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение функции или процедуры
/// </summary>
public sealed record FunctionDefinition: DefinitionBase
{
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
    public IReadOnlyList<TableColumn>? ReturnTableColumns { get; init; }
    
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
}