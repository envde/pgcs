namespace PgCs.QueryAnalyzer.Tests.Unit;

using System.Linq;
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
        Assert.Equal(2, result.Count);
        Assert.Single(result, p => p.Name == "id" && p.Position == 1);
        Assert.Single(result, p => p.Name == "email" && p.Position == 2);
    }

    [Fact]
    public void Extract_WithAtParameters_FindsAll()
    {
        // Arrange
        var sql = "UPDATE users SET status = @status WHERE id = @id;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(["status", "id"], result.Select(p => p.Name).ToList());
    }

    [Fact]
    public void Extract_MixedSyntax_FindsAll()
    {
        // Arrange
        var sql = "SELECT * FROM t WHERE a = $param1 AND b = @param2 OR c = $param3;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(["param1", "param2", "param3"], result.Select(p => p.Name).ToList());
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
        Assert.Single(result);
        var parameter = result.First();
        Assert.Equal("id", parameter.Name);
        Assert.Equal(1, parameter.Position);
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
        Assert.Equal(3, result.Count);

        var idParam = result.First(p => p.Name == "id");
        Assert.Equal("uuid", idParam.PostgresType);
        Assert.Equal("Guid", idParam.CSharpType);

        var ageParam = result.First(p => p.Name == "age");
        Assert.Equal("integer", ageParam.PostgresType);
        Assert.Equal("int", ageParam.CSharpType);

        var activeParam = result.First(p => p.Name == "active");
        Assert.Equal("boolean", activeParam.PostgresType);
        Assert.Equal("bool", activeParam.CSharpType);
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
        Assert.Single(result);
        var parameter = result.First();
        Assert.Equal(expectedPgType, parameter.PostgresType);
        Assert.Equal(expectedCsType, parameter.CSharpType);
    }

    [Fact]
    public void Extract_WithoutTypeCast_UsesDefaultTextType()
    {
        // Arrange
        var sql = "SELECT * FROM users WHERE name = $name;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        Assert.Single(result);
        var parameter = result.First();
        Assert.Equal("text", parameter.PostgresType);
        Assert.Equal("string", parameter.CSharpType);
    }

    [Fact]
    public void Extract_EmptyQuery_ReturnsEmpty()
    {
        // Arrange
        var sql = "SELECT 1;";

        // Act
        var result = ParameterExtractor.Extract(sql);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Extract_InvalidInput_ThrowsArgumentException(string? sql)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => ParameterExtractor.Extract(sql!));
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
        Assert.Equal(4, result.Count);
        var orderedPositions = result.Select(p => p.Position).ToList();
        var expectedPositions = result.Select(p => p.Position).OrderBy(p => p).ToList();
        Assert.Equal(expectedPositions, orderedPositions);
        Assert.Equal(1, result[0].Position);
        Assert.Equal(4, result[3].Position);
    }
}