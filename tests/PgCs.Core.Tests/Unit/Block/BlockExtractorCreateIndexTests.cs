using PgCs.Core.Parsing.Blocks;


namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с CREATE INDEX
/// </summary>
public sealed class BlockExtractorCreateIndexTests
{

    [Fact]
    public void Extract_CreateIndex_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- Индекс для быстрого поиска по email
            CREATE INDEX idx_users_email ON users (email) WHERE is_deleted = FALSE;
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Индекс для быстрого поиска по email", blocks[0].HeaderComment);
        Assert.Contains("CREATE INDEX idx_users_email", blocks[0].Content);
        Assert.Contains("WHERE is_deleted = FALSE", blocks[0].Content);
    }

    [Fact]
    public void Extract_CreateUniqueIndex_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE UNIQUE INDEX idx_users_username ON users (username);
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Contains("CREATE UNIQUE INDEX", blocks[0].Content);
    }

    [Fact]
    public void Extract_CreateGinIndex_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE INDEX idx_users_preferences ON users USING GIN (preferences);
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Contains("USING GIN", blocks[0].Content);
    }
}
