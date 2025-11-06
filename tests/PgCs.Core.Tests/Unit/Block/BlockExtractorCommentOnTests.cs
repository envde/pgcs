using PgCs.Core.Parsing.Blocks;


namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с COMMENT ON
/// </summary>
public sealed class BlockExtractorCommentOnTests
{

    [Fact]
    public void Extract_CommentOnTable_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            COMMENT ON TABLE users IS 'Основная таблица пользователей системы';
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Contains("COMMENT ON TABLE users", blocks[0].Content);
    }

    [Fact]
    public void Extract_CommentOnColumn_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            COMMENT ON COLUMN users.email IS 'Email адрес пользователя с валидацией';
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Contains("COMMENT ON COLUMN users.email", blocks[0].Content);
    }

    [Fact]
    public void Extract_MultipleCommentOnStatements_ExtractsAll()
    {
        // Arrange
        var sql = """
            COMMENT ON TABLE users IS 'Таблица пользователей';
            COMMENT ON COLUMN users.id IS 'Уникальный идентификатор';
            COMMENT ON COLUMN users.email IS 'Email адрес';
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(3, blocks.Count);
        Assert.All(blocks, block => Assert.Contains("COMMENT ON", block.Content));
    }
}
