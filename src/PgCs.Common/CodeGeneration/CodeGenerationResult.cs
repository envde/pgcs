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
    public bool HasErrors => ValidationIssues.Any(i => i.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Есть ли предупреждения
    /// </summary>
    public bool HasWarnings => ValidationIssues.Any(i => i.Severity == ValidationSeverity.Warning);

    /// <summary>
    /// Количество ошибок
    /// </summary>
    public int ErrorCount => ValidationIssues.Count(i => i.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Количество предупреждений
    /// </summary>
    public int WarningCount => ValidationIssues.Count(i => i.Severity == ValidationSeverity.Warning);
}
