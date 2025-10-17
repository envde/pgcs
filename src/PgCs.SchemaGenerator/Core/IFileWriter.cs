using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Core;

/// <summary>
/// Интерфейс для записи сгенерированных моделей в файлы
/// </summary>
internal interface IFileWriter
{
    /// <summary>
    /// Записывает сгенерированные модели в файлы
    /// </summary>
    /// <param name="models">Список моделей для записи</param>
    /// <param name="options">Опции генерации</param>
    ValueTask WriteModelsAsync(IReadOnlyList<GeneratedModel> models, SchemaGenerationOptions options);
}
