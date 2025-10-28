using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение COMMENT для CONSTRAINT
/// </summary>
public sealed record ConstraintCommentDefinition : DefinitionBase
{
    public required string ConstraintName { get; init; }
    
    /// <summary>
    /// Комментарий PostgreSQL (COMMENT ON CONSTRAINT)
    /// </summary>
    public required string Comment { get; set; }
}