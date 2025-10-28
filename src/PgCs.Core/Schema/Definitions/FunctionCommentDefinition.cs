using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение COMMENT для FUNCTION
/// </summary>
public sealed record FunctionCommentDefinition : DefinitionBase
{
    public required string FunctionName { get; init; }
    
    /// <summary>
    /// Комментарий PostgreSQL (COMMENT ON FUNCTION)
    /// </summary>
    public required string Comment { get; set; }
}