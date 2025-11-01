using PgCs.Core.Extraction.Block;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для IndexCommentExtractor
/// </summary>
public sealed class IndexCommentExtractorTests
{
    private readonly IIndexCommentExtractor _extractor = new IndexCommentExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidIndexCommentBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("COMMENT ON INDEX idx_users_email IS 'Email index';");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableComment_ReturnsFalse()
    {
        // Arrange
        var block = CreateBlock("COMMENT ON TABLE users IS 'User table';");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExtract_WithNullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
    }

    #endregion

    #region Extract Index Comment Tests

    [Fact]
    public void Extract_SimpleIndexComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON INDEX idx_users_email IS 'Index for email lookups';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_email", result.IndexName);
        Assert.Equal("Index for email lookups", result.Comment);
        Assert.Null(result.Schema);
    }

    [Fact]
    public void Extract_IndexCommentWithSchema_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON INDEX public.idx_users_email IS 'Public schema index';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_email", result.IndexName);
        Assert.Equal("Public schema index", result.Comment);
        Assert.Equal("public", result.Schema);
    }

    [Fact]
    public void Extract_IndexCommentWithEmptyComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON INDEX idx_orders_date IS '';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_orders_date", result.IndexName);
        Assert.Equal("", result.Comment);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_UppercaseKeywords_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON INDEX IDX_USERS_EMAIL IS 'Upper case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("IDX_USERS_EMAIL", result.IndexName);
        Assert.Equal("Upper case", result.Comment);
    }

    [Fact]
    public void Extract_MixedCase_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "Comment On Index MyIndex Is 'Mixed case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyIndex", result.IndexName);
        Assert.Equal("Mixed case", result.Comment);
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_InvalidSql_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id INTEGER);");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_NullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_EmptyBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Helper Methods

    private static SqlBlock CreateBlock(string content, string? headerComment = null)
    {
        return new SqlBlock
        {
            Content = content,
            RawContent = content,
            HeaderComment = headerComment,
            StartLine = 1,
            EndLine = 1
        };
    }

    #endregion
}
