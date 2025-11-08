using PgCs.Core.Lexer;

namespace PgCs.Core.Tests.Unit.Lexer;

/// <summary>
/// Tests for Token struct focusing on complex logic and edge cases
/// </summary>
public sealed class TokenTests
{
    [Fact]
    public void Value_MaterializesFromValueMemory()
    {
        // Arrange
        var sourceText = "SELECT id FROM users";
        var token = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = sourceText.AsMemory(7, 2),
            Span = new Token.TextSpan(7, 2),
            Line = 1,
            Column = 8
        };

        // Act
        var value = token.Value;

        // Assert
        Assert.Equal("id", value);
    }

    [Fact]
    public void IsTrivia_WithWhitespace_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Whitespace,
            ValueMemory = "  ".AsMemory(),
            Span = new Token.TextSpan(0, 2),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsTrivia);
        Assert.False(token.IsSignificant);
    }

    [Fact]
    public void IsTrivia_WithLineComment_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.LineComment,
            ValueMemory = "-- comment".AsMemory(),
            Span = new Token.TextSpan(0, 10),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsTrivia);
        Assert.False(token.IsSignificant);
    }

    [Fact]
    public void IsTrivia_WithBlockComment_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.BlockComment,
            ValueMemory = "/* comment */".AsMemory(),
            Span = new Token.TextSpan(0, 13),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsTrivia);
        Assert.False(token.IsSignificant);
    }

    [Fact]
    public void IsSignificant_WithKeyword_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Keyword,
            ValueMemory = "SELECT".AsMemory(),
            Span = new Token.TextSpan(0, 6),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsSignificant);
        Assert.False(token.IsTrivia);
    }

    [Fact]
    public void IsSignificant_WithIdentifier_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "users".AsMemory(),
            Span = new Token.TextSpan(0, 5),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsSignificant);
        Assert.False(token.IsTrivia);
    }

    [Fact]
    public void TextSpan_CalculatesEndCorrectly()
    {
        // Arrange
        var span = new Token.TextSpan(10, 5);

        // Act
        var end = span.End;

        // Assert
        Assert.Equal(15, end);
    }

    [Fact]
    public void TextSpan_WithZeroLength_EndEqualsStart()
    {
        // Arrange
        var span = new Token.TextSpan(10, 0);

        // Act
        var end = span.End;

        // Assert
        Assert.Equal(10, end);
    }

    [Fact]
    public void Token_AllTokenKinds_CanBeCreated()
    {
        // Arrange & Act & Assert
        foreach (TokenKind kind in Enum.GetValues(typeof(TokenKind)))
        {
            var token = new Token
            {
                Kind = kind,
                ValueMemory = "test".AsMemory(),
                Span = new Token.TextSpan(0, 4),
                Line = 1,
                Column = 1
            };

            Assert.Equal(kind, token.Kind);
        }
    }

    [Fact]
    public void Token_WithEmptyValueMemory_HasEmptyValue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.EndOfFile,
            ValueMemory = ReadOnlyMemory<char>.Empty,
            Span = new Token.TextSpan(100, 0),
            Line = 10,
            Column = 1
        };

        // Act
        var value = token.Value;

        // Assert
        Assert.Empty(value);
        Assert.Equal(0, token.Span.Length);
    }
}
