using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определяет COMMENT для TABLE
/// </summary>
public sealed record TableCommentDefinition : DefinitionBase
{
    
    public required string TableName { get; init; }
    
    /// <summary>
    /// Комментарий PostgreSQL (COMMENT ON TABLE)
    /// </summary>
    public required string Comment { get; set; }
}