namespace PgCs.Core.Tests.Unit.Lexer;

/// <summary>
/// Comprehensive tests for SqlLexer class
/// Tests all tokenization capabilities including keywords, identifiers, literals, operators, comments
/// </summary>
public sealed class SqlLexerTests
{
    [Fact]
    public void Tokenize_WithSimpleSelect_ReturnsCorrectTokens()
    {
        // Arrange
        var sql = "SELECT id FROM users;";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        Assert.NotNull(tokens);
        Assert.NotEmpty(tokens);

        var significantTokens = tokens.Where(t => t.IsSignificant).ToList();
        Assert.Equal(5, significantTokens.Count); // SELECT, id, FROM, users, ;

        Assert.Equal(TokenKind.Keyword, significantTokens[0].Kind);
        Assert.Equal("SELECT", significantTokens[0].Value);
        Assert.Equal(TokenKind.Identifier, significantTokens[1].Kind);
        Assert.Equal("id", significantTokens[1].Value);
        Assert.Equal(TokenKind.Keyword, significantTokens[2].Kind);
        Assert.Equal("FROM", significantTokens[2].Value);
        Assert.Equal(TokenKind.Identifier, significantTokens[3].Kind);
        Assert.Equal("users", significantTokens[3].Value);
        Assert.Equal(TokenKind.Semicolon, significantTokens[4].Kind);

        // Last token should be EOF
        Assert.Equal(TokenKind.EndOfFile, tokens[^1].Kind);
    }

    [Fact]
    public void Tokenize_WithWhitespace_PreservesWhitespaceTokens()
    {
        // Arrange
        var sql = "SELECT  \t\n  id";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var triviaTokens = tokens.Where(t => t.IsTrivia).ToList();
        Assert.NotEmpty(triviaTokens);
        Assert.All(triviaTokens, token => Assert.Equal(TokenKind.Whitespace, token.Kind));
    }

    [Fact]
    public void Tokenize_WithLineComment_CreatesLineCommentToken()
    {
        // Arrange
        var sql = "SELECT id -- This is a comment\nFROM users";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var commentToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.LineComment);
        Assert.NotEqual(default, commentToken);
        Assert.Equal("-- This is a comment", commentToken.Value);
        Assert.True(commentToken.IsTrivia);
    }

    [Fact]
    public void Tokenize_WithBlockComment_CreatesBlockCommentToken()
    {
        // Arrange
        var sql = "SELECT /* multi\nline\ncomment */ id";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var commentToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.BlockComment);
        Assert.NotEqual(default, commentToken);
        Assert.Contains("multi", commentToken.Value);
        Assert.Contains("line", commentToken.Value);
        Assert.Contains("comment", commentToken.Value);
        Assert.True(commentToken.IsTrivia);
    }

    [Fact]
    public void Tokenize_WithNestedBlockComments_HandlesNesting()
    {
        // Arrange
        var sql = "SELECT /* outer /* inner */ still outer */ id";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var commentToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.BlockComment);
        Assert.NotEqual(default, commentToken);
        Assert.Contains("outer", commentToken.Value);
        Assert.Contains("inner", commentToken.Value);
    }

    [Fact]
    public void Tokenize_WithStringLiteral_CreatesStringLiteralToken()
    {
        // Arrange
        var sql = "SELECT 'hello world'";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var stringToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.StringLiteral);
        Assert.NotEqual(default, stringToken);
        Assert.Equal("'hello world'", stringToken.Value);
        Assert.True(stringToken.IsLiteral);
    }

    [Fact]
    public void Tokenize_WithEscapedQuotes_HandlesCorrectly()
    {
        // Arrange
        var sql = "SELECT 'don''t'";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var stringToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.StringLiteral);
        Assert.NotEqual(default, stringToken);
        Assert.Equal("'don''t'", stringToken.Value);
    }

    [Fact]
    public void Tokenize_WithDollarQuotedString_CreatesDollarQuotedToken()
    {
        // Arrange
        var sql = "SELECT $$hello world$$";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var stringToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.DollarQuotedString);
        Assert.NotEqual(default, stringToken);
        Assert.Equal("$$hello world$$", stringToken.Value);
        Assert.True(stringToken.IsLiteral);
    }

    [Fact]
    public void Tokenize_WithTaggedDollarQuotedString_HandlesTag()
    {
        // Arrange
        var sql = "SELECT $tag$hello world$tag$";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var stringToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.DollarQuotedString);
        Assert.NotEqual(default, stringToken);
        Assert.Equal("$tag$hello world$tag$", stringToken.Value);
    }

    [Fact]
    public void Tokenize_WithQuotedIdentifier_CreatesQuotedIdentifierToken()
    {
        // Arrange
        var sql = "SELECT \"Table Name\"";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var identifierToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.QuotedIdentifier);
        Assert.NotEqual(default, identifierToken);
        Assert.Equal("\"Table Name\"", identifierToken.Value);
        Assert.True(identifierToken.IsIdentifier);
    }

    [Fact]
    public void Tokenize_WithNumericLiterals_HandlesAllFormats()
    {
        // Arrange
        var sql = "SELECT 123, 45.67, 1.23e10, 0x1A, 0b101, 0o17";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var numericTokens = tokens.Where(t => t.Kind == TokenKind.NumericLiteral).ToList();
        Assert.Equal(6, numericTokens.Count);

        Assert.Equal("123", numericTokens[0].Value);
        Assert.Equal("45.67", numericTokens[1].Value);
        Assert.Equal("1.23e10", numericTokens[2].Value);
        Assert.Equal("0x1A", numericTokens[3].Value);
        Assert.Equal("0b101", numericTokens[4].Value);
        Assert.Equal("0o17", numericTokens[5].Value);

        Assert.All(numericTokens, token => Assert.True(token.IsLiteral));
    }

    [Fact]
    public void Tokenize_WithOperators_RecognizesAllOperators()
    {
        // Arrange
        var sql = "SELECT * WHERE a = b AND c <> d OR e >= f AND g <= h";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var operatorTokens = tokens.Where(t => t.Kind == TokenKind.Operator).ToList();
        Assert.NotEmpty(operatorTokens);
        Assert.Contains(operatorTokens, t => t.Value == "*");
        Assert.Contains(operatorTokens, t => t.Value == "=");
        Assert.Contains(operatorTokens, t => t.Value == "<>");
        Assert.Contains(operatorTokens, t => t.Value == ">=");
        Assert.Contains(operatorTokens, t => t.Value == "<=");
    }

    [Fact]
    public void Tokenize_WithPostgreSQLOperators_HandlesSpecialOperators()
    {
        // Arrange
        var sql = "SELECT col::text, jsonb @> '{}'::jsonb, arr <@ other";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var operatorTokens = tokens.Where(t => t.Kind == TokenKind.Operator).ToList();
        Assert.Contains(operatorTokens, t => t.Value == "::");
        Assert.Contains(operatorTokens, t => t.Value == "@>");
        Assert.Contains(operatorTokens, t => t.Value == "<@");
    }

    [Fact]
    public void Tokenize_WithPunctuation_RecognizesAllPunctuation()
    {
        // Arrange
        var sql = "SELECT (id, name) FROM users[1];";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        Assert.Contains(tokens, t => t.Kind == TokenKind.OpenParen);
        Assert.Contains(tokens, t => t.Kind == TokenKind.CloseParen);
        Assert.Contains(tokens, t => t.Kind == TokenKind.OpenBracket);
        Assert.Contains(tokens, t => t.Kind == TokenKind.CloseBracket);
        Assert.Contains(tokens, t => t.Kind == TokenKind.Comma);
        Assert.Contains(tokens, t => t.Kind == TokenKind.Semicolon);
        Assert.Contains(tokens, t => t.Kind == TokenKind.Dot);
    }

    [Fact]
    public void Tokenize_WithKeywords_IdentifiesAllCommonKeywords()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INTEGER PRIMARY KEY, name VARCHAR(255))";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var keywordTokens = tokens.Where(t => t.Kind == TokenKind.Keyword).ToList();
        Assert.Contains(keywordTokens, t => t.Value.Equals("CREATE", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keywordTokens, t => t.Value.Equals("TABLE", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keywordTokens, t => t.Value.Equals("INTEGER", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keywordTokens, t => t.Value.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keywordTokens, t => t.Value.Equals("KEY", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keywordTokens, t => t.Value.Equals("VARCHAR", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Tokenize_WithCaseInsensitiveKeywords_RecognizesRegardlessOfCase()
    {
        // Arrange
        var sql = "select SELECT SeLeCt";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var keywordTokens = tokens.Where(t => t.Kind == TokenKind.Keyword).ToList();
        Assert.Equal(3, keywordTokens.Count);
        Assert.All(keywordTokens, token => Assert.True(token.IsKeyword));
    }

    [Fact]
    public void Tokenize_WithIdentifiers_RecognizesValidIdentifiers()
    {
        // Arrange
        var sql = "SELECT user_id, _private, table123, var$1";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var identifierTokens = tokens.Where(t => t.Kind == TokenKind.Identifier).ToList();
        Assert.Contains(identifierTokens, t => t.Value == "user_id");
        Assert.Contains(identifierTokens, t => t.Value == "_private");
        Assert.Contains(identifierTokens, t => t.Value == "table123");
        Assert.Contains(identifierTokens, t => t.Value == "var$1");
    }

    [Fact]
    public void Tokenize_WithComplexQuery_TokenizesCorrectly()
    {
        // Arrange
        var sql = @"
            WITH cte AS (
                SELECT id, name
                FROM users
                WHERE age > 18
            )
            SELECT * FROM cte
            UNION ALL
            SELECT id, name FROM guests;
        ";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        Assert.NotEmpty(tokens);
        var significantTokens = tokens.Where(t => t.IsSignificant).ToList();
        Assert.NotEmpty(significantTokens);

        // Verify some key tokens exist
        Assert.Contains(significantTokens, t => t.Value.Equals("WITH", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(significantTokens, t => t.Value.Equals("AS", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(significantTokens, t => t.Value.Equals("UNION", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(significantTokens, t => t.Value.Equals("ALL", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Tokenize_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var lexer = new SqlLexer("valid");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => lexer.Tokenize(""));
    }

    [Fact]
    public void Tokenize_WithNullString_ThrowsArgumentException()
    {
        // Arrange
        var lexer = new SqlLexer("valid");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => lexer.Tokenize(null!));
    }

    [Fact]
    public void Tokenize_WithWhitespaceOnly_ReturnsWhitespaceAndEOF()
    {
        // Arrange
        var sql = "   \t\n  ";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        Assert.Equal(2, tokens.Count); // Whitespace + EOF
        Assert.Equal(TokenKind.Whitespace, tokens[0].Kind);
        Assert.Equal(TokenKind.EndOfFile, tokens[1].Kind);
    }

    [Fact]
    public void Constructor_WithNullSourceText_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SqlLexer(null!));
    }

    [Fact]
    public void Constructor_WithEmptySourceText_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SqlLexer(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceOnlySourceText_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SqlLexer("   "));
    }

    [Fact]
    public void Tokenize_VerifiesTokenPositions_AreSequential()
    {
        // Arrange
        var sql = "SELECT id FROM users";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var significantTokens = tokens.Where(t => t.IsSignificant).ToList();
        for (int i = 0; i < significantTokens.Count - 1; i++)
        {
            Assert.True(significantTokens[i].Span.Start < significantTokens[i + 1].Span.Start);
        }
    }

    [Fact]
    public void Tokenize_VerifiesLineAndColumnNumbers_AreCorrect()
    {
        // Arrange
        var sql = "SELECT\nid\nFROM\nusers";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var selectToken = tokens.First(t => t.Value == "SELECT");
        Assert.Equal(1, selectToken.Line);
        Assert.Equal(1, selectToken.Column);

        var idToken = tokens.First(t => t.Value == "id");
        Assert.Equal(2, idToken.Line);
        Assert.Equal(1, idToken.Column);

        var fromToken = tokens.First(t => t.Value == "FROM");
        Assert.Equal(3, fromToken.Line);
        Assert.Equal(1, fromToken.Column);

        var usersToken = tokens.First(t => t.Value == "users");
        Assert.Equal(4, usersToken.Line);
        Assert.Equal(1, usersToken.Column);
    }

    [Fact]
    public void Tokenize_WithMultipleStatements_TokenizesAll()
    {
        // Arrange
        var sql = "SELECT id FROM users; DELETE FROM logs; UPDATE status SET active = true;";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var semicolonTokens = tokens.Where(t => t.Kind == TokenKind.Semicolon).ToList();
        Assert.Equal(3, semicolonTokens.Count);

        var keywordTokens = tokens.Where(t => t.Kind == TokenKind.Keyword).ToList();
        Assert.Contains(keywordTokens, t => t.Value.Equals("SELECT", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keywordTokens, t => t.Value.Equals("DELETE", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keywordTokens, t => t.Value.Equals("UPDATE", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Tokenize_WithScientificNotation_HandlesExponent()
    {
        // Arrange
        var sql = "SELECT 1.5e10, 2E-5, 3.14E+2";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var numericTokens = tokens.Where(t => t.Kind == TokenKind.NumericLiteral).ToList();
        Assert.Equal(3, numericTokens.Count);
        Assert.Equal("1.5e10", numericTokens[0].Value);
        Assert.Equal("2E-5", numericTokens[1].Value);
        Assert.Equal("3.14E+2", numericTokens[2].Value);
    }

    [Fact]
    public void Tokenize_WithDollarOperator_WhenNotDollarQuoted_RecognizesAsOperator()
    {
        // Arrange
        var sql = "SELECT $ FROM table";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var dollarToken = tokens.FirstOrDefault(t => t.Value == "$");
        Assert.NotEqual(default, dollarToken);
        Assert.Equal(TokenKind.Operator, dollarToken.Kind);
    }

    [Fact]
    public void Tokenize_WithUnknownCharacter_CreatesUnknownToken()
    {
        // Arrange
        var sql = "SELECT ` FROM table";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var unknownToken = tokens.FirstOrDefault(t => t.Kind == TokenKind.Unknown);
        Assert.NotEqual(default, unknownToken);
    }

    [Fact]
    public void Tokenize_PreservesOriginalText_ThroughValueMemory()
    {
        // Arrange
        var sql = "SELECT id FROM users";
        var lexer = new SqlLexer(sql);

        // Act
        var tokens = lexer.Tokenize(sql);

        // Assert
        var reconstructed = string.Concat(tokens.Where(t => t.Kind != TokenKind.EndOfFile).Select(t => t.Value));
        Assert.Equal(sql, reconstructed);
    }
}
