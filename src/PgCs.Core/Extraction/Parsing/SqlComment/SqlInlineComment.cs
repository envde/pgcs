namespace PgCs.Core.Extraction.Parsing.SqlComment;

/// <summary>
/// Результат парсинга inline-комментария
/// </summary>
public sealed record SqlInlineComment
{
    /// <summary>
    /// Комментарий к колонке
    /// </summary>
    public string? Comment { get; init; }
        
    /// <summary>
    /// Тип данных колонки
    /// </summary>
    public string? ToDateType { get; init; }
        
    /// <summary>
    /// Переименованное имя колонки
    /// </summary>
    public string? ToName { get; init; }
}