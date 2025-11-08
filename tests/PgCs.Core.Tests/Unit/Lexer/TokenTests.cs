using PgCs.Core.Lexer;

namespace PgCs.Core.Tests.Unit.Lexer;

/// <summary>
/// Comprehensive tests for Token struct
/// Tests all properties, methods, and helper functions
/// </summary>
public sealed class TokenTests
{
    [Fact]
    public void Token_WithRequiredProperties_CreatesSuccessfully()
    {
        // Arrange & Act
        var token = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "test".AsMemory(),
            Span = new Token.TextSpan(0, 4),
            Line = 1,
            Column = 1
        };

        // Assert
        Assert.Equal(TokenKind.Identifier, token.Kind);
        Assert.Equal("test", token.Value);
        Assert.Equal(4, token.Span.Length);
        Assert.Equal(1, token.Line);
        Assert.Equal(1, token.Column);
    }

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
    public void IsKeyword_WithKeywordToken_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Keyword,
            ValueMemory = "CREATE".AsMemory(),
            Span = new Token.TextSpan(0, 6),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsKeyword);
    }

    [Fact]
    public void IsKeyword_WithNonKeywordToken_ReturnsFalse()
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
        Assert.False(token.IsKeyword);
    }

    [Fact]
    public void IsIdentifier_WithIdentifierToken_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "user_id".AsMemory(),
            Span = new Token.TextSpan(0, 7),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsIdentifier);
    }

    [Fact]
    public void IsIdentifier_WithQuotedIdentifierToken_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.QuotedIdentifier,
            ValueMemory = "\"Table Name\"".AsMemory(),
            Span = new Token.TextSpan(0, 12),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsIdentifier);
    }

    [Fact]
    public void IsIdentifier_WithNonIdentifierToken_ReturnsFalse()
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
        Assert.False(token.IsIdentifier);
    }

    [Fact]
    public void IsLiteral_WithStringLiteral_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.StringLiteral,
            ValueMemory = "'hello'".AsMemory(),
            Span = new Token.TextSpan(0, 7),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsLiteral);
    }

    [Fact]
    public void IsLiteral_WithDollarQuotedString_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.DollarQuotedString,
            ValueMemory = "$$hello$$".AsMemory(),
            Span = new Token.TextSpan(0, 9),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsLiteral);
    }

    [Fact]
    public void IsLiteral_WithNumericLiteral_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.NumericLiteral,
            ValueMemory = "123.45".AsMemory(),
            Span = new Token.TextSpan(0, 6),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsLiteral);
    }

    [Fact]
    public void IsLiteral_WithNonLiteralToken_ReturnsFalse()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "test".AsMemory(),
            Span = new Token.TextSpan(0, 4),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.False(token.IsLiteral);
    }

    [Fact]
    public void IsOperator_WithOperatorToken_ReturnsTrue()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Operator,
            ValueMemory = "=".AsMemory(),
            Span = new Token.TextSpan(0, 1),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.True(token.IsOperator);
    }

    [Fact]
    public void IsOperator_WithNonOperatorToken_ReturnsFalse()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "test".AsMemory(),
            Span = new Token.TextSpan(0, 4),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.False(token.IsOperator);
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
    public void Token_IsRecordStruct_SupportsValueSemantics()
    {
        // Arrange
        var token1 = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "test".AsMemory(),
            Span = new Token.TextSpan(0, 4),
            Line = 1,
            Column = 1
        };

        var token2 = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "test".AsMemory(),
            Span = new Token.TextSpan(0, 4),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.Equal(token1, token2);
    }

    [Fact]
    public void Token_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var token1 = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "test1".AsMemory(),
            Span = new Token.TextSpan(0, 5),
            Line = 1,
            Column = 1
        };

        var token2 = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "test2".AsMemory(),
            Span = new Token.TextSpan(0, 5),
            Line = 1,
            Column = 1
        };

        // Act & Assert
        Assert.NotEqual(token1, token2);
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

    [Fact]
    public void TextSpan_IsRecordStruct_SupportsValueSemantics()
    {
        // Arrange
        var span1 = new Token.TextSpan(10, 5);
        var span2 = new Token.TextSpan(10, 5);

        // Act & Assert
        Assert.Equal(span1, span2);
    }

    [Fact]
    public void TextSpan_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var span1 = new Token.TextSpan(10, 5);
        var span2 = new Token.TextSpan(10, 6);

        // Act & Assert
        Assert.NotEqual(span1, span2);
    }

    [Fact]
    public void Token_LineAndColumn_TrackPositionCorrectly()
    {
        // Arrange
        var token = new Token
        {
            Kind = TokenKind.Identifier,
            ValueMemory = "test".AsMemory(),
            Span = new Token.TextSpan(50, 4),
            Line = 5,
            Column = 10
        };

        // Act & Assert
        Assert.Equal(5, token.Line);
        Assert.Equal(10, token.Column);
    }
}
