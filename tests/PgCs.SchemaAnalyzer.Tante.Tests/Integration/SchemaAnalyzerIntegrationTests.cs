using PgCs.SchemaAnalyzer.Tante;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Integration;

/// <summary>
/// Интеграционные тесты для SchemaAnalyzer с реальными файлами
/// </summary>
public sealed class SchemaAnalyzerIntegrationTests
{
    private readonly SchemaAnalyzer _analyzer = new();
    private readonly string _projectRoot;

    public SchemaAnalyzerIntegrationTests()
    {
        // Находим корень проекта (где находится .sln файл)
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, "PgCs.slnx")))
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        
        _projectRoot = currentDir ?? throw new InvalidOperationException("Project root not found");
    }

    [Fact]
    public async Task AnalyzeFileAsync_WithExampleSchemaFile_ExtractsEnums()
    {
        // Arrange
        var schemaPath = Path.Combine(_projectRoot, "src", "PgCs.Core", "Example", "Schema.sql");
        
        // Skip test if file doesn't exist
        if (!File.Exists(schemaPath))
        {
            return;
        }

        // Act
        var metadata = await _analyzer.AnalyzeFileAsync(schemaPath);

        // Assert
        Assert.NotNull(metadata);
        Assert.NotEmpty(metadata.Enums);
        
        // Проверяем, что извлечены ENUMы из примера
        Assert.Contains(metadata.Enums, e => e.Name == "user_status");
        Assert.Contains(metadata.Enums, e => e.Name == "order_status");
        Assert.Contains(metadata.Enums, e => e.Name == "payment_method");
        Assert.Contains(metadata.Enums, e => e.Name == "priority_level");

        // Проверяем источник
        Assert.Single(metadata.SourcePaths);
        Assert.Equal(schemaPath, metadata.SourcePaths[0]);
    }

    [Fact]
    public async Task AnalyzeFileAsync_WithExampleSchemaFile_ExtractsCorrectEnumValues()
    {
        // Arrange
        var schemaPath = Path.Combine(_projectRoot, "src", "PgCs.Core", "Example", "Schema.sql");
        
        if (!File.Exists(schemaPath))
        {
            return;
        }

        // Act
        var metadata = await _analyzer.AnalyzeFileAsync(schemaPath);

        // Assert
        var userStatus = metadata.Enums.FirstOrDefault(e => e.Name == "user_status");
        Assert.NotNull(userStatus);
        Assert.Equal(4, userStatus.Values.Count);
        Assert.Equal("active", userStatus.Values[0]);
        Assert.Equal("inactive", userStatus.Values[1]);
        Assert.Equal("suspended", userStatus.Values[2]);
        Assert.Equal("deleted", userStatus.Values[3]);

        var orderStatus = metadata.Enums.FirstOrDefault(e => e.Name == "order_status");
        Assert.NotNull(orderStatus);
        Assert.Equal(5, orderStatus.Values.Count);
        Assert.Contains("pending", orderStatus.Values);
        Assert.Contains("delivered", orderStatus.Values);
        Assert.Contains("cancelled", orderStatus.Values);
    }

    [Fact]
    public async Task AnalyzeFileAsync_WithExampleSchemaFile_PreservesComments()
    {
        // Arrange
        var schemaPath = Path.Combine(_projectRoot, "src", "PgCs.Core", "Example", "Schema.sql");
        
        if (!File.Exists(schemaPath))
        {
            return;
        }

        // Act
        var metadata = await _analyzer.AnalyzeFileAsync(schemaPath);

        // Assert
        var userStatus = metadata.Enums.FirstOrDefault(e => e.Name == "user_status");
        Assert.NotNull(userStatus);
        Assert.NotNull(userStatus.SqlComment);
        Assert.Contains("статус", userStatus.SqlComment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AnalyzeFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_projectRoot, "non_existent.sql");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            _analyzer.AnalyzeFileAsync(nonExistentPath).AsTask());
    }

    [Fact]
    public async Task AnalyzeDirectoryAsync_WithExampleDirectory_ExtractsEnums()
    {
        // Arrange
        var exampleDir = Path.Combine(_projectRoot, "src", "PgCs.Core", "Example");
        
        if (!Directory.Exists(exampleDir))
        {
            return;
        }

        // Act
        var metadata = await _analyzer.AnalyzeDirectoryAsync(exampleDir);

        // Assert
        Assert.NotNull(metadata);
        Assert.NotEmpty(metadata.Enums);
        Assert.NotEmpty(metadata.SourcePaths);
    }

    [Fact]
    public async Task AnalyzeDirectoryAsync_NonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_projectRoot, "non_existent_directory");

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => 
            _analyzer.AnalyzeDirectoryAsync(nonExistentDir).AsTask());
    }

    [Fact]
    public void ExtractEnums_WithRealWorldSql_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
-- ENUM для статуса пользователя
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');
COMMENT ON TYPE user_status IS 'Возможные статусы пользователя в системе';

-- ENUM для статуса заказа
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');
COMMENT ON TYPE order_status IS 'Статусы жизненного цикла заказа';
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("user_status", result[0].Name);
        Assert.Equal("order_status", result[1].Name);
    }

    [Fact]
    public async Task AnalyzeFileAsync_SetsAnalyzedAtTimestamp()
    {
        // Arrange
        var schemaPath = Path.Combine(_projectRoot, "src", "PgCs.Core", "Example", "Schema.sql");
        
        if (!File.Exists(schemaPath))
        {
            return;
        }

        var beforeAnalysis = DateTime.UtcNow;

        // Act
        var metadata = await _analyzer.AnalyzeFileAsync(schemaPath);

        var afterAnalysis = DateTime.UtcNow;

        // Assert
        Assert.True(metadata.AnalyzedAt >= beforeAnalysis);
        Assert.True(metadata.AnalyzedAt <= afterAnalysis);
    }
}
