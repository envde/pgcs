using PgCs.Core.Extraction.Block;

namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с CREATE TABLE
/// </summary>
public sealed class BlockExtractorCreateTableTests
{
    private readonly BlockExtractor _extractor = new();

    [Fact]
    public void Extract_CreateTable_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- Таблица пользователей
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(50) NOT NULL UNIQUE,
                email VARCHAR(255) NOT NULL UNIQUE,
                created_at TIMESTAMP DEFAULT NOW()
            );
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Таблица пользователей", blocks[0].HeaderComment);
        Assert.Contains("CREATE TABLE users", blocks[0].Content);
        Assert.Contains("id SERIAL PRIMARY KEY", blocks[0].Content);
        Assert.Contains("username VARCHAR(50)", blocks[0].Content);
    }

    [Fact]
    public void Extract_CreateTableWithConstraints_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE TABLE orders (
                id SERIAL PRIMARY KEY,
                user_id INTEGER NOT NULL,
                total NUMERIC(10, 2) NOT NULL,
                CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id),
                CONSTRAINT check_total CHECK (total >= 0)
            );
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Contains("CONSTRAINT fk_user", blocks[0].Content);
        Assert.Contains("CONSTRAINT check_total", blocks[0].Content);
    }
}
