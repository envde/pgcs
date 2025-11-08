using PgCs.Core.Lexer;
using PgCs.Core.Parser;

namespace PgCs.Core.Tests.Unit.Parser;

/// <summary>
/// Comprehensive tests for ParseContext class
/// Tests all navigation and state management methods
/// </summary>
public sealed class ParseContextTests
{
    [Fact]
    public void Constructor_WithTokens_InitializesCorrectly()
    {
        // Arrange
        var sql = "SELECT id FROM users";
        var context = CreateContext(sql);

        // Assert
        Assert.NotNull(context);
        Assert.Equal(0, context.Position);
        Assert.Equal(4, context.TokenCount); // SELECT, id, FROM, users
        Assert.False(context.IsAtEnd);
    }

    [Fact]
    public void Current_ReturnsCurrentToken()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var current = context.Current;

        // Assert
        Assert.Equal(TokenKind.Keyword, current.Kind);
        Assert.Equal("SELECT", current.Value);
    }

    [Fact]
    public void Current_AtEnd_ReturnsDefault()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        
        // Navigate past all tokens
        while (!context.IsAtEnd)
        {
            context.Advance();
        }

        // Act
        var current = context.Current;

        // Assert
        Assert.Equal(default, current);
    }

    [Fact]
    public void Advance_MovesToNextToken()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var previousToken = context.Advance();

        // Assert
        Assert.Equal("SELECT", previousToken.Value);
        Assert.Equal(1, context.Position);
        Assert.Equal("id", context.Current.Value);
    }

    [Fact]
    public void Advance_AtEnd_DoesNotMovePosition()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        
        while (!context.IsAtEnd)
        {
            context.Advance();
        }
        
        var positionBeforeAdvance = context.Position;

        // Act
        context.Advance();

        // Assert
        Assert.Equal(positionBeforeAdvance, context.Position);
    }

    [Fact]
    public void Peek_ReturnsNextToken()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id FROM");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var nextToken = context.Peek();

        // Assert
        Assert.Equal("id", nextToken.Value);
        Assert.Equal(0, context.Position); // Position unchanged
    }

    [Fact]
    public void Peek_WithOffset_ReturnsCorrectToken()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id FROM users");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var token2 = context.Peek(2);

        // Assert
        Assert.Equal("FROM", token2.Value);
        Assert.Equal(0, context.Position);
    }

    [Fact]
    public void Peek_BeyondEnd_ReturnsDefault()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var token = context.Peek(10);

        // Assert
        Assert.Equal(default, token);
    }

    [Fact]
    public void Peek_WithNegativeOffset_ReturnsDefault()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        context.Advance();

        // Act
        var token = context.Peek(-1);

        // Assert
        Assert.Equal(default, token);
    }

    [Fact]
    public void Check_WithMatchingType_ReturnsTrue()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var result = context.Check(TokenKind.Keyword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Check_WithNonMatchingType_ReturnsFalse()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var result = context.Check(TokenKind.Identifier);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Check_AtEnd_ReturnsFalse()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        
        while (!context.IsAtEnd)
        {
            context.Advance();
        }

        // Act
        var result = context.Check(TokenKind.Keyword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Match_WithMatchingType_ConsumesToken()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var matched = context.Match(TokenKind.Keyword);

        // Assert
        Assert.True(matched);
        Assert.Equal(1, context.Position);
        Assert.Equal("id", context.Current.Value);
    }

    [Fact]
    public void Match_WithNonMatchingType_DoesNotConsumeToken()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var matched = context.Match(TokenKind.Identifier);

        // Assert
        Assert.False(matched);
        Assert.Equal(0, context.Position);
    }

    [Fact]
    public void Match_WithMultipleTypes_MatchesFirst()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var matched = context.Match(TokenKind.Identifier, TokenKind.Keyword, TokenKind.Operator);

        // Assert
        Assert.True(matched);
        Assert.Equal(1, context.Position);
    }

    [Fact]
    public void Match_WithMultipleTypes_NoneMatch_ReturnsFalse()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var matched = context.Match(TokenKind.Identifier, TokenKind.Operator, TokenKind.Comma);

        // Assert
        Assert.False(matched);
        Assert.Equal(0, context.Position);
    }

    [Fact]
    public void GetCurrentText_ReturnsTokenText()
    {
        // Arrange
        var sql = "SELECT id FROM users";
        var tokens = CreateTokenList(sql);
        var context = new ParseContext(tokens) { Source = sql.AsMemory() };

        // Act
        var text = context.GetCurrentText();

        // Assert
        Assert.Equal("SELECT", text.ToString());
    }

    [Fact]
    public void GetCurrentText_AtEnd_ReturnsEmpty()
    {
        // Arrange
        var sql = "SELECT";
        var tokens = CreateTokenList(sql);
        var context = new ParseContext(tokens) { Source = sql.AsMemory() };
        
        while (!context.IsAtEnd)
        {
            context.Advance();
        }

        // Act
        var text = context.GetCurrentText();

        // Assert
        Assert.True(text.IsEmpty);
    }

    [Fact]
    public void GetTokenText_ReturnsSpecifiedTokenText()
    {
        // Arrange
        var sql = "SELECT id";
        var tokens = CreateTokenList(sql);
        var context = new ParseContext(tokens) { Source = sql.AsMemory() };
        var token = tokens[1]; // "id"

        // Act
        var text = context.GetTokenText(token);

        // Assert
        Assert.Equal("id", text.ToString());
    }

    [Fact]
    public void SavePosition_ReturnsCurrentPosition()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id FROM");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        context.Advance();
        context.Advance();

        // Act
        var savedPosition = context.SavePosition();

        // Assert
        Assert.Equal(2, savedPosition);
        Assert.Equal(context.Position, savedPosition);
    }

    [Fact]
    public void RestorePosition_RestoresSavedPosition()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id FROM users");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        context.Advance();
        var savedPosition = context.SavePosition();
        
        context.Advance();
        context.Advance();

        // Act
        context.RestorePosition(savedPosition);

        // Assert
        Assert.Equal(1, context.Position);
        Assert.Equal("id", context.Current.Value);
    }

    [Fact]
    public void IsAtEnd_WithMoreTokens_ReturnsFalse()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act & Assert
        Assert.False(context.IsAtEnd);
    }

    [Fact]
    public void IsAtEnd_AfterAllTokens_ReturnsTrue()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        
        while (context.Position < context.TokenCount)
        {
            context.Advance();
        }

        // Act & Assert
        Assert.True(context.IsAtEnd);
    }

    [Fact]
    public void TokenCount_ReturnsCorrectCount()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id FROM users");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var count = context.TokenCount;

        // Assert
        Assert.Equal(tokens.Count, count);
    }

    [Fact]
    public void Position_InitiallyZero()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act & Assert
        Assert.Equal(0, context.Position);
    }

    [Fact]
    public void Context_NavigationThroughCompleteTokenList_WorksCorrectly()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id FROM users");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        var expectedValues = new[] { "SELECT", "id", "FROM", "users" };
        var actualValues = new List<string>();

        // Act
        while (!context.IsAtEnd)
        {
            actualValues.Add(context.Current.Value);
            context.Advance();
        }

        // Assert
        Assert.Equal(expectedValues, actualValues);
    }

    [Fact]
    public void Context_MultiplePositionSaves_CanRestoreToAny()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id FROM users WHERE");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };
        
        context.Advance();
        var pos1 = context.SavePosition();
        
        context.Advance();
        context.Advance();
        var pos2 = context.SavePosition();
        
        context.Advance();

        // Act & Assert
        context.RestorePosition(pos2);
        Assert.Equal("users", context.Current.Value);
        
        context.RestorePosition(pos1);
        Assert.Equal("id", context.Current.Value);
    }

    [Fact]
    public void Context_WithEmptyTokenList_IsAtEnd()
    {
        // Arrange
        var tokens = new List<Token>();
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act & Assert
        Assert.True(context.IsAtEnd);
        Assert.Equal(0, context.TokenCount);
    }

    [Fact]
    public void Context_CheckAndMatch_WorkTogether()
    {
        // Arrange
        var tokens = CreateTokenList("SELECT id");
        var context = new ParseContext(tokens) { Source = ReadOnlyMemory<char>.Empty };

        // Act
        var canMatch = context.Check(TokenKind.Keyword);
        var didMatch = context.Match(TokenKind.Keyword);

        // Assert
        Assert.True(canMatch);
        Assert.True(didMatch);
        Assert.Equal("id", context.Current.Value);
    }

    private static IReadOnlyList<Token> CreateTokenList(string sql)
    {
        var lexer = new SqlLexer(sql);
        var allTokens = lexer.Tokenize(sql);
        
        // Filter out trivia tokens for easier testing
        return allTokens.Where(t => t.IsSignificant && t.Kind != TokenKind.EndOfFile).ToList();
    }

    private static ParseContext CreateContext(string sql)
    {
        var lexer = new SqlLexer(sql);
        var tokens = lexer.Tokenize(sql).Where(t => t.IsSignificant && t.Kind != TokenKind.EndOfFile).ToList();
        return new ParseContext(tokens) { Source = sql.AsMemory() };
    }
}
