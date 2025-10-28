using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение комментария к ограничению целостности (COMMENT ON CONSTRAINT)
/// </summary>
public sealed record ConstraintCommentDefinition : DefinitionBase
{
    /// <summary>
    /// Имя ограничения, к которому относится комментарий
    /// </summary>
    public required string ConstraintName { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL (COMMENT ON CONSTRAINT)
    /// </summary>
    public required string Comment { get; init; }
}