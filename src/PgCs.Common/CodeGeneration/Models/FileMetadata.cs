namespace PgCs.Common.CodeGeneration.Models;

/// <summary>
/// Метаданные сгенерированного файла
/// </summary>
public sealed record FileMetadata
{
    /// <summary>
    /// Время генерации
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Версия генератора
    /// </summary>
    public string? GeneratorVersion { get; init; }

    /// <summary>
    /// Исходные файлы, из которых сгенерирован
    /// </summary>
    public IReadOnlyList<string> SourceFiles { get; init; } = [];

    /// <summary>
    /// Хеш для определения изменений (SHA256)
    /// </summary>
    public string? ContentHash { get; init; }

    /// <summary>
    /// Предупреждения при генерации
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
}