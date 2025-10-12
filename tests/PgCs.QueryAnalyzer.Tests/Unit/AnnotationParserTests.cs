namespace PgCs.QueryAnalyzer.Tests.Unit;

using Parsing;

public sealed class AnnotationParserTests
{
    [Theory]
    [InlineData("-- name: GetUser :one", "GetUser", ReturnCardinality.One)]
    [InlineData("--name:ListUsers:many", "ListUsers", ReturnCardinality.Many)]
    [InlineData("--  name:  CreateUser  :exec", "CreateUser", ReturnCardinality.Exec)]
    [InlineData("-- NAME: UpdateUser :EXECROWS", "UpdateUser", ReturnCardinality.ExecRows)]
    public void Parse_ValidAnnotations_ReturnsCorrectData(
        string comment, string expectedName, ReturnCardinality expectedCardinality)
    {
        // Act
        var result = AnnotationParser.Parse(comment);

        // Assert
        result.Name.Should().Be(expectedName);
        result.Cardinality.Should().Be(expectedCardinality);
    }

    [Fact]
    public void Parse_WithMultilineComments_ExtractsAnnotation()
    {
        // Arrange
        var comments = """
        -- This is a description
        -- name: GetUser :one
        -- Another comment
        """;

        // Act
        var result = AnnotationParser.Parse(comments);

        // Assert
        result.Name.Should().Be("GetUser");
        result.Cardinality.Should().Be(ReturnCardinality.One);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyOrWhitespace_ThrowsArgumentException(string comment)
    {
        // Act
        var act = () => AnnotationParser.Parse(comment);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("-- just a comment")]
    [InlineData("-- name: MissingCardinality")]
    [InlineData("SELECT * FROM users;")]
    public void Parse_InvalidFormat_ThrowsInvalidOperationException(string comment)
    {
        // Act
        var act = () => AnnotationParser.Parse(comment);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*аннотация формата*");
    }

    [Theory]
    [InlineData("-- name: Query :unknown")]
    [InlineData("-- name: Query :select")]
    [InlineData("-- name: Query :insert")]
    public void Parse_InvalidCardinality_ThrowsInvalidOperationException(string comment)
    {
        // Act
        var act = () => AnnotationParser.Parse(comment);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Неизвестная кардинальность*");
    }

    [Theory]
    [InlineData("-- name: Test :one", true)]
    [InlineData("-- name: Test :many", true)]
    [InlineData("-- just a comment", false)]
    [InlineData("SELECT * FROM users;", false)]
    [InlineData("", false)]
    public void HasAnnotation_VariousInputs_ReturnsCorrectResult(string text, bool expected)
    {
        // Act
        var result = AnnotationParser.HasAnnotation(text);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Parse_CaseInsensitiveCardinality_ParsesCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("-- name: Q :ONE", ReturnCardinality.One),
            ("-- name: Q :Many", ReturnCardinality.Many),
            ("-- name: Q :EXEC", ReturnCardinality.Exec),
            ("-- name: Q :ExecRows", ReturnCardinality.ExecRows)
        };

        foreach (var (comment, expected) in testCases)
        {
            // Act
            var result = AnnotationParser.Parse(comment);

            // Assert
            result.Cardinality.Should().Be(expected);
        }
    }
}