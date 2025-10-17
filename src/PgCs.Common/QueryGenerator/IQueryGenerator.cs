using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;

namespace PgCs.Common.QueryGenerator;

/// <summary>
/// Генератор C# методов для SQL запросов
/// </summary>
public interface IQueryGenerator
{
    /// <summary>
    /// Генерирует C# класс с методами для всех проанализированных запросов
    /// </summary>
    /// <param name="queries">Список метаданных запросов</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результаты генерации с информацией о созданных файлах</returns>
    ValueTask<QueryGenerationResult> GenerateAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options);

    /// <summary>
    /// Генерирует отдельный C# метод для одного запроса
    /// </summary>
    /// <param name="queryMetadata">Метаданные запроса</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результат генерации метода</returns>
    ValueTask<GeneratedMethodResult> GenerateMethodAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options);

    /// <summary>
    /// Генерирует модель результата запроса (DTO) на основе возвращаемых колонок
    /// </summary>
    /// <param name="queryMetadata">Метаданные запроса</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результат генерации модели</returns>
    ValueTask<GeneratedModelResult> GenerateResultModelAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options);

    /// <summary>
    /// Генерирует модель параметров запроса (если требуется сложная модель)
    /// </summary>
    /// <param name="queryMetadata">Метаданные запроса</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результат генерации модели параметров</returns>
    ValueTask<GeneratedModelResult> GenerateParameterModelAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options);

    /// <summary>
    /// Генерирует интерфейс репозитория с методами запросов
    /// </summary>
    /// <param name="queries">Список метаданных запросов</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результат генерации интерфейса</returns>
    ValueTask<GeneratedInterfaceResult> GenerateRepositoryInterfaceAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options);

    /// <summary>
    /// Генерирует реализацию репозитория с методами запросов
    /// </summary>
    /// <param name="queries">Список метаданных запросов</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результат генерации класса репозитория</returns>
    ValueTask<GeneratedClassResult> GenerateRepositoryImplementationAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options);

    /// <summary>
    /// Проверяет корректность метаданных запросов перед генерацией
    /// </summary>
    /// <param name="queries">Список метаданных запросов</param>
    /// <returns>Список предупреждений и ошибок валидации</returns>
    IReadOnlyList<ValidationIssue> ValidateQueries(IReadOnlyList<QueryMetadata> queries);

    /// <summary>
    /// Форматирует сгенерированный C# код с использованием Roslyn
    /// </summary>
    /// <param name="sourceCode">Исходный код для форматирования</param>
    /// <returns>Отформатированный код</returns>
    ValueTask<string> FormatCodeAsync(string sourceCode);
}
