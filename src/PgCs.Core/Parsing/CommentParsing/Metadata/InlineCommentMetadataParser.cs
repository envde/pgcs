namespace PgCs.Core.Parsing.CommentParsing.Metadata;

/// <summary>
/// Парсер метаданных из inline-комментариев со служебными словами
/// </summary>
/// <remarks>
/// Координирует работу экстракторов полей для извлечения метаданных из комментария.
/// Поддерживает два формата служебных слов:
/// - Формат с двоеточием: comment: текст; to_type: тип; to_name: имя;
/// - Формат со скобками: comment(текст); to_type(тип); to_name(имя);
/// 
/// Служебные слова могут идти в любом порядке и не обязательно присутствовать все.
/// Если служебных слов нет, весь текст считается комментарием.
/// </remarks>
public sealed class InlineCommentMetadataParser
{
    private readonly CommentFieldExtractor _commentExtractor = new();
    private readonly TypeFieldExtractor _typeExtractor = new();
    private readonly NameFieldExtractor _nameExtractor = new();

    /// <summary>
    /// Парсит inline-комментарий и извлекает метаданные
    /// </summary>
    /// <param name="comment">Текст комментария (может содержать префикс --)</param>
    /// <returns>Извлеченные метаданные или null если комментарий пустой</returns>
    /// <remarks>
    /// Примеры поддерживаемых форматов:
    /// - "comment: User ID; to_type: BIGINT; to_name: UserId;"
    /// - "comment(User ID); to_type(BIGINT); to_name(UserId);"
    /// - "to_type: VARCHAR(100); comment: Email address;"
    /// - "Simple comment without service words"
    /// </remarks>
    public InlineCommentMetadata? Parse(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        // Убираем префикс -- если он есть
        var cleanComment = comment.TrimStart('-').Trim();

        // Извлекаем служебные поля
        var commentText = _commentExtractor.Extract(cleanComment);
        var dataType = _typeExtractor.Extract(cleanComment);
        var renameTo = _nameExtractor.Extract(cleanComment);

        // Если ничего не найдено через служебные слова, считаем весь текст комментарием
        if (commentText is null && dataType is null && renameTo is null)
        {
            return new InlineCommentMetadata
            {
                Comment = cleanComment,
                ToDataType = null,
                ToName = null
            };
        }

        return new InlineCommentMetadata
        {
            Comment = commentText,
            ToDataType = dataType,
            ToName = renameTo
        };
    }
}
