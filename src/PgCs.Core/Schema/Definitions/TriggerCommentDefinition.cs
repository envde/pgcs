using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение COMMENT для TRIGGER
/// </summary>
public sealed record TriggerCommentDefinition : DefinitionBase
{
    public required string TriggerName { get; init; }
    /// <summary>
    /// Комментарий PostgreSQL (COMMENT ON TRIGGER)
    /// </summary>
    public required string Comment { get; set; }
}