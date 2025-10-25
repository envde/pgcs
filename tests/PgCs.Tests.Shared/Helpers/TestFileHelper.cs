namespace PgCs.Tests.Shared.Helpers;

/// <summary>
/// Утилиты для работы с тестовыми файлами
/// </summary>
public static class TestFileHelper
{
    /// <summary>
    /// Получить путь к папке TestData относительно текущей сборки
    /// </summary>
    public static string GetTestDataPath(Type? testClassType = null)
    {
        var assemblyLocation = testClassType?.Assembly.Location 
            ?? AppContext.BaseDirectory;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
        return Path.Combine(assemblyDir, "TestData");
    }

    /// <summary>
    /// Получить полный путь к файлу в папке TestData
    /// </summary>
    public static string GetTestDataFilePath(string fileName, Type? testClassType = null)
    {
        var testDataPath = GetTestDataPath(testClassType);
        return Path.Combine(testDataPath, fileName);
    }

    /// <summary>
    /// Прочитать содержимое тестового файла
    /// </summary>
    public static string ReadTestFile(string fileName, Type? testClassType = null)
    {
        var filePath = GetTestDataFilePath(fileName, testClassType);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test file not found: {filePath}");
        }

        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// Прочитать содержимое тестового файла асинхронно
    /// </summary>
    public static async Task<string> ReadTestFileAsync(string fileName, Type? testClassType = null)
    {
        var filePath = GetTestDataFilePath(fileName, testClassType);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test file not found: {filePath}");
        }

        return await File.ReadAllTextAsync(filePath);
    }

    /// <summary>
    /// Создать временный файл с указанным содержимым
    /// </summary>
    public static TempSqlFile CreateTempFile(string content, string prefix = "pgcs_test")
    {
        return new TempSqlFile(content, prefix);
    }
}
