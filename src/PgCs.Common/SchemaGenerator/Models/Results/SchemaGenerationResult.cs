using PgCs.Common.CodeGeneration;

namespace PgCs.Common.SchemaGenerator.Models.Results;

/// <summary>
/// Результат генерации схемы базы данных
/// </summary>
public sealed record SchemaGenerationResult : CodeGenerationResult
{
    /// <summary>
    /// Сгенерированные модели таблиц
    /// </summary>
    public IReadOnlyList<GeneratedCode> TableModels { get; init; } = [];

    /// <summary>
    /// Сгенерированные модели представлений
    /// </summary>
    public IReadOnlyList<GeneratedCode> ViewModels { get; init; } = [];

    /// <summary>
    /// Результаты генерации пользовательских типов
    /// </summary>
    public IReadOnlyList<GeneratedCode> CustomTypes { get; init; } = [];

    /// <summary>
    /// Сгенерированные методы для функций
    /// </summary>
    public IReadOnlyList<GeneratedCode> Functions { get; init; } = [];

    /// <summary>
    /// Статистика генерации
    /// </summary>
    public GenerationStatistics? Statistics { get; init; }
}
