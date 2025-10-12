namespace PgCs.QueryAnalyzer.Tests.Helpers;

public static class TestFileHelper
{
    private static readonly string TestDataPath = Path.Combine(
        AppContext.BaseDirectory,
        "TestData"
    );

    public static string GetTestDataPath(string fileName) =>
        Path.Combine(TestDataPath, fileName);

    public static string ReadTestFile(string fileName)
    {
        var path = GetTestDataPath(fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Test file not found: {path}");
        
        return File.ReadAllText(path);
    }

    public static async Task<string> ReadTestFileAsync(string fileName)
    {
        var path = GetTestDataPath(fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Test file not found: {path}");
        
        return await File.ReadAllTextAsync(path);
    }

    public static TempSqlFile CreateTempFile(string content) => new(content);
}