using PgCs.Core.Extraction.Block;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для CompositeTypeCommentExtractor
/// </summary>
public sealed class CompositeTypeCommentExtractorTests
{
    private readonly ICompositeTypeCommentExtractor _extractor = new CompositeTypeCommentExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidTypeCommentBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("COMMENT ON TYPE address IS 'Address composite type';");

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

    #region Extract Type Comment Tests

    [Fact]
    public void Extract_SimpleTypeComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TYPE address IS 'Address composite type';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("address", result.CompositeTypeName);
        Assert.Equal("Address composite type", result.Comment);
        Assert.Null(result.Schema);
    }

    [Fact]
    public void Extract_TypeCommentWithSchema_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TYPE public.address IS 'Type in public schema';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("address", result.CompositeTypeName);
        Assert.Equal("Type in public schema", result.Comment);
        Assert.Equal("public", result.Schema);
    }

    [Fact]
    public void Extract_TypeCommentWithEmptyComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TYPE coordinates IS '';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("coordinates", result.CompositeTypeName);
        Assert.Equal("", result.Comment);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_UppercaseKeywords_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TYPE ADDRESS IS 'Upper case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ADDRESS", result.CompositeTypeName);
        Assert.Equal("Upper case", result.Comment);
    }

    [Fact]
    public void Extract_MixedCase_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "Comment On Type MyType Is 'Mixed case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyType", result.CompositeTypeName);
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
