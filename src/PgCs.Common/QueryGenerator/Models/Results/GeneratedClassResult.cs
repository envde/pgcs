using PgCs.Common.CodeGeneration;

namespace PgCs.Common.QueryGenerator.Models.Results;

/// <summary>
/// Результат генерации класса
/// </summary>
public sealed record GeneratedClassResult
{
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Имя сгенерированного класса
    /// </summary>
    public required string ClassName { get; init; }

    /// <summary>
    /// Сгенерированный код класса
    /// </summary>
    public required GeneratedCode Code { get; init; }

    /// <summary>
    /// Количество методов в классе
    /// </summary>
    public int MethodCount { get; init; }

    /// <summary>
    /// Проблемы валидации и предупреждения
    /// </summary>
    public IReadOnlyList<ValidationIssue>? ValidationIssues { get; init; }
}
