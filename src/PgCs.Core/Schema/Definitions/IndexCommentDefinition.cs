using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение комментария к индексу (COMMENT ON INDEX)
/// </summary>
public sealed record IndexCommentDefinition : DefinitionBase
{
    /// <summary>
    /// Имя индекса, к которому относится комментарий
    /// </summary>
    public required string IndexName { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL (COMMENT ON INDEX)
    /// </summary>
    public required string Comment { get; init; }
}