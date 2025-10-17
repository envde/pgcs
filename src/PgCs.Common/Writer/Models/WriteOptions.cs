namespace PgCs.Common.Writer.Models;

/// <summary>
/// Опции записи сгенерированных файлов
/// </summary>
public sealed record WriteOptions
{
    /// <summary>
    /// Базовый путь для записи файлов (директория вывода)
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Перезаписывать ли существующие файлы
    /// </summary>
    public bool OverwriteExisting { get; init; } = true;

    /// <summary>
    /// Создавать ли директории, если они не существуют
    /// </summary>
    public bool CreateDirectories { get; init; } = true;

    /// <summary>
    /// Создавать ли резервные копии перед перезаписью
    /// </summary>
    public bool CreateBackups { get; init; } = false;

    /// <summary>
    /// Путь для резервных копий (если CreateBackups = true)
    /// </summary>
    public string? BackupPath { get; init; }

    /// <summary>
    /// Использовать ли относительные пути из GeneratedFile.FilePath
    /// или создавать структуру на основе TypeName и Namespace
    /// </summary>
    public bool UseRelativePaths { get; init; } = true;

    /// <summary>
    /// Кодировка файлов (по умолчанию UTF-8 без BOM)
    /// </summary>
    public string Encoding { get; init; } = "UTF-8";

    /// <summary>
    /// Выполнять ли "сухой запуск" (проверка без записи)
    /// </summary>
    public bool DryRun { get; init; } = false;
}
