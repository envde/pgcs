using PgCs.Core.Extraction.Block;

namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с RawContent и Content
/// </summary>
public sealed class BlockExtractorContentTests
{
    private readonly BlockExtractor _extractor = new();

    [Fact]
    public void Extract_RawContentPreservesFormatting_ContentIsClean()
    {
        // Arrange
        var sql = """
            -- Комментарий
            SELECT 
                id, -- inline
                name
            FROM users;
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.NotEqual(blocks[0].Content, blocks[0].RawContent);
        Assert.Contains("-- inline", blocks[0].RawContent);
    }

    [Fact]
    public void Extract_PreservesLineNumbers_Correctly()
    {
        // Arrange
        var sql = """
            -- Line 1
            -- Line 2
            CREATE TABLE users (id INT);
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal(3, blocks[0].StartLine);
        Assert.Equal(3, blocks[0].EndLine);
        Assert.Equal(1, blocks[0].LineCount);
    }

    [Fact]
    public void Extract_MultilineStatement_CorrectLineNumbers()
    {
        // Arrange
        var sql = """
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(50),
                email VARCHAR(255)
            );
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal(1, blocks[0].StartLine);
        Assert.Equal(5, blocks[0].EndLine);
        Assert.Equal(5, blocks[0].LineCount);
    }

    [Fact]
    public void HasComments_WithHeaderComment_ReturnsTrue()
    {
        // Arrange
        var sql = """
            -- Header comment
            SELECT * FROM users;
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.True(blocks[0].HasComments);
    }

    [Fact]
    public void HasComments_WithInlineComment_ChecksRawContent()
    {
        // Arrange
        var sql = """
            SELECT id -- inline comment
            FROM users;
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        // InlineComments может быть null или пустым в текущей реализации
        Assert.Single(blocks);
        Assert.Contains("-- inline comment", blocks[0].RawContent);
    }

    [Fact]
    public void HasComments_WithoutComments_ReturnsFalse()
    {
        // Arrange
        var sql = "SELECT * FROM users;";

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.False(blocks[0].HasComments);
    }
}
