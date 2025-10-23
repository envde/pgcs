using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.QueryAnalyzer.Parsing;

namespace PgCs.QueryAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для SqlQueryParser - парсера SQL запросов
/// </summary>
public sealed class SqlQueryParserTests
{
    [Fact]
    public void SplitCommentsAndQuery_WithCommentAndQuery_SplitsCorrectly()
    {
        // Arrange
        const string sql = @"-- Get user by ID
-- @name: GetUserById
SELECT id, username
FROM users
WHERE id = $1";

        // Act
        var (comments, query) = SqlQueryParser.SplitCommentsAndQuery(sql);

        // Assert
        Assert.Contains("Get user by ID", comments);
        Assert.Contains("@name: GetUserById", comments);
        Assert.Contains("SELECT id, username", query);
        Assert.DoesNotContain("--", query);
    }

    [Fact]
    public void SplitCommentsAndQuery_WithNoComments_ReturnsEmptyComments()
    {
        // Arrange
        const string sql = @"SELECT id, username
FROM users
WHERE id = $1";

        // Act
        var (comments, query) = SqlQueryParser.SplitCommentsAndQuery(sql);

        // Assert
        Assert.Empty(comments);
        Assert.Contains("SELECT id, username", query);
    }

    [Fact]
    public void SplitCommentsAndQuery_WithInlineComments_IgnoresInlineComments()
    {
        // Arrange
        const string sql = @"-- Header comment
SELECT id, username -- inline comment
FROM users
WHERE id = $1";

        // Act
        var (comments, query) = SqlQueryParser.SplitCommentsAndQuery(sql);

        // Assert
        Assert.Contains("Header comment", comments);
        Assert.Contains("SELECT id, username -- inline comment", query);
    }

    [Fact]
    public void DetermineQueryType_WithSelectQuery_ReturnsSelect()
    {
        // Arrange
        const string sql = "SELECT id, username FROM users";

        // Act
        var type = SqlQueryParser.DetermineQueryType(sql);

        // Assert
        Assert.Equal(QueryType.Select, type);
    }

    [Fact]
    public void DetermineQueryType_WithCte_ReturnsSelect()
    {
        // Arrange
        const string sql = @"WITH active_users AS (
    SELECT * FROM users WHERE active = true
)
SELECT * FROM active_users";

        // Act
        var type = SqlQueryParser.DetermineQueryType(sql);

        // Assert
        Assert.Equal(QueryType.Select, type);
    }

    [Fact]
    public void DetermineQueryType_WithInsertQuery_ReturnsInsert()
    {
        // Arrange
        const string sql = "INSERT INTO users (username) VALUES ($1)";

        // Act
        var type = SqlQueryParser.DetermineQueryType(sql);

        // Assert
        Assert.Equal(QueryType.Insert, type);
    }

    [Fact]
    public void DetermineQueryType_WithUpdateQuery_ReturnsUpdate()
    {
        // Arrange
        const string sql = "UPDATE users SET username = $1 WHERE id = $2";

        // Act
        var type = SqlQueryParser.DetermineQueryType(sql);

        // Assert
        Assert.Equal(QueryType.Update, type);
    }

    [Fact]
    public void DetermineQueryType_WithDeleteQuery_ReturnsDelete()
    {
        // Arrange
        const string sql = "DELETE FROM users WHERE id = $1";

        // Act
        var type = SqlQueryParser.DetermineQueryType(sql);

        // Assert
        Assert.Equal(QueryType.Delete, type);
    }

    [Fact]
    public void DetermineQueryType_WithUnknownQuery_ReturnsUnknown()
    {
        // Arrange
        const string sql = "TRUNCATE TABLE users";

        // Act
        var type = SqlQueryParser.DetermineQueryType(sql);

        // Assert
        Assert.Equal(QueryType.Unknown, type);
    }

    [Fact]
    public void DetermineQueryType_CaseInsensitive_ReturnsCorrectType()
    {
        // Arrange
        const string sql = "select id from users";

        // Act
        var type = SqlQueryParser.DetermineQueryType(sql);

        // Assert
        Assert.Equal(QueryType.Select, type);
    }

    [Fact]
    public void SplitIntoQueryBlocks_WithMultipleQueries_SplitsCorrectly()
    {
        // Arrange
        const string sql = @"-- Get all users
SELECT * FROM users;

-- Get user by ID
SELECT * FROM users WHERE id = $1;";

        // Act
        var blocks = SqlQueryParser.SplitIntoQueryBlocks(sql).ToList();

        // Assert
        Assert.Equal(2, blocks.Count);
        Assert.Contains("Get all users", blocks[0]);
        Assert.Contains("Get user by ID", blocks[1]);
    }

    [Fact]
    public void SplitIntoQueryBlocks_WithSingleQuery_ReturnsSingleBlock()
    {
        // Arrange
        const string sql = @"SELECT id, username
FROM users
WHERE id = $1;";

        // Act
        var blocks = SqlQueryParser.SplitIntoQueryBlocks(sql).ToList();

        // Assert
        Assert.Single(blocks);
        Assert.Contains("SELECT id, username", blocks[0]);
    }

    [Fact]
    public void SplitIntoQueryBlocks_WithNoSemicolon_ReturnsEntireContent()
    {
        // Arrange
        const string sql = @"SELECT id, username
FROM users
WHERE id = $1";

        // Act
        var blocks = SqlQueryParser.SplitIntoQueryBlocks(sql).ToList();

        // Assert
        Assert.Single(blocks);
        Assert.Contains("SELECT id, username", blocks[0]);
    }

    [Fact]
    public void SplitIntoQueryBlocks_WithEmptyContent_ReturnsEmptyList()
    {
        // Arrange
        const string sql = "";

        // Act
        var blocks = SqlQueryParser.SplitIntoQueryBlocks(sql).ToList();

        // Assert
        Assert.Single(blocks); // Empty string is one block
    }
}
