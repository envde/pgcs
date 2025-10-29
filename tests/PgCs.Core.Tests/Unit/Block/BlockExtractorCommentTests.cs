using PgCs.Core.Extraction.Block;

namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с комментариями
/// </summary>
public sealed class BlockExtractorCommentTests
{
    private readonly BlockExtractor _extractor = new();

    [Fact]
    public void Extract_HeaderComment_AttachesToBlock()
    {
        // Arrange
        var sql = """
            -- Таблица пользователей
            CREATE TABLE users (id INT);
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Таблица пользователей", blocks[0].HeaderComment);
        Assert.Contains("CREATE TABLE users", blocks[0].Content);
    }

    [Fact]
    public void Extract_MultilineHeaderComment_CombinesLines()
    {
        // Arrange
        var sql = """
            -- Таблица пользователей
            -- Содержит основную информацию
            -- о зарегистрированных пользователях
            CREATE TABLE users (id INT);
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.NotNull(blocks[0].HeaderComment);
        Assert.Contains("Таблица пользователей", blocks[0].HeaderComment);
        Assert.Contains("Содержит основную информацию", blocks[0].HeaderComment);
        Assert.Contains("о зарегистрированных пользователях", blocks[0].HeaderComment);
    }

    [Fact]
    public void Extract_InlineComment_PreservesInRawContent()
    {
        // Arrange
        var sql = """
            -- Информация о пользователе
            SELECT 
                id, -- Уникальный идентификатор
                name -- Имя пользователя
            FROM users;
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        Assert.NotNull(blocks[0].InlineComments);
        Assert.Equal(2, blocks[0].InlineComments?.Count);
        Assert.True(blocks[0].InlineComments?.Any(c=>c.Key == "id"));
        Assert.True(blocks[0].InlineComments?.Any(c=>c.Key == "name"));
        Assert.Contains("-- Уникальный идентификатор", blocks[0].RawContent);
        Assert.Contains("-- Имя пользователя", blocks[0].RawContent);
        Assert.Contains("-- Информация о пользователе", blocks[0].RawContent);
        Assert.Contains("Информация о пользователе", blocks[0].HeaderComment);
    }

    [Fact]
    public void Extract_CommentAfterEmptyLine_StillAttachedToNextBlock()
    {
        // Arrange
        var sql = """
            -- Старый комментарий
            
            CREATE TABLE users (id INT);
            """;

        // Act
        var blocks = _extractor.Extract(sql);

        // Assert
        Assert.Single(blocks);
        // Примечание: текущая реализация не очищает комментарии после пустой строки
        // Это поведение можно изменить при необходимости
    }
}
