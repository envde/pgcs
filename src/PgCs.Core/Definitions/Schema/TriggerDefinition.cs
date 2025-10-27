using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.Definitions.Schema;

/// <summary>
/// Определение триггера
/// </summary>
public sealed record TriggerDefinition: DefinitionBase
{
    
    /// <summary>
    /// Время срабатывания триггера (Before, After, InsteadOf)
    /// </summary>
    public required TriggerTiming Timing { get; init; }
    
    /// <summary>
    /// Список событий, вызывающих триггер (Insert, Update, Delete, Truncate)
    /// </summary>
    public required IReadOnlyList<TriggerEvent> Events { get; init; }
    
    /// <summary>
    /// Имя функции, выполняемой триггером
    /// </summary>
    public required string FunctionName { get; init; }
    
    /// <summary>
    /// Уровень срабатывания (Row или Statement)
    /// </summary>
    public TriggerLevel Level { get; init; } = TriggerLevel.Row;
    
    /// <summary>
    /// Условие WHEN для ограничения срабатывания триггера
    /// </summary>
    public string? WhenCondition { get; init; }
    
    /// <summary>
    /// Список колонок для UPDATE OF (только для UPDATE триггеров)
    /// </summary>
    public IReadOnlyList<string>? UpdateColumns { get; init; }
}