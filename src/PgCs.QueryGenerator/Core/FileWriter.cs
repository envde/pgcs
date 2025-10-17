using System.Text;
using PgCs.Common.Generation.Models;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Core;

/// <summary>
/// Реализация записи файлов для сгенерированного кода
/// </summary>
internal sealed class FileWriter : IFileWriter
{
    /// <inheritdoc />
    public async ValueTask WriteClassAsync(GeneratedClass generatedClass, QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(generatedClass);
        ArgumentNullException.ThrowIfNull(options);

        Directory.CreateDirectory(options.OutputDirectory);

        var filePath = Path.Combine(options.OutputDirectory, $"{generatedClass.Name}.cs");

        if (!options.OverwriteExistingFiles && File.Exists(filePath))
        {
            return;
        }

        await File.WriteAllTextAsync(filePath, generatedClass.SourceCode, Encoding.UTF8);

        // Генерируем интерфейс, если требуется
        if (options.GenerateInterface && !string.IsNullOrEmpty(generatedClass.InterfaceSourceCode))
        {
            var interfaceFilePath = Path.Combine(
                options.OutputDirectory,
                $"{generatedClass.InterfaceName}.cs");

            await File.WriteAllTextAsync(interfaceFilePath, generatedClass.InterfaceSourceCode, Encoding.UTF8);
        }
    }

    /// <inheritdoc />
    public async ValueTask WriteModelsAsync(IReadOnlyList<GeneratedModel> models, QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(models);
        ArgumentNullException.ThrowIfNull(options);

        if (models.Count == 0)
        {
            return;
        }

        foreach (var model in models)
        {
            var filePath = GetModelFilePath(model, options);
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

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
    private static string GetModelFilePath(GeneratedModel model, QueryGenerationOptions options)
    {
        if (!string.IsNullOrEmpty(model.FilePath))
        {
            return model.FilePath;
        }

        var fileName = $"{model.Name}.cs";

        // Группируем по типам модели
        var subfolder = model.ModelType switch
        {
            ModelType.QueryResult => "Results",
            ModelType.QueryParameters => "Parameters",
            _ => "Models"
        };

        return Path.Combine(options.OutputDirectory, subfolder, fileName);
    }
}
