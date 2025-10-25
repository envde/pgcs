using PgCs.QueryAnalyzer.Tests.Helpers;
using SharedTestHelper = PgCs.Tests.Shared.Helpers.TestFileHelper;

namespace PgCs.QueryAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для QueryAnalyzerBuilder - fluent API для анализа запросов
/// </summary>
public sealed class QueryAnalyzerBuilderTests
{
    [Fact]
    public void Create_ReturnsNewBuilder()
    {
        // Act
        var builder = QueryAnalyzerBuilder.Create();

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void FromFile_WithValidPath_AddsFile()
    {
        // Arrange
        using var tempFile = SharedTestHelper.CreateTempFile("-- name: GetUser :one\nSELECT * FROM users;");

        // Act & Assert
        var builder = QueryAnalyzerBuilder.Create().FromFile(tempFile.Path);
        Assert.NotNull(builder);
    }

    [Fact]
    public void FromFile_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            QueryAnalyzerBuilder.Create().FromFile(null!));
    }

    [Fact]
    public void FromFile_WithEmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            QueryAnalyzerBuilder.Create().FromFile(""));
    }

    [Fact]
    public void FromFiles_WithMultiplePaths_AddsAllFiles()
    {
        // Arrange
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        File.WriteAllText(tempFile1, "-- @name: GetUser\nSELECT * FROM users;");
        File.WriteAllText(tempFile2, "-- @name: GetPost\nSELECT * FROM posts;");

        try
        {
            // Act & Assert
            var builder = QueryAnalyzerBuilder.Create().FromFiles(tempFile1, tempFile2);
            Assert.NotNull(builder);
        }
        finally
        {
            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }
    }

    [Fact]
    public void FromFiles_WithNullArray_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            QueryAnalyzerBuilder.Create().FromFiles(null!));
    }

    [Fact]
    public void FromDirectory_WithValidDirectory_AddsAllSqlFiles()
    {
        // Arrange
        using var tempDir = SharedTestHelper.CreateTempDirectory();
        tempDir.CreateFile("query1.sql", "-- name: GetUser :one\nSELECT * FROM users;");
        tempDir.CreateFile("query2.sql", "-- name: GetPost :one\nSELECT * FROM posts;");

        // Act & Assert
        var builder = QueryAnalyzerBuilder.Create().FromDirectory(tempDir.Path);
        Assert.NotNull(builder);
    }

    [Fact]
    public void FromDirectory_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => 
            QueryAnalyzerBuilder.Create().FromDirectory(nonExistentDir));
    }

    [Fact]
    public void FromDirectory_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            QueryAnalyzerBuilder.Create().FromDirectory(null!));
    }

    [Fact]
    public void FromDirectory_WithEmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            QueryAnalyzerBuilder.Create().FromDirectory(""));
    }

    [Fact]
    public async Task AnalyzeAsync_WithoutFiles_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = QueryAnalyzerBuilder.Create();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await builder.AnalyzeAsync());
    }

    [Fact]
    public async Task AnalyzeAsync_WithSingleFile_ReturnsQueries()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, @"-- name: GetUser :one
-- description: Get user by ID
SELECT id, username
FROM users
WHERE id = $1;");

        try
        {
            // Act
            var queries = await QueryAnalyzerBuilder.Create()
                .FromFile(tempFile)
                .AnalyzeAsync();

            // Assert
            Assert.NotEmpty(queries);
            Assert.Single(queries);
            Assert.Equal("GetUser", queries[0].MethodName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_WithMultipleFiles_ReturnsAllQueries()
    {
        // Arrange
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        File.WriteAllText(tempFile1, @"-- name: GetUser :one
SELECT id, username FROM users WHERE id = $1;");
        File.WriteAllText(tempFile2, @"-- name: GetPost :one
SELECT id, title FROM posts WHERE id = $1;");

        try
        {
            // Act
            var queries = await QueryAnalyzerBuilder.Create()
                .FromFiles(tempFile1, tempFile2)
                .AnalyzeAsync();

            // Assert
            Assert.NotEmpty(queries);
            Assert.Equal(2, queries.Count);
            Assert.Contains(queries, q => q.MethodName == "GetUser");
            Assert.Contains(queries, q => q.MethodName == "GetPost");
        }
        finally
        {
            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_WithDirectory_ReturnsAllQueriesFromAllFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var file1 = Path.Combine(tempDir, "query1.sql");
        var file2 = Path.Combine(tempDir, "query2.sql");
        File.WriteAllText(file1, @"-- name: GetUser :one
SELECT id, username FROM users WHERE id = $1;");
        File.WriteAllText(file2, @"-- name: GetPost :one
SELECT id, title FROM posts WHERE id = $1;");

        try
        {
            // Act
            var queries = await QueryAnalyzerBuilder.Create()
                .FromDirectory(tempDir)
                .AnalyzeAsync();

            // Assert
            Assert.NotEmpty(queries);
            Assert.Equal(2, queries.Count);
            Assert.Contains(queries, q => q.MethodName == "GetUser");
            Assert.Contains(queries, q => q.MethodName == "GetPost");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_WithSubdirectories_ReturnsQueriesFromAllLevels()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var subDir = Path.Combine(tempDir, "sub");
        Directory.CreateDirectory(subDir);
        
        var file1 = Path.Combine(tempDir, "query1.sql");
        var file2 = Path.Combine(subDir, "query2.sql");
        File.WriteAllText(file1, @"-- name: GetUser :one
SELECT id, username FROM users WHERE id = $1;");
        File.WriteAllText(file2, @"-- name: GetPost :one
SELECT id, title FROM posts WHERE id = $1;");

        try
        {
            // Act
            var queries = await QueryAnalyzerBuilder.Create()
                .FromDirectory(tempDir)
                .AnalyzeAsync();

            // Assert
            Assert.NotEmpty(queries);
            Assert.Equal(2, queries.Count);
            Assert.Contains(queries, q => q.MethodName == "GetUser");
            Assert.Contains(queries, q => q.MethodName == "GetPost");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_ChainedCalls_ReturnsAllQueries()
    {
        // Arrange
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var file3 = Path.Combine(tempDir, "query3.sql");
        
        File.WriteAllText(tempFile1, @"-- name: GetUser :one
SELECT id, username FROM users WHERE id = $1;");
        File.WriteAllText(tempFile2, @"-- name: GetPost :one
SELECT id, title FROM posts WHERE id = $1;");
        File.WriteAllText(file3, @"-- name: GetComment :one
SELECT id, text FROM comments WHERE id = $1;");

        try
        {
            // Act
            var queries = await QueryAnalyzerBuilder.Create()
                .FromFile(tempFile1)
                .FromFile(tempFile2)
                .FromDirectory(tempDir)
                .AnalyzeAsync();

            // Assert
            Assert.NotEmpty(queries);
            Assert.Equal(3, queries.Count);
            Assert.Contains(queries, q => q.MethodName == "GetUser");
            Assert.Contains(queries, q => q.MethodName == "GetPost");
            Assert.Contains(queries, q => q.MethodName == "GetComment");
        }
        finally
        {
            File.Delete(tempFile1);
            File.Delete(tempFile2);
            Directory.Delete(tempDir, true);
        }
    }
}
