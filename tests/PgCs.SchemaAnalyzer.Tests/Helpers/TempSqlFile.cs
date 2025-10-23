namespace PgCs.SchemaAnalyzer.Tests.Helpers;

/// <summary>
/// Временный SQL файл для тестирования. Автоматически удаляется после использования.
/// </summary>
public sealed class TempSqlFile : IDisposable
{
    public string Path { get; }

    public TempSqlFile(string content)
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), 
            $"pgcs_schema_test_{Guid.NewGuid():N}.sql"
        );
        File.WriteAllText(Path, content);
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }
        catch
        {
            // Игнорируем ошибки очистки в тестах
        }
    }
}
