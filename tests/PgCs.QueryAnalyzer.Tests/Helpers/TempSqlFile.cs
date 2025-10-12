namespace PgCs.QueryAnalyzer.Tests.Helpers;

public sealed class TempSqlFile : IDisposable
{
    public string Path { get; }

    public TempSqlFile(string content)
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), 
            $"pgcs_test_{Guid.NewGuid():N}.sql"
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
            // Ignore cleanup errors in tests
        }
    }
}