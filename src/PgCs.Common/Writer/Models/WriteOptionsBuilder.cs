namespace PgCs.Common.Writer.Models;

/// <summary>
/// Fluent API builder для настройки опций записи файлов
/// </summary>
public sealed class WriteOptionsBuilder
{
    private string _outputPath = "./Generated";
    private bool _overwriteExisting = true;
    private bool _createDirectories = true;
    private bool _createBackups = false;
    private string? _backupPath;
    private bool _useRelativePaths = true;
    private string _encoding = "UTF-8";
    private bool _dryRun = false;

    /// <summary>
    /// Создаёт новый builder с настройками по умолчанию
    /// </summary>
    public static WriteOptionsBuilder Create() => new();

    /// <summary>
    /// Устанавливает путь для вывода файлов
    /// </summary>
    public WriteOptionsBuilder OutputTo(string path)
    {
        _outputPath = path;
        return this;
    }

    /// <summary>
    /// Перезаписывать существующие файлы
    /// </summary>
    public WriteOptionsBuilder OverwriteExisting()
    {
        _overwriteExisting = true;
        return this;
    }

    /// <summary>
    /// Сохранять существующие файлы (не перезаписывать)
    /// </summary>
    public WriteOptionsBuilder PreserveExisting()
    {
        _overwriteExisting = false;
        return this;
    }

    /// <summary>
    /// Создавать директории если не существуют
    /// </summary>
    public WriteOptionsBuilder CreateDirectories()
    {
        _createDirectories = true;
        return this;
    }

    /// <summary>
    /// Не создавать директории автоматически
    /// </summary>
    public WriteOptionsBuilder DontCreateDirectories()
    {
        _createDirectories = false;
        return this;
    }

    /// <summary>
    /// Создавать резервные копии перед перезаписью
    /// </summary>
    /// <param name="backupPath">Путь для backup файлов (необязательно)</param>
    public WriteOptionsBuilder WithBackups(string? backupPath = null)
    {
        _createBackups = true;
        _backupPath = backupPath;
        return this;
    }

    /// <summary>
    /// Не создавать резервные копии
    /// </summary>
    public WriteOptionsBuilder WithoutBackups()
    {
        _createBackups = false;
        _backupPath = null;
        return this;
    }

    /// <summary>
    /// Использовать относительные пути
    /// </summary>
    public WriteOptionsBuilder UseRelativePaths()
    {
        _useRelativePaths = true;
        return this;
    }

    /// <summary>
    /// Использовать абсолютные пути
    /// </summary>
    public WriteOptionsBuilder UseAbsolutePaths()
    {
        _useRelativePaths = false;
        return this;
    }

    /// <summary>
    /// Установить кодировку файлов
    /// </summary>
    public WriteOptionsBuilder WithEncoding(string encoding)
    {
        _encoding = encoding;
        return this;
    }

    /// <summary>
    /// Использовать UTF-8 кодировку (по умолчанию)
    /// </summary>
    public WriteOptionsBuilder UseUtf8()
    {
        _encoding = "UTF-8";
        return this;
    }

    /// <summary>
    /// Использовать UTF-8 с BOM
    /// </summary>
    public WriteOptionsBuilder UseUtf8WithBom()
    {
        _encoding = "UTF-8-BOM";
        return this;
    }

    /// <summary>
    /// Использовать UTF-16 кодировку
    /// </summary>
    public WriteOptionsBuilder UseUtf16()
    {
        _encoding = "UTF-16";
        return this;
    }

    /// <summary>
    /// Режим "сухого запуска" - проверка без записи
    /// </summary>
    public WriteOptionsBuilder DryRun()
    {
        _dryRun = true;
        return this;
    }

    /// <summary>
    /// Обычный режим записи (не dry run)
    /// </summary>
    public WriteOptionsBuilder ActualWrite()
    {
        _dryRun = false;
        return this;
    }

    /// <summary>
    /// Строит финальные опции записи
    /// </summary>
    public WriteOptions Build()
    {
        return new WriteOptions
        {
            OutputPath = _outputPath,
            OverwriteExisting = _overwriteExisting,
            CreateDirectories = _createDirectories,
            CreateBackups = _createBackups,
            BackupPath = _backupPath,
            UseRelativePaths = _useRelativePaths,
            Encoding = _encoding,
            DryRun = _dryRun
        };
    }
}
