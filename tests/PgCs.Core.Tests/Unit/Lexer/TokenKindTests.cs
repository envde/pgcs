using PgCs.Core.Lexer;

namespace PgCs.Core.Tests.Unit.Lexer;

/// <summary>
/// Comprehensive tests for TokenKind enum
/// Tests all enum values and their properties
/// </summary>
public sealed class TokenKindTests
{
    [Fact]
    public void TokenKind_AllValuesAreDefined()
    {
        // Arrange
        var expectedValues = new[]
        {
            TokenKind.Whitespace,
            TokenKind.LineComment,
            TokenKind.BlockComment,
            TokenKind.Keyword,
            TokenKind.Identifier,
            TokenKind.QuotedIdentifier,
            TokenKind.StringLiteral,
            TokenKind.DollarQuotedString,
            TokenKind.NumericLiteral,
            TokenKind.Operator,
            TokenKind.OpenParen,
            TokenKind.CloseParen,
            TokenKind.OpenBracket,
            TokenKind.CloseBracket,
            TokenKind.Semicolon,
            TokenKind.Comma,
            TokenKind.Dot,
            TokenKind.EndOfFile,
            TokenKind.Unknown
        };

        // Act
        var actualValues = Enum.GetValues<TokenKind>();

        // Assert
        Assert.Equal(expectedValues.Length, actualValues.Length);
        foreach (var expected in expectedValues)
        {
            Assert.Contains(expected, actualValues);
        }
    }

    [Fact]
    public void TokenKind_TriviaTokens_AreCorrectlyIdentified()
    {
        // Arrange
        var triviaTokens = new[]
        {
            TokenKind.Whitespace,
            TokenKind.LineComment,
            TokenKind.BlockComment
        };

        // Act & Assert
        foreach (var kind in triviaTokens)
        {
            var token = new Token
            {
                Kind = kind,
                ValueMemory = "test".AsMemory(),
                Span = new Token.TextSpan(0, 4),
                Line = 1,
                Column = 1
            };

            Assert.True(token.IsTrivia, $"{kind} should be trivia");
        }
    }

    [Fact]
    public void TokenKind_NonTriviaTokens_AreCorrectlyIdentified()
    {
        // Arrange
        var nonTriviaTokens = new[]
        {
            TokenKind.Keyword,
            TokenKind.Identifier,
            TokenKind.QuotedIdentifier,
            TokenKind.StringLiteral,
            TokenKind.DollarQuotedString,
            TokenKind.NumericLiteral,
            TokenKind.Operator,
            TokenKind.OpenParen,
            TokenKind.CloseParen,
            TokenKind.OpenBracket,
            TokenKind.CloseBracket,
            TokenKind.Semicolon,
            TokenKind.Comma,
            TokenKind.Dot,
            TokenKind.EndOfFile,
            TokenKind.Unknown
        };

        // Act & Assert
        foreach (var kind in nonTriviaTokens)
        {
            var token = new Token
            {
                Kind = kind,
                ValueMemory = "test".AsMemory(),
                Span = new Token.TextSpan(0, 4),
                Line = 1,
                Column = 1
            };

            Assert.False(token.IsTrivia, $"{kind} should not be trivia");
        }
    }

    [Fact]
    public void TokenKind_LiteralTokens_AreCorrectlyIdentified()
    {
        // Arrange
        var literalTokens = new[]
        {
            TokenKind.StringLiteral,
            TokenKind.DollarQuotedString,
            TokenKind.NumericLiteral
        };

        // Act & Assert
        foreach (var kind in literalTokens)
        {
            var token = new Token
            {
                Kind = kind,
                ValueMemory = "test".AsMemory(),
                Span = new Token.TextSpan(0, 4),
                Line = 1,
                Column = 1
            };

            Assert.True(token.IsLiteral, $"{kind} should be literal");
        }
    }

    [Fact]
    public void TokenKind_IdentifierTokens_AreCorrectlyIdentified()
    {
        // Arrange
        var identifierTokens = new[]
        {
            TokenKind.Identifier,
            TokenKind.QuotedIdentifier
        };

        // Act & Assert
        foreach (var kind in identifierTokens)
        {
            var token = new Token
            {
                Kind = kind,
                ValueMemory = "test".AsMemory(),
                Span = new Token.TextSpan(0, 4),
                Line = 1,
                Column = 1
            };

            Assert.True(token.IsIdentifier, $"{kind} should be identifier");
        }
    }

    [Fact]
    public void TokenKind_KeywordToken_IsCorrectlyIdentified()
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
        Assert.True(token.IsKeyword);
    }

    [Fact]
    public void TokenKind_OperatorToken_IsCorrectlyIdentified()
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
    public void TokenKind_PunctuationTokens_HaveDistinctValues()
    {
        // Arrange
        var punctuationKinds = new[]
        {
            TokenKind.OpenParen,
            TokenKind.CloseParen,
            TokenKind.OpenBracket,
            TokenKind.CloseBracket,
            TokenKind.Semicolon,
            TokenKind.Comma,
            TokenKind.Dot
        };

        // Act & Assert
        var distinctKinds = punctuationKinds.Distinct().ToList();
        Assert.Equal(punctuationKinds.Length, distinctKinds.Count);
    }

    [Fact]
    public void TokenKind_AllValues_AreUnique()
    {
        // Act
        var allValues = Enum.GetValues<TokenKind>();
        var distinctValues = allValues.Distinct().ToList();

        // Assert
        Assert.Equal(allValues.Length, distinctValues.Count);
    }

    [Fact]
    public void TokenKind_CanBeUsedInSwitch()
    {
        // Arrange
        var allKinds = Enum.GetValues<TokenKind>();

        // Act & Assert
        foreach (var kind in allKinds)
        {
            var category = kind switch
            {
                TokenKind.Whitespace or TokenKind.LineComment or TokenKind.BlockComment => "Trivia",
                TokenKind.Keyword => "Keyword",
                TokenKind.Identifier or TokenKind.QuotedIdentifier => "Identifier",
                TokenKind.StringLiteral or TokenKind.DollarQuotedString or TokenKind.NumericLiteral => "Literal",
                TokenKind.Operator => "Operator",
                TokenKind.OpenParen or TokenKind.CloseParen or TokenKind.OpenBracket or
                TokenKind.CloseBracket or TokenKind.Semicolon or TokenKind.Comma or TokenKind.Dot => "Punctuation",
                TokenKind.EndOfFile => "EOF",
                TokenKind.Unknown => "Unknown",
                _ => throw new ArgumentException($"Unexpected TokenKind: {kind}")
            };

            Assert.NotNull(category);
        }
    }

    [Fact]
    public void TokenKind_ToString_ReturnsName()
    {
        // Arrange & Act & Assert
        Assert.Equal("Whitespace", TokenKind.Whitespace.ToString());
        Assert.Equal("Keyword", TokenKind.Keyword.ToString());
        Assert.Equal("Identifier", TokenKind.Identifier.ToString());
        Assert.Equal("StringLiteral", TokenKind.StringLiteral.ToString());
        Assert.Equal("NumericLiteral", TokenKind.NumericLiteral.ToString());
        Assert.Equal("Operator", TokenKind.Operator.ToString());
        Assert.Equal("EndOfFile", TokenKind.EndOfFile.ToString());
    }

    [Fact]
    public void TokenKind_CanBeCastToInt()
    {
        // Act & Assert
        Assert.True((int)TokenKind.Whitespace >= 0);
        Assert.True((int)TokenKind.EndOfFile >= 0);
        Assert.NotEqual((int)TokenKind.Keyword, (int)TokenKind.Identifier);
    }

    [Fact]
    public void TokenKind_CanBeCompared()
    {
        // Arrange
        var kind1 = TokenKind.Whitespace;
        var kind2 = TokenKind.Whitespace;
        var kind3 = TokenKind.LineComment;

        // Act & Assert
        Assert.Equal(kind1, kind2);
        Assert.NotEqual(kind1, kind3);
        Assert.NotEqual(TokenKind.Keyword, TokenKind.Identifier);
    }

    [Fact]
    public void TokenKind_CommentsAreGrouped()
    {
        // Arrange
        var commentKinds = new[]
        {
            TokenKind.LineComment,
            TokenKind.BlockComment
        };

        // Act & Assert
        foreach (var kind in commentKinds)
        {
            var token = new Token
            {
                Kind = kind,
                ValueMemory = "comment".AsMemory(),
                Span = new Token.TextSpan(0, 7),
                Line = 1,
                Column = 1
            };

            Assert.True(token.IsTrivia);
            Assert.Contains("Comment", kind.ToString());
        }
    }

    [Fact]
    public void TokenKind_BracketsArePaired()
    {
        // Arrange
        var bracketPairs = new (TokenKind open, TokenKind close)[]
        {
            (TokenKind.OpenParen, TokenKind.CloseParen),
            (TokenKind.OpenBracket, TokenKind.CloseBracket)
        };

        // Act & Assert
        foreach (var (open, close) in bracketPairs)
        {
            Assert.Contains("Open", open.ToString());
            Assert.Contains("Close", close.ToString());
            Assert.NotEqual(open, close);
        }
    }
}
