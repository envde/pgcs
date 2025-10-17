using PgCs.Common.CodeGeneration;

namespace PgCs.Common.SchemaGenerator.Models.Results;

/// <summary>
/// Результат генерации схемы базы данных
/// </summary>
public sealed record SchemaGenerationResult : CodeGenerationResult
{
    /// <summary>
    /// Результаты генерации моделей таблиц
    /// </summary>
    public GeneratedModelsResult? TableModels { get; init; }

    /// <summary>
    /// Результаты генерации моделей представлений
    /// </summary>
    public GeneratedModelsResult? ViewModels { get; init; }

    /// <summary>
    /// Результаты генерации пользовательских типов
    /// </summary>
    public GeneratedTypesResult? CustomTypes { get; init; }

    /// <summary>
    /// Результаты генерации методов для функций
    /// </summary>
    public GeneratedFunctionsResult? Functions { get; init; }

    /// <summary>
    /// Статистика генерации
    /// </summary>
    public GenerationStatistics? Statistics { get; init; }
}
