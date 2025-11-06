namespace PgCs.Core.Parsing.CommentParsing.Metadata;

/// <summary>
/// Метаданные извлеченные из inline-комментария со служебными словами
/// </summary>
/// <remarks>
/// Inline комментарий может содержать служебные слова для управления генерацией кода:
/// - comment: текстовое описание
/// - to_type: переопределение типа данных
/// - to_name: переименование поля/параметра
/// </remarks>
public sealed record InlineCommentMetadata
{
    /// <summary>
    /// Текстовый комментарий (из служебного слова comment: или весь текст если служебных слов нет)
    /// </summary>
    public string? Comment { get; init; }
        
    /// <summary>
    /// Переопределенный тип данных (из служебного слова to_type:)
    /// </summary>
    public string? ToDataType { get; init; }
        
    /// <summary>
    /// Переименованное имя (из служебного слова to_name:)
    /// </summary>
    public string? ToName { get; init; }
}
