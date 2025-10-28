using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение комментария к таблице (COMMENT ON TABLE)
/// </summary>
public sealed record TableCommentDefinition : DefinitionBase
{
    /// <summary>
    /// Имя таблицы, к которой относится комментарий
    /// </summary>
    public required string TableName { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL (COMMENT ON TABLE)
    /// </summary>
    public required string Comment { get; init; }
}