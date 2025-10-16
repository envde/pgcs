namespace PgCs.Common.QueryGenerator.Models;

/// <summary>
/// Результат валидации сгенерированного кода
/// </summary>
public sealed record ValidationResult
{
    /// <summary>
    /// Успешна ли валидация
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Ошибки валидации
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; init; } = Array.Empty<ValidationError>();

    /// <summary>
    /// Предупреждения
    /// </summary>
    public IReadOnlyList<ValidationWarning> Warnings { get; init; } = Array.Empty<ValidationWarning>();

    /// <summary>
    /// Информационные сообщения
    /// </summary>
    public IReadOnlyList<string> InfoMessages { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Время валидации
    /// </summary>
    public TimeSpan ValidationTime { get; init; }

    /// <summary>
    /// Создаёт успешный результат валидации
    /// </summary>
    public static ValidationResult Success() => new()
    {
        IsValid = true
    };

    /// <summary>
    /// Создаёт неуспешный результат валидации с ошибками
    /// </summary>
    public static ValidationResult Failure(params ValidationError[] errors) => new()
    {
        IsValid = false,
        Errors = errors
    };
}

/// <summary>
/// Ошибка валидации
/// </summary>
public sealed record ValidationError
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Номер строки в коде (если применимо)
    /// </summary>
    public int? LineNumber { get; init; }

    /// <summary>
    /// Номер колонки в коде (если применимо)
    /// </summary>
    public int? ColumnNumber { get; init; }

    /// <summary>
    /// Фрагмент кода с ошибкой
    /// </summary>
    public string? CodeFragment { get; init; }

    /// <summary>
    /// Серьёзность ошибки
    /// </summary>
    public ErrorSeverity Severity { get; init; } = ErrorSeverity.Error;
}

/// <summary>
/// Предупреждение валидации
/// </summary>
public sealed record ValidationWarning
{
    /// <summary>
    /// Код предупреждения
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Сообщение предупреждения
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Номер строки в коде (если применимо)
    /// </summary>
    public int? LineNumber { get; init; }

    /// <summary>
    /// Рекомендация по исправлению
    /// </summary>
    public string? Recommendation { get; init; }
}

/// <summary>
/// Серьёзность ошибки
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Информация
    /// </summary>
    Info,

    /// <summary>
    /// Предупреждение
    /// </summary>
    Warning,

    /// <summary>
    /// Ошибка
    /// </summary>
    Error,

    /// <summary>
    /// Критическая ошибка
    /// </summary>
    Critical
}
