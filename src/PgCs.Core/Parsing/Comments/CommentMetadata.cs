namespace PgCs.Core.Parsing.Comments;

/// <summary>
/// Метаданные комментария с служебными полями
/// Используется для извлечения структурированной информации из inline-комментариев
/// </summary>
/// <remarks>
/// Поддерживаемые поля:
/// - comment: текстовое описание
/// - to_type: переопределение типа данных
/// - to_name: переименование поля/параметра
/// 
/// Примеры комментариев:
/// <code>
/// -- comment: User ID; to_type: BIGINT; to_name: UserId;
/// -- comment(User ID); to_type(BIGINT); to_name(UserId);
/// </code>
/// </remarks>
public sealed record CommentMetadata
{
    /// <summary>
    /// Текстовый комментарий (из поля comment: или весь текст если служебных полей нет)
    /// </summary>
    public string Comment { get; init; } = string.Empty;

    /// <summary>
    /// Переопределенный тип данных (из поля to_type:)
    /// </summary>
    public string? ToDataType { get; init; }

    /// <summary>
    /// Переименованное имя (из поля to_name:)
    /// </summary>
    public string? ToName { get; init; }

    /// <summary>
    /// Пустые метаданные (используется как default значение)
    /// </summary>
    public static CommentMetadata Empty { get; } = new();
}
