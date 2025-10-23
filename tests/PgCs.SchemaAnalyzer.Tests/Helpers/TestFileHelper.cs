namespace PgCs.SchemaAnalyzer.Tests.Helpers;

/// <summary>
/// Утилиты для работы с тестовыми файлами
/// </summary>
public static class TestFileHelper
{
    /// <summary>
    /// Получить путь к папке с тестовыми данными
    /// </summary>
    public static string GetTestDataPath()
    {
        var assemblyLocation = typeof(TestFileHelper).Assembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
        return Path.Combine(assemblyDir, "TestData");
    }

    /// <summary>
    /// Прочитать содержимое тестового файла
    /// </summary>
    public static string ReadTestFile(string fileName)
    {
        var testDataPath = GetTestDataPath();
        var filePath = Path.Combine(testDataPath, fileName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test file not found: {filePath}");
        }

        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// Прочитать содержимое тестового файла асинхронно
    /// </summary>
    public static async Task<string> ReadTestFileAsync(string fileName)
    {
        var testDataPath = GetTestDataPath();
        var filePath = Path.Combine(testDataPath, fileName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test file not found: {filePath}");
        }

        return await File.ReadAllTextAsync(filePath);
    }

    /// <summary>
    /// Создать временный файл с указанным содержимым
    /// </summary>
    public static TempSqlFile CreateTempFile(string content)
    {
        return new TempSqlFile(content);
    }
}
