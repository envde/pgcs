using PgCs.Common.CodeGeneration.Models.Enums;

namespace PgCs.Common.CodeGeneration.Models;

/// <summary>
/// Сгенерированный файл с кодом
/// </summary>
public sealed record GeneratedFile
{
    /// <summary>
    /// Имя файла (без пути)
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Относительный путь к файлу
    /// </summary>
    public string? RelativePath { get; init; }

    /// <summary>
    /// Содержимое файла
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Тип генерируемого файла
    /// </summary>
    public GeneratedFileType FileType { get; init; }

    /// <summary>
    /// Метаданные о сгенерированном файле
    /// </summary>
    public FileMetadata? Metadata { get; init; }
}