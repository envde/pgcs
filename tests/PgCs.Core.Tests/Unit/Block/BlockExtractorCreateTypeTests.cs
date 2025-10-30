using PgCs.Core.Extraction.Block;

namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с CREATE TYPE
/// </summary>
public sealed class BlockExtractorCreateTypeTests
{
    private readonly BlockExtractor _extractor = new();

    [Fact]
    public void Extract_CreateEnumType_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- ENUM для статуса пользователя
            CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("ENUM для статуса пользователя", blocks[0].HeaderComment);
        Assert.Contains("CREATE TYPE user_status", blocks[0].Content);
        Assert.Contains("ENUM", blocks[0].Content);
        Assert.Contains("'active'", blocks[0].Content);
    }

    [Fact]
    public void Extract_CreateCompositeType_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- Композитный тип для адреса
            CREATE TYPE address AS (
                street VARCHAR(255),
                city VARCHAR(100),
                postal_code VARCHAR(20),
                country VARCHAR(100)
            );
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Композитный тип для адреса", blocks[0].HeaderComment);
        Assert.Contains("CREATE TYPE address", blocks[0].Content);
        Assert.Contains("street VARCHAR(255)", blocks[0].Content);
        Assert.True(blocks[0].LineCount >= 5);
    }

    [Fact]
    public void Extract_CreateDomainType_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- Email с валидацией
            CREATE DOMAIN email AS VARCHAR(255)
                CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Email с валидацией", blocks[0].HeaderComment);
        Assert.Contains("CREATE DOMAIN email", blocks[0].Content);
        Assert.Contains("CHECK", blocks[0].Content);
    }
}
