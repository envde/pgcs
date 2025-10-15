using PgCs.Common.CodeGeneration.Models;
using PgCs.Common.CodeGeneration.Query.Models;
using PgCs.Common.QueryAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models;

namespace PgCs.Common.CodeGeneration.Query;

/// <summary>
/// Генератор методов для выполнения SQL запросов через Npgsql
/// </summary>
public interface IQueryGenerator
{
    /// <summary>
    /// Генерирует методы для всех запросов
    /// </summary>
    /// <param name="queries">Метаданные запросов</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="schema">Схема базы данных (опционально, для валидации типов)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированные файлы с методами</returns>
    ValueTask<IReadOnlyList<GeneratedFile>> GenerateMethodsAsync(
        IReadOnlyList<QueryMetadata> queries,
        MethodGenerationOptions options,
        SchemaMetadata? schema = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует метод для конкретного запроса
    /// </summary>
    /// <param name="query">Метаданные запроса</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированный метод</returns>
    ValueTask<GeneratedMethod> GenerateQueryMethodAsync(
        QueryMetadata query,
        MethodGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует модели для результатов запросов (если требуется)
    /// </summary>
    /// <param name="queries">Метаданные запросов</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированные модели результатов</returns>
    ValueTask<IReadOnlyList<GeneratedModel>> GenerateResultModelsAsync(
        IReadOnlyList<QueryMetadata> queries,
        MethodGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует репозиторий с методами запросов
    /// </summary>
    /// <param name="repositoryName">Имя класса репозитория</param>
    /// <param name="queries">Метаданные запросов</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированный файл репозитория</returns>
    ValueTask<GeneratedFile> GenerateRepositoryAsync(
        string repositoryName,
        IReadOnlyList<QueryMetadata> queries,
        MethodGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует интерфейс репозитория
    /// </summary>
    /// <param name="interfaceName">Имя интерфейса</param>
    /// <param name="queries">Метаданные запросов</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированный файл интерфейса</returns>
    ValueTask<GeneratedFile> GenerateRepositoryInterfaceAsync(
        string interfaceName,
        IReadOnlyList<QueryMetadata> queries,
        MethodGenerationOptions options,
        CancellationToken cancellationToken = default);
}