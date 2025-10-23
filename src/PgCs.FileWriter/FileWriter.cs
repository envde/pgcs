using System.Diagnostics;
using System.Text;
using PgCs.Common.CodeGeneration;
using PgCs.Common.Writer;
using PgCs.Common.Writer.Models;

namespace PgCs.FileWriter;

/// <summary>
/// Реализация записи сгенерированных файлов на диск
/// </summary>
public sealed class FileWriter : IWriter
{
    /// <summary>
    /// Создает экземпляр FileWriter
    /// </summary>
    public static FileWriter Create() => new();

    /// <summary>
    /// Записывает сгенерированный код на диск
    /// </summary>
    public async ValueTask<WriteResult> WriteManyAsync( IReadOnlyList<GeneratedCode> code, WriteOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        var writtenFiles = new List<string>();
        var errors = new List<WriteError>();
        var backupFiles = new List<string>();
        long totalBytes = 0;

        // Проверка возможности записи
        if (!await CanWriteAsync(options))
        {
            return new WriteResult
            {
                IsSuccess = false,
                WrittenFiles = [],
                Errors =
                [
                    new WriteError
                    {
                        FilePath = options.OutputPath,
                        Message = $"Невозможно записать в директорию: {options.OutputPath}",
                        ErrorType = WriteErrorType.AccessDenied
                    }
                ],
                Duration = stopwatch.Elapsed
            };
        }

        // Записываем каждый файл
        foreach (var item in code)
        {
            try
            {
                var result = await WriteOneAsync(item, options);
                
                if (result.IsSuccess)
                {
                    writtenFiles.AddRange(result.WrittenFiles);
                    backupFiles.AddRange(result.BackupFiles);
                    totalBytes += result.TotalBytesWritten;
                }
                else
                {
                    errors.AddRange(result.Errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new WriteError
                {
                    FilePath = item.SuggestedFileName,
                    Message = $"Неожиданная ошибка при записи: {ex.Message}",
                    ErrorType = WriteErrorType.IOError,
                    Exception = ex
                });
            }
        }

        stopwatch.Stop();

        return new WriteResult
        {
            IsSuccess = errors.Count == 0,
            WrittenFiles = writtenFiles,
            Errors = errors,
            BackupFiles = backupFiles,
            Duration = stopwatch.Elapsed,
            TotalBytesWritten = totalBytes
        };
    }

    /// <summary>
    /// Записывает один элемент сгенерированного кода на диск
    /// </summary>
    public async ValueTask<WriteResult> WriteOneAsync( GeneratedCode code, WriteOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        var backupFiles = new List<string>();

        try
        {
            // Определяем финальный путь к файлу
            var targetPath = GetTargetFilePath(code, options);

            // Валидация пути
            if (!IsValidPath(targetPath))
            {
                return CreateErrorResult(
                    targetPath,
                    "Некорректный путь к файлу",
                    WriteErrorType.InvalidPath,
                    stopwatch.Elapsed);
            }

            // Dry run - только проверка
            if (options.DryRun)
            {
                return new WriteResult
                {
                    IsSuccess = true,
                    WrittenFiles = new[] { targetPath },
                    Duration = stopwatch.Elapsed,
                    TotalBytesWritten = code.SizeInBytes
                };
            }

            // Создаем директорию если нужно
            var directory = Path.GetDirectoryName(targetPath)!;
            if (!Directory.Exists(directory))
            {
                if (!options.CreateDirectories)
                {
                    return CreateErrorResult(
                        targetPath,
                        $"Директория не существует: {directory}",
                        WriteErrorType.DirectoryNotFound,
                        stopwatch.Elapsed);
                }

                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    return CreateErrorResult(
                        targetPath,
                        $"Не удалось создать директорию: {ex.Message}",
                        WriteErrorType.AccessDenied,
                        stopwatch.Elapsed,
                        ex);
                }
            }

            // Проверяем существование файла
            if (File.Exists(targetPath))
            {
                if (!options.OverwriteExisting)
                {
                    return CreateErrorResult(
                        targetPath,
                        "Файл уже существует и перезапись запрещена",
                        WriteErrorType.FileExists,
                        stopwatch.Elapsed);
                }

                // Создаем backup если нужно
                if (options.CreateBackups)
                {
                    var backupPath = CreateBackup(targetPath, options.BackupPath);
                    if (backupPath != null)
                    {
                        backupFiles.Add(backupPath);
                    }
                }
            }

            // Записываем файл
            var encoding = GetEncoding(options.Encoding);
            await File.WriteAllTextAsync(targetPath, code.SourceCode, encoding);

            stopwatch.Stop();

            return new WriteResult
            {
                IsSuccess = true,
                WrittenFiles = [targetPath],
                BackupFiles = backupFiles,
                Duration = stopwatch.Elapsed,
                TotalBytesWritten = encoding.GetByteCount(code.SourceCode)
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            return CreateErrorResult(
                code.SuggestedFileName,
                "Нет доступа к файлу",
                WriteErrorType.AccessDenied,
                stopwatch.Elapsed,
                ex);
        }
        catch (IOException ex)
        {
            return CreateErrorResult(
                code.SuggestedFileName,
                $"Ошибка ввода-вывода: {ex.Message}",
                WriteErrorType.IOError,
                stopwatch.Elapsed,
                ex);
        }
        catch (Exception ex)
        {
            return CreateErrorResult(
                code.SuggestedFileName,
                $"Неожиданная ошибка: {ex.Message}",
                WriteErrorType.IOError,
                stopwatch.Elapsed,
                ex);
        }
    }

    /// <summary>
    /// Проверяет возможность записи в указанное место назначения
    /// </summary>
    public async ValueTask<bool> CanWriteAsync(WriteOptions options)
    {
        try
        {
            // Проверяем корректность пути
            if (string.IsNullOrWhiteSpace(options.OutputPath))
            {
                return false;
            }

            // Если директория существует, проверяем права записи
            if (Directory.Exists(options.OutputPath))
            {
                // Пытаемся создать временный файл для проверки прав
                var testFile = Path.Combine(options.OutputPath, $".pgcs_test_{Guid.NewGuid()}.tmp");
                try
                {
                    await File.WriteAllTextAsync(testFile, "test");
                    File.Delete(testFile);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // Директория не существует - проверяем можем ли мы её создать
            if (options.CreateDirectories)
            {
                try
                {
                    var parentDir = Directory.GetParent(options.OutputPath);
                    return parentDir?.Exists ?? false;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Удаляет ранее созданные файлы
    /// </summary>
    public WriteResult DeleteFiles(IReadOnlyList<string> filePaths)
    {
        var stopwatch = Stopwatch.StartNew();
        var deletedFiles = new List<string>();
        var errors = new List<WriteError>();

        foreach (var filePath in filePaths)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    deletedFiles.Add(filePath);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                errors.Add(new WriteError
                {
                    FilePath = filePath,
                    Message = "Нет доступа для удаления файла",
                    ErrorType = WriteErrorType.AccessDenied,
                    Exception = ex
                });
            }
            catch (IOException ex)
            {
                errors.Add(new WriteError
                {
                    FilePath = filePath,
                    Message = $"Ошибка при удалении: {ex.Message}",
                    ErrorType = WriteErrorType.IOError,
                    Exception = ex
                });
            }
        }

        stopwatch.Stop();

        return new WriteResult
        {
            IsSuccess = errors.Count == 0,
            WrittenFiles = deletedFiles,
            Errors = errors,
            Duration = stopwatch.Elapsed
        };
    }

    #region Private helpers

    /// <summary>
    /// Определяет финальный путь к файлу на основе опций
    /// </summary>
    private static string GetTargetFilePath(GeneratedCode code, WriteOptions options)
    {
        // Создаем путь на основе namespace и имени типа
        var namespaceParts = code.Namespace.Split('.');
        var relativePath = Path.Combine(namespaceParts);
        var fileName = code.SuggestedFileName;
        
        return Path.Combine(options.OutputPath, relativePath, fileName);
    }

    /// <summary>
    /// Проверяет корректность пути
    /// </summary>
    private static bool IsValidPath(string path)
    {
        try
        {
            Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Создает резервную копию файла
    /// </summary>
    private static string? CreateBackup(string filePath, string? backupPath)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var backupFileName = $"{fileName}.{timestamp}{extension}.bak";

            var backupDir = backupPath ?? Path.Combine(
                Path.GetDirectoryName(filePath)!, ".backup");

            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var backupFilePath = Path.Combine(backupDir, backupFileName);
            File.Copy(filePath, backupFilePath, overwrite: true);

            return backupFilePath;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Получает кодировку по имени
    /// </summary>
    private static Encoding GetEncoding(string encodingName)
    {
        return encodingName.ToUpperInvariant() switch
        {
            "UTF-8" => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            "UTF-8-BOM" => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            "UTF-16" => Encoding.Unicode,
            "UTF-32" => Encoding.UTF32,
            "ASCII" => Encoding.ASCII,
            _ => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };
    }

    /// <summary>
    /// Создает результат с ошибкой
    /// </summary>
    private static WriteResult CreateErrorResult(
        string filePath,
        string message,
        WriteErrorType errorType,
        TimeSpan duration,
        Exception? exception = null)
    {
        return new WriteResult
        {
            IsSuccess = false,
            WrittenFiles = [],
            Errors = new[]
            {
                new WriteError
                {
                    FilePath = filePath,
                    Message = message,
                    ErrorType = errorType,
                    Exception = exception
                }
            },
            Duration = duration
        };
    }

    #endregion
}
