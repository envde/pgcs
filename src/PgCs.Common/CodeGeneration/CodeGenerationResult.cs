namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Базовый результат генерации кода
/// </summary>
public abstract record CodeGenerationResult
{
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Весь сгенерированный код
    /// </summary>
    public required IReadOnlyList<GeneratedCode> GeneratedCode { get; init; }

    /// <summary>
    /// Проблемы валидации и предупреждения
    /// </summary>
    public required IReadOnlyList<ValidationIssue> ValidationIssues { get; init; }

    /// <summary>
    /// Общее время генерации
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Время завершения генерации
    /// </summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Есть ли критические ошибки
    /// </summary>
    public bool HasErrors => ValidationIssues.HasErrors();

    /// <summary>
    /// Есть ли предупреждения
    /// </summary>
    public bool HasWarnings => ValidationIssues.HasWarnings();

    /// <summary>
    /// Количество ошибок
    /// </summary>
    public int ErrorCount => ValidationIssues.CountErrors();

    /// <summary>
    /// Количество предупреждений
    /// </summary>
    public int WarningCount => ValidationIssues.CountWarnings();
}
