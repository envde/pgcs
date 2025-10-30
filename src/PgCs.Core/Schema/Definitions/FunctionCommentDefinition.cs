using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение комментария к функции (COMMENT ON FUNCTION)
/// </summary>
public sealed record FunctionCommentDefinition : DefinitionBase
{
    /// <summary>
    /// Имя функции, к которой относится комментарий
    /// </summary>
    public required string FunctionName { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL (COMMENT ON FUNCTION)
    /// </summary>
    public required string Comment { get; init; }
}