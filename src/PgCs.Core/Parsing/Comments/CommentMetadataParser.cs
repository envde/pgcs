namespace PgCs.Core.Parsing.Comments;

/// <summary>
/// Парсер метаданных комментария
/// Извлекает структурированные данные из inline-комментариев со служебными полями
/// </summary>
/// <remarks>
/// Координирует процесс парсинга:
/// 1. Очищает текст комментария от префиксов (--)
/// 2. Использует CommentFieldScanner для извлечения полей
/// 3. Собирает результат в CommentMetadata
/// 
/// Примеры поддерживаемых форматов:
/// <code>
/// "comment: User ID; to_type: BIGINT; to_name: UserId;"
/// "comment(User ID); to_type(BIGINT); to_name(UserId);"
/// "to_type: VARCHAR(100); comment: Email address;"
/// "Simple comment without service fields"
/// </code>
/// </remarks>
public sealed class CommentMetadataParser
{
    private readonly CommentFieldScanner _fieldScanner = new();

    /// <summary>
    /// Парсит inline-комментарий и извлекает метаданные
    /// </summary>
    /// <param name="commentText">Текст комментария (может содержать префикс --)</param>
    /// <returns>Извлеченные метаданные или Empty если комментарий пустой</returns>
    public CommentMetadata Parse(string? commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
        {
            return CommentMetadata.Empty;
        }

        // Убираем префикс -- если он есть
        var cleanText = commentText.TrimStart('-', ' ', '\t').Trim();

        if (string.IsNullOrWhiteSpace(cleanText))
        {
            return CommentMetadata.Empty;
        }

        // Проверяем, есть ли служебные поля
        if (!_fieldScanner.HasFields(cleanText))
        {
            // Нет служебных полей - весь текст является комментарием
            return new CommentMetadata
            {
                Comment = cleanText
            };
        }

        // Извлекаем служебные поля
        _fieldScanner.TryExtractField(cleanText, "comment", out var comment);
        _fieldScanner.TryExtractField(cleanText, "to_type", out var toDataType);
        _fieldScanner.TryExtractField(cleanText, "to_name", out var toName);

        // Если поле comment не найдено, но есть другие поля - используем весь текст
        if (string.IsNullOrWhiteSpace(comment))
        {
            comment = cleanText;
        }

        return new CommentMetadata
        {
            Comment = comment ?? string.Empty,
            ToDataType = toDataType,
            ToName = toName
        };
    }
}
