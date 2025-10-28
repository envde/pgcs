using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение COMMENT для INDEX
/// </summary>
public sealed record IndexCommentDefinition : DefinitionBase
{
    public required string IndexName { get; init; }
    
    /// <summary>
    /// Комментарий PostgreSQL (COMMENT ON INDEX)
    /// </summary>
    public required string Comment { get; set; }
}