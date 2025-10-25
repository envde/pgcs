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

    /// <summary>
    /// Создает ошибку валидации (Error)
    /// </summary>
    public static ValidationIssue Error(string code, string message, string? location = null, Dictionary<string, string>? details = null)
    {
        return new ValidationIssue
        {
            Severity = ValidationSeverity.Error,
            Code = code,
            Message = message,
            Location = location,
            Details = details
        };
    }

    /// <summary>
    /// Создает предупреждение валидации (Warning)
    /// </summary>
    public static ValidationIssue Warning(string code, string message, string? location = null, Dictionary<string, string>? details = null)
    {
        return new ValidationIssue
        {
            Severity = ValidationSeverity.Warning,
            Code = code,
            Message = message,
            Location = location,
            Details = details
        };
    }

    /// <summary>
    /// Создает информационное сообщение валидации (Info)
    /// </summary>
    public static ValidationIssue Info(string code, string message, string? location = null, Dictionary<string, string>? details = null)
    {
        return new ValidationIssue
        {
            Severity = ValidationSeverity.Info,
            Code = code,
            Message = message,
            Location = location,
            Details = details
        };
    }
}
