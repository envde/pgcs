namespace PgCs.Common.QueryAnalyzer.Models.Results;

/// <summary>
/// Информация о возвращаемом типе
/// </summary>
public record ReturnTypeInfo
{
    /// <summary>
    /// Имя модели (класса) для возврата
    /// </summary>
    public required string ModelName { get; init; }

    /// <summary>
    /// Колонки, которые возвращает запрос
    /// </summary>
    public required IReadOnlyList<ReturnColumn> Columns { get; init; }

    /// <summary>
    /// Требуется ли создать анонимную модель (если не совпадает с существующей)
    /// </summary>
    public bool RequiresCustomModel { get; init; }
}
