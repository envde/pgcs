using PgCs.Core.Parsing.Blocks;


namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с CREATE VIEW
/// </summary>
public sealed class BlockExtractorCreateViewTests
{

    [Fact]
    public void Extract_CreateView_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- Активные пользователи
            CREATE VIEW active_users AS
            SELECT id, username, email
            FROM users
            WHERE status = 'active' AND is_deleted = FALSE;
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Активные пользователи", blocks[0].HeaderComment);
        Assert.Contains("CREATE VIEW active_users", blocks[0].Content);
    }

    [Fact]
    public void Extract_CreateMaterializedView_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE MATERIALIZED VIEW user_statistics AS
            SELECT 
                user_id,
                COUNT(*) AS order_count,
                SUM(total) AS total_spent
            FROM orders
            GROUP BY user_id;
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Contains("CREATE MATERIALIZED VIEW", blocks[0].Content);
    }
}
