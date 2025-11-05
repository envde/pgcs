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
    public string? DataType { get; init; }
        
    /// <summary>
    /// Переименованное имя колонки
    /// </summary>
    public string? RenameTo { get; init; }
}