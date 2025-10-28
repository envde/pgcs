using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение COMMENT для TYPE
/// </summary>
public sealed record CompositeTypeCommentDefinition : DefinitionBase
{
    public required string CompositeTypeName { get; init; }
    
    /// <summary>
    /// Комментарий PostgreSQL (COMMENT ON TYPE)
    /// </summary>
    public required string Comment { get; set; }
}