using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение комментария в PostgreSQL (COMMENT ON)
/// </summary>
public sealed record CommentDefinition : DefinitionBase
{
    /// <summary>
    /// Имя таблицы, к колонке которой относится комментарий
    /// </summary>
    public string? TableName { get; init; }
    
    /// <summary>
    /// Имя колонки, к которой относится комментарий
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL (COMMENT ON COLUMN)
    /// </summary>
    public required string Comment { get; init; }
}