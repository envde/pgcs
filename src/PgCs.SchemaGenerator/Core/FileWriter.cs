using System.Text;
using PgCs.Common.Generation.Models;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Core;

/// <summary>
/// Реализация записи моделей в файловую систему
/// </summary>
internal sealed class FileWriter : IFileWriter
{
    /// <inheritdoc />
    public async ValueTask WriteModelsAsync(IReadOnlyList<GeneratedModel> models, SchemaGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(models);
        ArgumentNullException.ThrowIfNull(options);

        // Создаём выходную директорию если не существует
        Directory.CreateDirectory(options.OutputDirectory);

        foreach (var model in models)
        {
            var filePath = GetFilePath(model, options);
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Проверяем, нужно ли перезаписывать файл
            if (!options.OverwriteExistingFiles && File.Exists(filePath))
            {
                continue;
            }

            await File.WriteAllTextAsync(filePath, model.SourceCode, Encoding.UTF8);

            // Обновляем путь в модели (это не изменяет исходный record, но для информации)
            // В идеале нужно вернуть обновлённую модель, но это усложнит архитектуру
        }
    }

    /// <summary>
    /// Определяет путь к файлу для модели
    /// </summary>
    private static string GetFilePath(GeneratedModel model, SchemaGenerationOptions options)
    {
        if (!string.IsNullOrEmpty(model.FilePath))
        {
            return model.FilePath;
        }

        var fileName = $"{model.Name}.cs";
        
        // Если используется режим "один файл на модель", группируем по типам
        if (options.OneFilePerModel)
        {
            var subfolder = model.ModelType switch
            {
                ModelType.Table => "Tables",
                ModelType.View => "Views",
                ModelType.Enum => "Enums",
                ModelType.CustomType => "Types",
                ModelType.FunctionParameters => "Functions",
                _ => "Models"
            };

            return Path.Combine(options.OutputDirectory, subfolder, fileName);
        }

        return Path.Combine(options.OutputDirectory, fileName);
    }
}
