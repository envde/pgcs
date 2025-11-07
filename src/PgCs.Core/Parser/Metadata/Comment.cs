namespace PgCs.Core.Parser.Metadata;

/// <summary>
/// Представляет SQL комментарий с возможными служебными метаданными
/// </summary>
/// <remarks>
/// SQL комментарии (начинающиеся с --) могут содержать служебную информацию
/// для кодогенерации, такую как: comment, to_name, to_type и другие.
/// 
/// НЕ путать с PostgreSQL COMMENT ON - это разные концепции:
/// - Comment: служебный комментарий в коде SQL (-- ...)
/// - COMMENT ON: команда PostgreSQL для добавления описания объекта
/// </remarks>
public sealed record Comment
{
    /// <summary>
    /// Чистый текст комментария (без префикса --)
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Служебные метаданные из комментария (если есть)
    /// </summary>
    /// <example>
    /// Для комментария: "-- comment: User ID; to_name: UserId; to_type: string"
    /// Метаданные: { Comment = "User ID", ToName = "UserId", ToType = "string" }
    /// </example>
    public CommentMetadata? Metadata { get; init; }

    /// <summary>
    /// Является ли комментарий header-комментарием (расположен перед объектом)
    /// </summary>
    public bool IsHeader { get; init; }

    /// <summary>
    /// Является ли комментарий inline-комментарием (расположен после элемента)
    /// </summary>
    public bool IsInline => !IsHeader;

    /// <summary>
    /// Номер строки в исходном SQL (если доступен)
    /// </summary>
    public int? LineNumber { get; init; }
}
