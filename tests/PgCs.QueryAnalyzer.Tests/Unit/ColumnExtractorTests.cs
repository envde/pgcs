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
        result.Should().HaveCount(3);
        result.Select(c => c.Name).Should().BeEquivalentTo(["id", "username", "email"]);
    }

    [Fact]
    public void Extract_WithExplicitAliases_UsesAliases()
    {
        // Arrange
        var sql = "SELECT id AS user_id, email AS user_email FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().BeEquivalentTo(["user_id", "user_email"]);
    }

    [Fact]
    public void Extract_WithFunctions_ExtractsNames()
    {
        // Arrange
        var sql = "SELECT COUNT(*) AS total, SUM(amount) AS total_amount FROM orders;";

        // Act
        var result = ColumnExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(c => c.Name == "total");
        result.Should().ContainSingle(c => c.Name == "total_amount");
    }

    [Fact]
    public void Extract_WithNestedFunctions_HandlesParentheses()
    {
        // Arrange
        var sql = "SELECT COALESCE(MAX(created_at), NOW()) AS last_date FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("last_date");
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
        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().BeEquivalentTo(["id", "created_at"]);
    }

    [Fact]
    public void Extract_SelectStar_ReturnsEmpty()
    {
        // Arrange
        var sql = "SELECT * FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

        // Assert
        result.Should().BeEmpty();
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
        result.Should().HaveCount(3);
        
        var total = result.First(c => c.Name == "total");
        total.PostgresType.Should().Be("bigint");
        total.CSharpType.Should().Be("long");
        
        var currentTime = result.First(c => c.Name == "current_time");
        currentTime.CSharpType.Should().Be("DateTime");
        
        var userId = result.First(c => c.Name == "user_id");
        userId.CSharpType.Should().Be("int");
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
        result.Should().HaveCount(1);
        result.First().PostgresType.Should().Be(expectedPgType);
        result.First().CSharpType.Should().Be(expectedCsType);
    }

    [Fact]
    public void Extract_NoSelectOrReturning_ReturnsEmpty()
    {
        // Arrange
        var sql = "UPDATE users SET status = 'active';";

        // Act
        var result = ColumnExtractor.Extract(sql);

        // Assert
        result.Should().BeEmpty();
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
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("is_active");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Extract_InvalidInput_ThrowsArgumentException(string? sql)
    {
        // Act
        var act = () => ColumnExtractor.Extract(sql!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Extract_AllColumnsAreNullable()
    {
        // Arrange
        var sql = "SELECT id, name FROM users;";

        // Act
        var result = ColumnExtractor.Extract(sql);

        // Assert
        result.Should().OnlyContain(c => c.IsNullable == true);
    }
}