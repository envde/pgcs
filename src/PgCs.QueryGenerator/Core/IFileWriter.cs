using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Core;

/// <summary>
/// Интерфейс для записи сгенерированных файлов
/// </summary>
internal interface IFileWriter
{
    /// <summary>
    /// Записывает сгенерированный класс в файл
    /// </summary>
    ValueTask WriteClassAsync(GeneratedClass generatedClass, QueryGenerationOptions options);

    /// <summary>
    /// Записывает сгенерированные модели в файлы
    /// </summary>
    ValueTask WriteModelsAsync(IReadOnlyList<GeneratedModel> models, QueryGenerationOptions options);
}
