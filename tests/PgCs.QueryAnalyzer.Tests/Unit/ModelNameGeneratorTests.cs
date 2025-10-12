namespace PgCs.QueryAnalyzer.Tests.Unit;

using Parsing;
using Helpers;

public sealed class ModelNameGeneratorTests
{
    [Fact]
    public void Generate_EmptyColumns_ReturnsVoid()
    {
        // Arrange
        var columns = Array.Empty<ReturnColumn>();

        // Act
        var result = ModelNameGenerator.Generate(columns);

        // Assert
        result.Should().Be("void");
    }

    [Fact]
    public void Generate_SingleColumn_AppendsSuffixResult()
    {
        // Arrange
        var columns = new[]
        {
            TestDataBuilder.CreateColumn("user_id")
        };

        // Act
        var result = ModelNameGenerator.Generate(columns);

        // Assert
        result.Should().Be("UserIdResult");
    }

    [Fact]
    public void Generate_MultipleColumns_CombinesNames()
    {
        // Arrange
        var columns = new[]
        {
            TestDataBuilder.CreateColumn("id"),
            TestDataBuilder.CreateColumn("username"),
            TestDataBuilder.CreateColumn("email")
        };

        // Act
        var result = ModelNameGenerator.Generate(columns);

        // Assert
        result.Should().Be("IdUsernameEmailResult");
    }

    [Fact]
    public void Generate_MoreThanThreeColumns_UsesOnlyFirstThree()
    {
        // Arrange
        var columns = new[]
        {
            TestDataBuilder.CreateColumn("id"),
            TestDataBuilder.CreateColumn("name"),
            TestDataBuilder.CreateColumn("email"),
            TestDataBuilder.CreateColumn("status"),
            TestDataBuilder.CreateColumn("created_at")
        };

        // Act
        var result = ModelNameGenerator.Generate(columns);

        // Assert
        result.Should().Be("IdNameEmailResult");
    }

    [Theory]
    [InlineData("user_id", "UserIdResult")]
    [InlineData("first_name", "FirstNameResult")]
    [InlineData("created_at", "CreatedAtResult")]
    [InlineData("full_name_data", "FullNameDataResult")]
    public void Generate_SnakeCase_ConvertsToPascalCase(string columnName, string expected)
    {
        // Arrange
        var columns = new[] { TestDataBuilder.CreateColumn(columnName) };

        // Act
        var result = ModelNameGenerator.Generate(columns);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("id", "IdResult")]
    [InlineData("Id", "IdResult")]
    [InlineData("ID", "IDResult")]
    public void Generate_VariousCases_HandlesPascalCase(string columnName, string expected)
    {
        // Arrange
        var columns = new[] { TestDataBuilder.CreateColumn(columnName) };

        // Act
        var result = ModelNameGenerator.Generate(columns);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Generate_NullColumns_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ModelNameGenerator.Generate(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}