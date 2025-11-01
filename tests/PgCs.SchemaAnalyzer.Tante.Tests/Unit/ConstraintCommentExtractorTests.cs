using PgCs.Core.Extraction.Block;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для ConstraintCommentExtractor
/// </summary>
public sealed class ConstraintCommentExtractorTests
{
    private readonly IConstraintCommentExtractor _extractor = new ConstraintCommentExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidConstraintCommentBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("COMMENT ON CONSTRAINT pk_users ON users IS 'Primary key';");

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

    #region Extract Constraint Comment Tests

    [Fact]
    public void Extract_SimpleConstraintComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON CONSTRAINT pk_users ON users IS 'Primary key constraint';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pk_users", result.ConstraintName);
        Assert.Equal("Primary key constraint", result.Comment);
        Assert.Null(result.Schema);
    }

    [Fact]
    public void Extract_ConstraintCommentWithSchema_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON CONSTRAINT fk_orders_user ON public.orders IS 'Foreign key to users';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fk_orders_user", result.ConstraintName);
        Assert.Equal("Foreign key to users", result.Comment);
        Assert.Equal("public", result.Schema);
    }

    [Fact]
    public void Extract_ConstraintCommentWithEmptyComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON CONSTRAINT uk_email ON users IS '';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("uk_email", result.ConstraintName);
        Assert.Equal("", result.Comment);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_UppercaseKeywords_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON CONSTRAINT PK_USERS ON USERS IS 'Upper case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PK_USERS", result.ConstraintName);
        Assert.Equal("Upper case", result.Comment);
    }

    [Fact]
    public void Extract_MixedCase_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "Comment On Constraint MyConstraint On MyTable Is 'Mixed case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyConstraint", result.ConstraintName);
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
