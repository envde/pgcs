using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.Common.QueryGenerator;

/// <summary>
/// Интерфейс генератора методов для выполнения SQL запросов
/// </summary>
public interface IQueryGenerator
{
    /// <summary>
    /// Генерирует C# методы на основе проанализированных SQL запросов
    /// </summary>
    /// <param name="queries">Список метаданных запросов</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результат генерации с информацией о созданных файлах</returns>
    ValueTask<QueryGenerationResult> GenerateAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions? options = null);

    /// <summary>
    /// Генерирует метод для одного запроса
    /// </summary>
    /// <param name="queryMetadata">Метаданные запроса</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Сгенерированный метод</returns>
    ValueTask<GeneratedMethod> GenerateMethodAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions? options = null);

    /// <summary>
    /// Генерирует класс с методами запросов
    /// </summary>
    /// <param name="queries">Список метаданных запросов</param>
    /// <param name="className">Имя класса</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Сгенерированный класс</returns>
    ValueTask<GeneratedClass> GenerateQueryClassAsync(
        IReadOnlyList<QueryMetadata> queries,
        string className,
        QueryGenerationOptions? options = null);

    /// <summary>
    /// Генерирует модели результатов для запросов (если требуется)
    /// </summary>
    /// <param name="queries">Список метаданных запросов</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированных моделей результатов</returns>
    ValueTask<IReadOnlyList<GeneratedModel>> GenerateResultModelsAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions? options = null);

    /// <summary>
    /// Генерирует модели параметров для запросов (если требуется)
    /// </summary>
    /// <param name="queries">Список метаданных запросов</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированных моделей параметров</returns>
    ValueTask<IReadOnlyList<GeneratedModel>> GenerateParameterModelsAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions? options = null);

    /// <summary>
    /// Проверяет корректность генерируемого кода
    /// </summary>
    /// <param name="generatedCode">Сгенерированный код</param>
    /// <returns>Результат валидации</returns>
    ValueTask<ValidationResult> ValidateGeneratedCodeAsync(string generatedCode);
}
