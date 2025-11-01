using PgCs.Core.Extraction.Block;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для TriggerCommentExtractor
/// </summary>
public sealed class TriggerCommentExtractorTests
{
    private readonly ITriggerCommentExtractor _extractor = new TriggerCommentExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidTriggerCommentBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("COMMENT ON TRIGGER update_timestamp ON users IS 'Updates timestamp';");

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

    #region Extract Trigger Comment Tests

    [Fact]
    public void Extract_SimpleTriggerComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TRIGGER update_timestamp ON users IS 'Updates timestamp on modification';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("update_timestamp", result.TriggerName);
        Assert.Equal("Updates timestamp on modification", result.Comment);
        Assert.Null(result.Schema);
    }

    [Fact]
    public void Extract_TriggerCommentWithSchema_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TRIGGER update_timestamp ON public.users IS 'Public schema trigger';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("update_timestamp", result.TriggerName);
        Assert.Equal("Public schema trigger", result.Comment);
        Assert.Equal("public", result.Schema);
    }

    [Fact]
    public void Extract_TriggerCommentWithEmptyComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TRIGGER validate_order ON orders IS '';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("validate_order", result.TriggerName);
        Assert.Equal("", result.Comment);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_UppercaseKeywords_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TRIGGER UPDATE_TIMESTAMP ON USERS IS 'Upper case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UPDATE_TIMESTAMP", result.TriggerName);
        Assert.Equal("Upper case", result.Comment);
    }

    [Fact]
    public void Extract_MixedCase_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "Comment On Trigger MyTrigger On MyTable Is 'Mixed case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyTrigger", result.TriggerName);
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
