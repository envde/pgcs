

namespace PgCs.QueryAnalyzer.Tests.Unit;

using Parsing;
using PgCs.Common.QueryAnalyzer.Models.Results;

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
    Assert.Equal(expectedName, result.Name);
    Assert.Equal(expectedCardinality, result.Cardinality);
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
    Assert.Equal("GetUser", result.Name);
    Assert.Equal(ReturnCardinality.One, result.Cardinality);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyOrWhitespace_ThrowsArgumentException(string comment)
    {
        // Act
    // Assert
    Assert.Throws<ArgumentException>(() => AnnotationParser.Parse(comment));
    }

    [Theory]
    [InlineData("-- just a comment")]
    [InlineData("-- name: MissingCardinality")]
    [InlineData("SELECT * FROM users;")]
    public void Parse_InvalidFormat_ThrowsInvalidOperationException(string comment)
    {
        // Act
        // Assert
        var ex = Assert.Throws<InvalidOperationException>(() => AnnotationParser.Parse(comment));
        Assert.Contains("аннотация формата", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("-- name: Query :unknown")]
    [InlineData("-- name: Query :select")]
    [InlineData("-- name: Query :insert")]
    public void Parse_InvalidCardinality_ThrowsInvalidOperationException(string comment)
    {
        // Act
        // Assert
        var ex = Assert.Throws<InvalidOperationException>(() => AnnotationParser.Parse(comment));
        Assert.Contains("Неизвестная кардинальность", ex.Message, StringComparison.OrdinalIgnoreCase);
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
    Assert.Equal(expected, result);
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
            Assert.Equal(expected, result.Cardinality);
        }
    }
}