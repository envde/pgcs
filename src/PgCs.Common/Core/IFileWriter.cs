using PgCs.Common.Generation.Models;

namespace PgCs.Common.Core;

/// <summary>
/// Интерфейс для записи сгенерированных файлов
/// </summary>
public interface IFileWriter
{
    /// <summary>
    /// Записывает модели в файловую систему
    /// </summary>
    ValueTask WriteModelsAsync(IReadOnlyList<GeneratedModelFile> models, GenerationOptions options);
}
