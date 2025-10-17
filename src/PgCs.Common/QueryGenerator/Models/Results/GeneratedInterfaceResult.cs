using PgCs.Common.CodeGeneration;

namespace PgCs.Common.QueryGenerator.Models.Results;

/// <summary>
/// Результат генерации интерфейса
/// </summary>
public sealed record GeneratedInterfaceResult
{
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Имя сгенерированного интерфейса
    /// </summary>
    public required string InterfaceName { get; init; }

    /// <summary>
    /// Сгенерированный файл с интерфейсом
    /// </summary>
    public required GeneratedFile File { get; init; }

    /// <summary>
    /// Количество методов в интерфейсе
    /// </summary>
    public int MethodCount { get; init; }

    /// <summary>
    /// Проблемы валидации и предупреждения
    /// </summary>
    public IReadOnlyList<ValidationIssue>? ValidationIssues { get; init; }
}
