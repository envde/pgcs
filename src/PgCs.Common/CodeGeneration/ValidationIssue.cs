namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Проблема валидации схемы или запроса
/// </summary>
public sealed record ValidationIssue
{
    /// <summary>
    /// Уровень серьезности проблемы
    /// </summary>
    public required ValidationSeverity Severity { get; init; }

    /// <summary>
    /// Сообщение о проблеме
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Код проблемы для программной обработки
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Местоположение проблемы (имя таблицы, столбца, запроса и т.д.)
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Дополнительные детали проблемы
    /// </summary>
    public IReadOnlyDictionary<string, string>? Details { get; init; }
}
