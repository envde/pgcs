using PgCs.Common.Generation.Models;

namespace PgCs.Common.Core;

/// <summary>
/// Файл с генерируемой моделью
/// </summary>
public sealed record GeneratedModelFile
{
    /// <summary>
    /// Имя файла (без расширения)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Сгенерированный исходный код
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Путь к файлу (если известен)
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Тип модели для определения подпапки
    /// </summary>
    public ModelType ModelType { get; init; }

    /// <summary>
    /// Namespace модели
    /// </summary>
    public required string Namespace { get; init; }
}
