using PgCs.Common.CodeGeneration;

namespace PgCs.Common.SchemaGenerator.Models.Results;

/// <summary>
/// Результат генерации пользовательских типов (ENUM, DOMAIN, COMPOSITE)
/// </summary>
public sealed record GeneratedTypesResult
{
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Сгенерированные файлы типов
    /// </summary>
    public required IReadOnlyList<GeneratedFile> Files { get; init; }

    /// <summary>
    /// Количество сгенерированных ENUM типов
    /// </summary>
    public int EnumCount { get; init; }

    /// <summary>
    /// Количество сгенерированных DOMAIN типов
    /// </summary>
    public int DomainCount { get; init; }

    /// <summary>
    /// Количество сгенерированных COMPOSITE типов
    /// </summary>
    public int CompositeCount { get; init; }

    /// <summary>
    /// Проблемы валидации и предупреждения
    /// </summary>
    public IReadOnlyList<ValidationIssue>? ValidationIssues { get; init; }

    /// <summary>
    /// Время генерации
    /// </summary>
    public TimeSpan Duration { get; init; }
}
