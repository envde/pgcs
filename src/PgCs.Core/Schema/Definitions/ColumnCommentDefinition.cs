using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определяет COMMENT для COLUMN
/// </summary>
public sealed record ColumnCommentDefinition : DefinitionBase
{
    /// <summary>
    /// Название таблицы
    /// </summary>
    public required string TableName { get; init; }
    /// <summary>
    /// Название колонки
    /// </summary>
    public required string ColumnName { get; set; }
    /// <summary>
    /// Комментарий PostgreSQL (COMMENT ON COLUMN)
    /// </summary>
    public required string Comment { get; set; }
}