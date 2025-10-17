using PgCs.Common.CodeGeneration;

namespace PgCs.Common.QueryGenerator.Models.Results;

/// <summary>
/// Результат генерации методов для SQL запросов
/// </summary>
public sealed record QueryGenerationResult : CodeGenerationResult
{
    /// <summary>
    /// Результат генерации интерфейса репозитория
    /// </summary>
    public GeneratedInterfaceResult? RepositoryInterface { get; init; }

    /// <summary>
    /// Результат генерации реализации репозитория
    /// </summary>
    public GeneratedClassResult? RepositoryImplementation { get; init; }

    /// <summary>
    /// Результаты генерации отдельных методов
    /// </summary>
    public required IReadOnlyList<GeneratedMethodResult> Methods { get; init; }

    /// <summary>
    /// Результаты генерации моделей результатов
    /// </summary>
    public IReadOnlyList<GeneratedModelResult>? ResultModels { get; init; }

    /// <summary>
    /// Результаты генерации моделей параметров
    /// </summary>
    public IReadOnlyList<GeneratedModelResult>? ParameterModels { get; init; }

    /// <summary>
    /// Статистика генерации
    /// </summary>
    public QueryGenerationStatistics? Statistics { get; init; }
}
