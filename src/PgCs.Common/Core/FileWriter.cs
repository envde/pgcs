using System.Text;
using PgCs.Common.Generation.Models;

namespace PgCs.Common.Core;

/// <summary>
/// Реализация записи файлов для сгенерированного кода
/// </summary>
public sealed class FileWriter : IFileWriter
{
    /// <inheritdoc />
    public async ValueTask WriteModelsAsync(IReadOnlyList<GeneratedModelFile> models, GenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(models);
        ArgumentNullException.ThrowIfNull(options);

        if (models.Count == 0)
        {
            return;
        }

        // Создаём выходную директорию
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
        }
    }

    /// <summary>
    /// Определяет путь к файлу для модели
    /// </summary>
    private static string GetFilePath(GeneratedModelFile model, GenerationOptions options)
    {
        if (!string.IsNullOrEmpty(model.FilePath))
        {
            return model.FilePath;
        }

        var fileName = $"{model.Name}.cs";

        // Группируем по типам модели в соответствующие подпапки
        var subfolder = model.ModelType switch
        {
            ModelType.Table => "Tables",
            ModelType.View => "Views",
            ModelType.Enum => "Enums",
            ModelType.CustomType => "Types",
            ModelType.FunctionParameters => "Functions",
            ModelType.QueryResult => "Results",
            ModelType.QueryParameters => "Parameters",
            _ => "Models"
        };

        return Path.Combine(options.OutputDirectory, subfolder, fileName);
    }
}
