namespace PgCs.Core.Parser.Metadata;

/// <summary>
/// Метаданные из SQL комментария
/// </summary>
/// <remarks>
/// Парсит служебные поля из комментариев в формате:
/// - field: value (разделитель двоеточие)
/// - field(value) (формат со скобками)
/// 
/// Примеры:
/// <code>
/// -- comment: User identifier; to_name: UserId; to_type: long
/// -- comment(User identifier); to_name(UserId); to_type(long)
/// </code>
/// </remarks>
public sealed record CommentMetadata
{
    /// <summary>
    /// Текст комментария (значение поля "comment")
    /// </summary>
    /// <example>User identifier</example>
    public string? Comment { get; init; }

    /// <summary>
    /// Целевое имя для кодогенерации (значение поля "to_name")
    /// </summary>
    /// <example>UserId, OrderNumber, ProductTitle</example>
    public string? ToName { get; init; }

    /// <summary>
    /// Целевой тип данных для кодогенерации (значение поля "to_type")
    /// </summary>
    /// <example>string, long, DateTime, decimal</example>
    public string? ToType { get; init; }

    /// <summary>
    /// Дополнительные пользовательские поля
    /// </summary>
    /// <remarks>
    /// Позволяет расширять метаданные произвольными полями
    /// без изменения структуры класса
    /// </remarks>
    public IReadOnlyDictionary<string, string>? CustomFields { get; init; }

    /// <summary>
    /// Пустые метаданные (нет служебных полей)
    /// </summary>
    public static CommentMetadata Empty { get; } = new();

    /// <summary>
    /// Проверяет, содержит ли метаданные какую-либо информацию
    /// </summary>
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Comment) &&
        string.IsNullOrWhiteSpace(ToName) &&
        string.IsNullOrWhiteSpace(ToType) &&
        (CustomFields == null || CustomFields.Count == 0);
}
