using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение комментария к триггеру (COMMENT ON TRIGGER)
/// </summary>
public sealed record TriggerCommentDefinition : DefinitionBase
{
    /// <summary>
    /// Имя триггера, к которому относится комментарий
    /// </summary>
    public required string TriggerName { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL (COMMENT ON TRIGGER)
    /// </summary>
    public required string Comment { get; init; }
}