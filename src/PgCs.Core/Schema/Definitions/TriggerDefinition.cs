using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение триггера базы данных (CREATE TRIGGER)
/// Триггер автоматически выполняет функцию при наступлении определённых событий
/// </summary>
public sealed record TriggerDefinition: DefinitionBase
{
    /// <summary>
    /// Имя триггера
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Имя таблицы или представления, на которое установлен триггер
    /// </summary>
    public required string TableName { get; init; }
    
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