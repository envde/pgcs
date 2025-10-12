namespace PgCs.QueryAnalyzer.Tests.Unit;

using Parsing;

public sealed class ParameterExtractorTests
{
    [Fact]
    public void Extract_WithDollarParameters_FindsAll()
    {
        // Arrange
        var sql = "SELECT * FROM users WHERE id = $id AND email = $email;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.Name == "id" && p.Position == 1);
        result.Should().ContainSingle(p => p.Name == "email" && p.Position == 2);
    }

    [Fact]
    public void Extract_WithAtParameters_FindsAll()
    {
        // Arrange
        var sql = "UPDATE users SET status = @status WHERE id = @id;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().BeEquivalentTo(["status", "id"]);
    }

    [Fact]
    public void Extract_MixedSyntax_FindsAll()
    {
        // Arrange
        var sql = "SELECT * FROM t WHERE a = $param1 AND b = @param2 OR c = $param3;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(3);
        result.Select(p => p.Name).Should().BeEquivalentTo(["param1", "param2", "param3"]);
    }

    [Fact]
    public void Extract_WithDuplicateParameters_Deduplicates()
    {
        // Arrange
        var sql = """
        SELECT * FROM users 
        WHERE id = $id OR parent_id = $id OR $id IS NOT NULL;
        """;

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("id");
        result.First().Position.Should().Be(1);
    }

    [Fact]
    public void Extract_WithTypeCasts_InfersTypes()
    {
        // Arrange
        var sql = """
        SELECT * FROM users 
        WHERE id = $id::uuid 
          AND age = @age::int 
          AND active = $active::boolean;
        """;

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(3);
        
        var idParam = result.First(p => p.Name == "id");
        idParam.PostgresType.Should().Be("uuid");
        idParam.CSharpType.Should().Be("Guid");
        
        var ageParam = result.First(p => p.Name == "age");
        ageParam.PostgresType.Should().Be("integer");
        ageParam.CSharpType.Should().Be("int");
        
        var activeParam = result.First(p => p.Name == "active");
        activeParam.PostgresType.Should().Be("boolean");
        activeParam.CSharpType.Should().Be("bool");
    }

    [Theory]
    [InlineData("SELECT * WHERE a = $p::bigint", "bigint", "long")]
    [InlineData("SELECT * WHERE a = @p::timestamp", "timestamp", "DateTime")]
    [InlineData("SELECT * WHERE a = $p::numeric", "numeric", "decimal")]
    [InlineData("SELECT * WHERE a = $p::decimal", "numeric", "decimal")]
    public void Extract_VariousTypeCasts_InfersCorrectly(
        string sql, string expectedPgType, string expectedCsType)
    {
        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(1);
        result.First().PostgresType.Should().Be(expectedPgType);
        result.First().CSharpType.Should().Be(expectedCsType);
    }

    [Fact]
    public void Extract_WithoutTypeCast_UsesDefaultTextType()
    {
        // Arrange
        var sql = "SELECT * FROM users WHERE name = $name;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(1);
        result.First().PostgresType.Should().Be("text");
        result.First().CSharpType.Should().Be("string");
    }

    [Fact]
    public void Extract_EmptyQuery_ReturnsEmpty()
    {
        // Arrange
        var sql = "SELECT 1;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Extract_InvalidInput_ThrowsArgumentException(string? sql)
    {
        // Act
        var act = () => ParameterExtractor.Extract(sql!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Extract_ComplexQuery_MaintainsPositionOrder()
    {
        // Arrange
        var sql = """
        INSERT INTO orders (user_id, total, status, created_at)
        VALUES ($user_id, $total, $status, $created_at)
        RETURNING id;
        """;

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        result.Should().HaveCount(4);
        result.Should().BeInAscendingOrder(p => p.Position);
        result[0].Position.Should().Be(1);
        result[3].Position.Should().Be(4);
    }
}