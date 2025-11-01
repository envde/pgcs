using PgCs.Core.Extraction.Block;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для ColumnCommentExtractor
/// </summary>
public sealed class ColumnCommentExtractorTests
{
    private readonly IColumnCommentExtractor _extractor = new ColumnCommentExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidColumnCommentBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("COMMENT ON COLUMN users.email IS 'User email address';");

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

    #region Extract Column Comment Tests

    [Fact]
    public void Extract_SimpleColumnComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON COLUMN users.email IS 'User email address';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("users", result.TableName);
        Assert.Equal("email", result.ColumnName);
        Assert.Equal("User email address", result.Comment);
        Assert.Null(result.Schema);
    }

    [Fact]
    public void Extract_ColumnCommentWithSchema_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON COLUMN public.users.email IS 'Email in public schema';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("users", result.TableName);
        Assert.Equal("email", result.ColumnName);
        Assert.Equal("Email in public schema", result.Comment);
        Assert.Equal("public", result.Schema);
    }

    [Fact]
    public void Extract_ColumnCommentWithEmptyComment_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON COLUMN orders.status IS '';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("orders", result.TableName);
        Assert.Equal("status", result.ColumnName);
        Assert.Equal("", result.Comment);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_UppercaseKeywords_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "COMMENT ON COLUMN USERS.EMAIL IS 'Upper case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USERS", result.TableName);
        Assert.Equal("EMAIL", result.ColumnName);
        Assert.Equal("Upper case", result.Comment);
    }

    [Fact]
    public void Extract_MixedCase_ReturnsCorrectDefinition()
    {
        // Arrange
        var sql = "Comment On Column MyTable.MyColumn Is 'Mixed case';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyTable", result.TableName);
        Assert.Equal("MyColumn", result.ColumnName);
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
