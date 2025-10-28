using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение комментария к колонке таблицы (COMMENT ON COLUMN)
/// </summary>
public sealed record ColumnCommentDefinition : DefinitionBase
{
    /// <summary>
    /// Имя таблицы, к колонке которой относится комментарий
    /// </summary>
    public required string TableName { get; init; }
    
    /// <summary>
    /// Имя колонки, к которой относится комментарий
    /// </summary>
    public required string ColumnName { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL (COMMENT ON COLUMN)
    /// </summary>
    public required string Comment { get; init; }
}