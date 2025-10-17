using PgCs.Common.CodeGeneration;

namespace PgCs.Common.SchemaGenerator.Models.Results;

/// <summary>
/// Результат генерации методов для функций базы данных
/// </summary>
public sealed record GeneratedFunctionsResult
{
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Сгенерированные файлы с методами функций
    /// </summary>
    public required IReadOnlyList<GeneratedFile> Files { get; init; }

    /// <summary>
    /// Количество сгенерированных методов
    /// </summary>
    public int FunctionCount => Files.Count;

    /// <summary>
    /// Проблемы валидации и предупреждения
    /// </summary>
    public IReadOnlyList<ValidationIssue>? ValidationIssues { get; init; }

    /// <summary>
    /// Время генерации
    /// </summary>
    public TimeSpan Duration { get; init; }
}
