using PgCs.Common.CodeGeneration;

namespace PgCs.Common.QueryGenerator.Models.Results;

/// <summary>
/// Результат генерации модели (результата или параметров запроса)
/// </summary>
public sealed record GeneratedModelResult
{
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Имя сгенерированной модели
    /// </summary>
    public required string ModelName { get; init; }

    /// <summary>
    /// Сгенерированный файл с моделью
    /// </summary>
    public required GeneratedFile File { get; init; }

    /// <summary>
    /// Количество свойств в модели
    /// </summary>
    public int PropertyCount { get; init; }

    /// <summary>
    /// Проблемы валидации и предупреждения
    /// </summary>
    public IReadOnlyList<ValidationIssue>? ValidationIssues { get; init; }
}
