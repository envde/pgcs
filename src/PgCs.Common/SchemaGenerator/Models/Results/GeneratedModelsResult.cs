using PgCs.Common.CodeGeneration;

namespace PgCs.Common.SchemaGenerator.Models.Results;

/// <summary>
/// Результат генерации моделей (таблиц или представлений)
/// </summary>
public sealed record GeneratedModelsResult
{
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Сгенерированные файлы моделей
    /// </summary>
    public required IReadOnlyList<GeneratedFile> Files { get; init; }

    /// <summary>
    /// Количество сгенерированных моделей
    /// </summary>
    public int ModelCount => Files.Count;

    /// <summary>
    /// Проблемы валидации и предупреждения
    /// </summary>
    public IReadOnlyList<ValidationIssue>? ValidationIssues { get; init; }

    /// <summary>
    /// Время генерации
    /// </summary>
    public TimeSpan Duration { get; init; }
}
