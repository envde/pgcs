namespace PgCs.Core.Extraction.Block;

/// <summary>
/// Inline комментарий
/// </summary>
public sealed record InlineComment
{
    /// <summary>
    /// Ключ, к которому относится комментарий (поле, параметр запроса и т.д)
    /// </summary>
    public required string Key { get; init; }
    /// <summary>
    /// Комментарий
    /// </summary>
    public required string Comment { get; init; }
    /// <summary>
    /// Позиция комментария в строке
    /// </summary>
    public required int Position { get; init; }
}