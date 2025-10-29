using PgCs.Core.Extraction.Block;

namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с CREATE TRIGGER
/// </summary>
public sealed class BlockExtractorCreateTriggerTests
{
    private readonly BlockExtractor _extractor = new();

    [Fact]
    public void Extract_CreateTrigger_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- Триггер для автоматического обновления timestamp
            CREATE TRIGGER trigger_users_updated_at
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at();
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Триггер для автоматического обновления timestamp", blocks[0].HeaderComment);
        Assert.Contains("CREATE TRIGGER", blocks[0].Content);
        Assert.Contains("BEFORE UPDATE", blocks[0].Content);
        Assert.Contains("FOR EACH ROW", blocks[0].Content);
    }
}
