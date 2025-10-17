namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Информация о сгенерированном файле
/// </summary>
public sealed record GeneratedFile
{
    /// <summary>
    /// Абсолютный путь к сгенерированному файлу
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Сгенерированный исходный код
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Имя основного типа в файле (класс, интерфейс, enum)
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Namespace сгенерированного типа
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long SizeInBytes { get; init; }

    /// <summary>
    /// Время генерации
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Тип сгенерированного файла
    /// </summary>
    public GeneratedFileType FileType { get; init; }
}
