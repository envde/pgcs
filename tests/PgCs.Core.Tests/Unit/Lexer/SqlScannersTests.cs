using PgCs.Core.Lexer;

namespace PgCs.Core.Tests.Unit.Lexer;

/// <summary>
/// Comprehensive tests for SqlScanners static class
/// Tests all scanner methods for different token types
/// </summary>
public sealed class SqlScannersTests
{
    [Fact]
    public void ScanLineComment_BasicComment_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("-- comment\nSELECT");

        // Act
        var (type, length) = SqlScanners.ScanLineComment(cursor);

        // Assert
        Assert.Equal(TokenKind.LineComment, type);
        Assert.Equal(10, length); // "-- comment"
        Assert.Equal(10, cursor.Position);
    }

    [Fact]
    public void ScanLineComment_CommentAtEndOfFile_ScansToEnd()
    {
        // Arrange
        var cursor = new TextCursor("-- comment");

        // Act
        var (type, length) = SqlScanners.ScanLineComment(cursor);

        // Assert
        Assert.Equal(TokenKind.LineComment, type);
        Assert.Equal(10, length);
        Assert.True(cursor.IsAtEnd());
    }

    [Fact]
    public void ScanLineComment_EmptyComment_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("--\nSELECT");

        // Act
        var (type, length) = SqlScanners.ScanLineComment(cursor);

        // Assert
        Assert.Equal(TokenKind.LineComment, type);
        Assert.Equal(2, length);
    }

    [Fact]
    public void ScanBlockComment_SimpleComment_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("/* comment */SELECT");

        // Act
        var (type, length) = SqlScanners.ScanBlockComment(cursor);

        // Assert
        Assert.Equal(TokenKind.BlockComment, type);
        Assert.Equal(13, length); // "/* comment */"
        Assert.Equal('S', cursor.Current);
    }

    [Fact]
    public void ScanBlockComment_MultilineComment_ScansCompletely()
    {
        // Arrange
        var cursor = new TextCursor("/* line1\nline2\nline3 */SELECT");

        // Act
        var (type, length) = SqlScanners.ScanBlockComment(cursor);

        // Assert
        Assert.Equal(TokenKind.BlockComment, type);
        Assert.Equal(23, length);
        Assert.Equal('S', cursor.Current);
    }

    [Fact]
    public void ScanBlockComment_NestedComments_HandlesNesting()
    {
        // Arrange
        var cursor = new TextCursor("/* outer /* inner */ still outer */SELECT");

        // Act
        var (type, length) = SqlScanners.ScanBlockComment(cursor);

        // Assert
        Assert.Equal(TokenKind.BlockComment, type);
        Assert.Equal(35, length);
        Assert.Equal('S', cursor.Current);
    }

    [Fact]
    public void ScanBlockComment_DeeplyNested_HandlesMultipleLevels()
    {
        // Arrange
        var cursor = new TextCursor("/* 1 /* 2 /* 3 */ 2 */ 1 */X");

        // Act
        var (type, length) = SqlScanners.ScanBlockComment(cursor);

        // Assert
        Assert.Equal(TokenKind.BlockComment, type);
        Assert.Equal(27, length);
        Assert.Equal('X', cursor.Current);
    }

    [Fact]
    public void ScanBlockComment_UnterminatedComment_ScansToEnd()
    {
        // Arrange
        var cursor = new TextCursor("/* unterminated comment");

        // Act
        var (type, length) = SqlScanners.ScanBlockComment(cursor);

        // Assert
        Assert.Equal(TokenKind.BlockComment, type);
        Assert.True(cursor.IsAtEnd());
    }

    [Fact]
    public void ScanNumber_Integer_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("12345 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(5, length);
        Assert.Equal(' ', cursor.Current);
    }

    [Fact]
    public void ScanNumber_Decimal_ScansCompletely()
    {
        // Arrange
        var cursor = new TextCursor("123.456 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(7, length);
    }

    [Fact]
    public void ScanNumber_ScientificNotation_ScansWithExponent()
    {
        // Arrange
        var cursor = new TextCursor("1.5e10 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(6, length);
    }

    [Fact]
    public void ScanNumber_ScientificWithPositiveExponent_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("3.14E+2 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(7, length);
    }

    [Fact]
    public void ScanNumber_ScientificWithNegativeExponent_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("2E-5 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(4, length);
    }

    [Fact]
    public void ScanNumber_Hexadecimal_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("0x1A2B FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(6, length);
    }

    [Fact]
    public void ScanNumber_HexadecimalUppercase_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("0X1A2B FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(6, length);
    }

    [Fact]
    public void ScanNumber_Binary_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("0b101010 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(8, length);
    }

    [Fact]
    public void ScanNumber_BinaryUppercase_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("0B101010 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(8, length);
    }

    [Fact]
    public void ScanNumber_Octal_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("0o177 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(5, length);
    }

    [Fact]
    public void ScanNumber_OctalUppercase_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("0O177 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(5, length);
    }

    [Fact]
    public void ScanNumber_Zero_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("0 FROM");

        // Act
        var (type, length) = SqlScanners.ScanNumber(cursor);

        // Assert
        Assert.Equal(TokenKind.NumericLiteral, type);
        Assert.Equal(1, length);
    }

    [Fact]
    public void ScanOperator_SingleCharOperator_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("= value");

        // Act
        var (type, length) = SqlScanners.ScanOperator(cursor);

        // Assert
        Assert.Equal(TokenKind.Operator, type);
        Assert.Equal(1, length);
    }

    [Fact]
    public void ScanOperator_TwoCharOperator_ScansCompletely()
    {
        // Arrange
        var cursor = new TextCursor("<> value");

        // Act
        var (type, length) = SqlScanners.ScanOperator(cursor);

        // Assert
        Assert.Equal(TokenKind.Operator, type);
        Assert.Equal(2, length);
    }

    [Fact]
    public void ScanOperator_PostgreSQLCast_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("::text");

        // Act
        var (type, length) = SqlScanners.ScanOperator(cursor);

        // Assert
        Assert.Equal(TokenKind.Operator, type);
        Assert.Equal(2, length);
    }

    [Fact]
    public void ScanOperator_JSONOperator_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("@> value");

        // Act
        var (type, length) = SqlScanners.ScanOperator(cursor);

        // Assert
        Assert.Equal(TokenKind.Operator, type);
        Assert.Equal(2, length);
    }

    [Fact]
    public void ScanOperator_ThreeCharOperator_ScansCompletely()
    {
        // Arrange
        var cursor = new TextCursor("!~~ value");

        // Act
        var (type, length) = SqlScanners.ScanOperator(cursor);

        // Assert
        Assert.Equal(TokenKind.Operator, type);
        Assert.Equal(3, length);
    }

    [Fact]
    public void ScanStringLiteral_SimpleString_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("'hello' FROM");

        // Act
        var (type, length) = SqlScanners.ScanStringLiteral(cursor);

        // Assert
        Assert.Equal(TokenKind.StringLiteral, type);
        Assert.Equal(7, length);
    }

    [Fact]
    public void ScanStringLiteral_WithEscapedQuotes_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("'don''t' FROM");

        // Act
        var (type, length) = SqlScanners.ScanStringLiteral(cursor);

        // Assert
        Assert.Equal(TokenKind.StringLiteral, type);
        Assert.Equal(8, length); // 'don''t'
    }

    [Fact]
    public void ScanStringLiteral_EmptyString_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("'' FROM");

        // Act
        var (type, length) = SqlScanners.ScanStringLiteral(cursor);

        // Assert
        Assert.Equal(TokenKind.StringLiteral, type);
        Assert.Equal(2, length);
    }

    [Fact]
    public void ScanStringLiteral_MultilineString_ScansCompletely()
    {
        // Arrange
        var cursor = new TextCursor("'line1\nline2' FROM");

        // Act
        var (type, length) = SqlScanners.ScanStringLiteral(cursor);

        // Assert
        Assert.Equal(TokenKind.StringLiteral, type);
        Assert.Equal(13, length);
    }

    [Fact]
    public void ScanStringLiteral_UnterminatedString_ScansToEnd()
    {
        // Arrange
        var cursor = new TextCursor("'unterminated");

        // Act
        var (type, length) = SqlScanners.ScanStringLiteral(cursor);

        // Assert
        Assert.Equal(TokenKind.StringLiteral, type);
        Assert.True(cursor.IsAtEnd());
    }

    [Fact]
    public void ScanDollarQuotedString_Simple_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("$$hello$$ FROM");

        // Act
        var (type, length) = SqlScanners.ScanDollarQuotedString(cursor);

        // Assert
        Assert.Equal(TokenKind.DollarQuotedString, type);
        Assert.Equal(9, length);
    }

    [Fact]
    public void ScanDollarQuotedString_WithTag_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("$tag$hello$tag$ FROM");

        // Act
        var (type, length) = SqlScanners.ScanDollarQuotedString(cursor);

        // Assert
        Assert.Equal(TokenKind.DollarQuotedString, type);
        Assert.Equal(15, length);
    }

    [Fact]
    public void ScanDollarQuotedString_Multiline_ScansCompletely()
    {
        // Arrange
        var cursor = new TextCursor("$$line1\nline2\nline3$$ FROM");

        // Act
        var (type, length) = SqlScanners.ScanDollarQuotedString(cursor);

        // Assert
        Assert.Equal(TokenKind.DollarQuotedString, type);
        Assert.Equal(21, length);
    }

    [Fact]
    public void ScanDollarQuotedString_WithQuotesInside_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("$$can contain 'quotes' and \"double quotes\"$$ FROM");

        // Act
        var (type, length) = SqlScanners.ScanDollarQuotedString(cursor);

        // Assert
        Assert.Equal(TokenKind.DollarQuotedString, type);
        Assert.Equal(44, length);
    }

    [Fact]
    public void ScanDollarQuotedString_NotDollarQuoted_ReturnsOperator()
    {
        // Arrange
        var cursor = new TextCursor("$ FROM");

        // Act
        var (type, length) = SqlScanners.ScanDollarQuotedString(cursor);

        // Assert
        Assert.Equal(TokenKind.Operator, type);
        Assert.Equal(1, length);
    }

    [Fact]
    public void ScanDollarQuotedString_UnterminatedTag_ReturnsOperator()
    {
        // Arrange
        var cursor = new TextCursor("$tag FROM");

        // Act
        var (type, length) = SqlScanners.ScanDollarQuotedString(cursor);

        // Assert
        Assert.Equal(TokenKind.Operator, type);
        Assert.Equal(1, length);
    }

    [Fact]
    public void ScanDollarQuotedString_EmptyContent_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("$$$$ FROM");

        // Act
        var (type, length) = SqlScanners.ScanDollarQuotedString(cursor);

        // Assert
        Assert.Equal(TokenKind.DollarQuotedString, type);
        Assert.Equal(4, length);
    }

    [Fact]
    public void ScanDollarQuotedString_UnterminatedString_ScansToEnd()
    {
        // Arrange
        var cursor = new TextCursor("$$unterminated");

        // Act
        var (type, length) = SqlScanners.ScanDollarQuotedString(cursor);

        // Assert
        Assert.Equal(TokenKind.DollarQuotedString, type);
        Assert.True(cursor.IsAtEnd());
    }

    [Fact]
    public void ScanQuotedIdentifier_Simple_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("\"Table Name\" FROM");

        // Act
        var (type, length) = SqlScanners.ScanQuotedIdentifier(cursor);

        // Assert
        Assert.Equal(TokenKind.QuotedIdentifier, type);
        Assert.Equal(12, length); // "Table Name" = 1 + 10 + 1 = 12
    }

    [Fact]
    public void ScanQuotedIdentifier_WithSpaces_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("\"My Table\" FROM");

        // Act
        var (type, length) = SqlScanners.ScanQuotedIdentifier(cursor);

        // Assert
        Assert.Equal(TokenKind.QuotedIdentifier, type);
        Assert.Equal(10, length);
    }

    [Fact]
    public void ScanQuotedIdentifier_Empty_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("\"\" FROM");

        // Act
        var (type, length) = SqlScanners.ScanQuotedIdentifier(cursor);

        // Assert
        Assert.Equal(TokenKind.QuotedIdentifier, type);
        Assert.Equal(2, length);
    }

    [Fact]
    public void ScanQuotedIdentifier_Unterminated_ScansToEnd()
    {
        // Arrange
        var cursor = new TextCursor("\"unterminated");

        // Act
        var (type, length) = SqlScanners.ScanQuotedIdentifier(cursor);

        // Assert
        Assert.Equal(TokenKind.QuotedIdentifier, type);
        Assert.True(cursor.IsAtEnd());
    }

    [Fact]
    public void ScanQuotedIdentifier_WithSpecialChars_ScansCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("\"table-name!@#\" FROM");

        // Act
        var (type, length) = SqlScanners.ScanQuotedIdentifier(cursor);

        // Assert
        Assert.Equal(TokenKind.QuotedIdentifier, type);
        Assert.Equal(15, length);
    }

    [Fact]
    public void AllScanners_ReturnCorrectTokenKind()
    {
        // Arrange & Act & Assert
        var lineCommentResult = SqlScanners.ScanLineComment(new TextCursor("-- test\n"));
        Assert.Equal(TokenKind.LineComment, lineCommentResult.Type);

        var blockCommentResult = SqlScanners.ScanBlockComment(new TextCursor("/* test */"));
        Assert.Equal(TokenKind.BlockComment, blockCommentResult.Type);

        var numberResult = SqlScanners.ScanNumber(new TextCursor("123"));
        Assert.Equal(TokenKind.NumericLiteral, numberResult.Type);

        var operatorResult = SqlScanners.ScanOperator(new TextCursor("="));
        Assert.Equal(TokenKind.Operator, operatorResult.Type);

        var stringResult = SqlScanners.ScanStringLiteral(new TextCursor("'test'"));
        Assert.Equal(TokenKind.StringLiteral, stringResult.Type);

        var dollarResult = SqlScanners.ScanDollarQuotedString(new TextCursor("$$test$$"));
        Assert.Equal(TokenKind.DollarQuotedString, dollarResult.Type);

        var quotedIdResult = SqlScanners.ScanQuotedIdentifier(new TextCursor("\"test\""));
        Assert.Equal(TokenKind.QuotedIdentifier, quotedIdResult.Type);
    }
}
