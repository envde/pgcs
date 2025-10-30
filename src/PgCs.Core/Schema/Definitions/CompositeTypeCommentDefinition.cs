using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение комментария к композитному типу (COMMENT ON TYPE)
/// </summary>
public sealed record CompositeTypeCommentDefinition : DefinitionBase
{
    /// <summary>
    /// Имя композитного типа, к которому относится комментарий
    /// </summary>
    public required string CompositeTypeName { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL (COMMENT ON TYPE)
    /// </summary>
    public required string Comment { get; init; }
}