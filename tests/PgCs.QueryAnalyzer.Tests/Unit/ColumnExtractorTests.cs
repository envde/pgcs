namespace PgCs.QueryAnalyzer.Tests.Unit;

using Parsing;

public sealed class ColumnExtractorTests
{
    [Fact]
    public void Extract_SimpleSelect_ReturnsColumns()
    {
        // Arrange
        var sql = "SELECT id, username, email FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Equal(3, result.Count);
    Assert.Equal(["id", "username", "email"], result.Select(c => c.Name).ToList());
    }

    [Fact]
    public void Extract_WithExplicitAliases_UsesAliases()
    {
        // Arrange
        var sql = "SELECT id AS user_id, email AS user_email FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Equal(["user_id", "user_email"], result.Select(c => c.Name).ToList());
    }

    [Fact]
    public void Extract_WithFunctions_ExtractsNames()
    {
        // Arrange
        var sql = "SELECT COUNT(*) AS total, SUM(amount) AS total_amount FROM orders;";

        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Single(result, c => c.Name == "total");
    Assert.Single(result, c => c.Name == "total_amount");
    }

    [Fact]
    public void Extract_WithNestedFunctions_HandlesParentheses()
    {
        // Arrange
        var sql = "SELECT COALESCE(MAX(created_at), NOW()) AS last_date FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Single(result);
    Assert.Equal("last_date", result.First().Name);
    }

    [Fact]
    public void Extract_WithReturningClause_ReturnsColumns()
    {
        // Arrange
        var sql = """
        INSERT INTO users (username, email)
        VALUES ($1, $2)
        RETURNING id, created_at;
        """;

        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Equal(["id", "created_at"], result.Select(c => c.Name).ToList());
    }

    [Fact]
    public void Extract_SelectStar_ReturnsEmpty()
    {
        // Arrange
        var sql = "SELECT * FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Empty(result);
    }

    [Fact]
    public void Extract_WithTypeCasts_InfersTypes()
    {
        // Arrange
        var sql = """
        SELECT 
            COUNT(*)::bigint AS total,
            NOW() AS current_time,
            id::int AS user_id
        FROM users;
        """;

        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Equal(3, result.Count);

    var total = result.First(c => c.Name == "total");
    Assert.Equal("bigint", total.PostgresType);
    Assert.Equal("long", total.CSharpType);

    var currentTime = result.First(c => c.Name == "current_time");
    Assert.Equal("DateTime", currentTime.CSharpType);

    var userId = result.First(c => c.Name == "user_id");
    Assert.Equal("int", userId.CSharpType);
    }

    [Theory]
    [InlineData("SELECT COUNT(*) FROM t;", "bigint", "long")]
    [InlineData("SELECT SUM(amount) FROM t;", "bigint", "long")]
    [InlineData("SELECT AVG(price) FROM t;", "numeric", "decimal")]
    public void Extract_AggregateFunctions_InfersCorrectTypes(
        string sql, string expectedPgType, string expectedCsType)
    {
        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Single(result);
    var column = result.First();
    Assert.Equal(expectedPgType, column.PostgresType);
    Assert.Equal(expectedCsType, column.CSharpType);
    }

    [Fact]
    public void Extract_NoSelectOrReturning_ReturnsEmpty()
    {
        // Arrange
        var sql = "UPDATE users SET status = 'active';";

        // Act
        var result = ColumnExtractor.Extract(sql);

    // Assert
    Assert.Empty(result);
    }

    [Fact]
    public void Extract_WithCaseExpression_ExtractsAlias()
    {
        // Arrange
        var sql = """
        SELECT 
            CASE 
                WHEN status = 'active' THEN 1 
                ELSE 0 
            END AS is_active
        FROM users;
        """;

        // Act
        var result = ColumnExtractor.Extract(sql);

        // Assert
        Assert.Single(result);
        Assert.Equal("is_active", result.First().Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Extract_InvalidInput_ThrowsArgumentException(string? sql)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => ColumnExtractor.Extract(sql!));
    }

    [Fact]
    public void Extract_AllColumnsAreNullable()
    {
        // Arrange
        var sql = "SELECT id, name FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

        // Assert
        Assert.All(result, c => Assert.True(c.IsNullable));
    }
}