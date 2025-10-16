using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.QueryAnalyzer.Tests.Unit;

using Helpers;
using Parsing;

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
        Assert.Equal("void", result);
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
        Assert.Equal("UserIdResult", result);
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
        Assert.Equal("IdUsernameEmailResult", result);
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
        Assert.Equal("IdNameEmailResult", result);
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
        Assert.Equal(expected, result);
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
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_NullColumns_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ModelNameGenerator.Generate(null!));
    }
}