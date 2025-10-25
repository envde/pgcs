namespace PgCs.Tests.Shared.Helpers;

/// <summary>
/// Временная директория для тестирования. Автоматически удаляется после использования.
/// </summary>
public sealed class TempDirectory : IDisposable
{
    public string Path { get; }

    public TempDirectory(string? prefix = null)
    {
        var dirName = string.IsNullOrEmpty(prefix) 
            ? $"pgcs_test_{Guid.NewGuid():N}"
            : $"{prefix}_{Guid.NewGuid():N}";
            
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            dirName
        );
        Directory.CreateDirectory(Path);
    }

    /// <summary>
    /// Создать файл в временной директории
    /// </summary>
    public string CreateFile(string fileName, string content)
    {
        var filePath = System.IO.Path.Combine(Path, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch
        {
            // Игнорируем ошибки очистки в тестах
        }
    }
}
