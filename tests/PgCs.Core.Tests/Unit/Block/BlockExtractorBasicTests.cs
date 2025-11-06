using PgCs.Core.Parsing.Blocks;


namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Базовые тесты для BlockExtractor
/// </summary>
public sealed class BlockExtractorBasicTests
{

    [Fact]
    public void Extract_EmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => BlockParser.Parse(""));
    }

    [Fact]
    public void Extract_WhitespaceOnly_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => BlockParser.Parse("   \n\t  "));
    }

    [Fact]
    public void Extract_SingleStatement_ReturnsOneBlock()
    {
        // Arrange
        var sql = "SELECT * FROM users;";

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("SELECT * FROM users;", blocks[0].Content);
        Assert.Equal(1, blocks[0].StartLine);
        Assert.Equal(1, blocks[0].EndLine);
    }

    [Fact]
    public void Extract_MultipleStatements_ReturnsMultipleBlocks()
    {
        // Arrange
        var sql = """
            SELECT * FROM users;
            SELECT * FROM orders;
            SELECT * FROM products;
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(3, blocks.Count);
        Assert.Contains("users", blocks[0].Content);
        Assert.Contains("orders", blocks[1].Content);
        Assert.Contains("products", blocks[2].Content);
    }
}
