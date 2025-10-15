using PgCs.Common.CodeGeneration.Models;
using PgCs.Common.CodeGeneration.Schema.Models;
using PgCs.Common.SchemaAnalyzer.Models;

namespace PgCs.Common.CodeGeneration.Schema;

/// <summary>
/// Генератор моделей на основе схемы базы данных PostgreSQL
/// </summary>
public interface ISchemaGenerator
{
    /// <summary>
    /// Генерирует модели для всех таблиц и представлений из схемы
    /// </summary>
    /// <param name="schema">Метаданные схемы базы данных</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированные файлы с моделями</returns>
    ValueTask<IReadOnlyList<GeneratedFile>> GenerateModelsAsync(
        SchemaMetadata schema,
        ModelGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует модель для конкретной таблицы
    /// </summary>
    /// <param name="table">Определение таблицы</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированная модель</returns>
    ValueTask<GeneratedModel> GenerateTableModelAsync(
        TableDefinition table,
        ModelGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует модель для представления
    /// </summary>
    /// <param name="view">Определение представления</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированная модель</returns>
    ValueTask<GeneratedModel> GenerateViewModelAsync(
        ViewDefinition view,
        ModelGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует перечисления для пользовательских типов
    /// </summary>
    /// <param name="types">Пользовательские типы из схемы</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированные перечисления</returns>
    ValueTask<IReadOnlyList<GeneratedEnum>> GenerateEnumsAsync(
        IReadOnlyList<TypeDefinition> types,
        ModelGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует класс контекста базы данных (опционально)
    /// </summary>
    /// <param name="schema">Метаданные схемы базы данных</param>
    /// <param name="options">Настройки генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Сгенерированный файл с контекстом</returns>
    ValueTask<GeneratedFile> GenerateDbContextAsync(
        SchemaMetadata schema,
        ModelGenerationOptions options,
        CancellationToken cancellationToken = default);
}