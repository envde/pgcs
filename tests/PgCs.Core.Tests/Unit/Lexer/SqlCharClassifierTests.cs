using PgCs.Core.Lexer;

namespace PgCs.Core.Tests.Unit.Lexer;

/// <summary>
/// Comprehensive tests for SqlCharClassifier static class
/// Tests all character classification methods
/// </summary>
public sealed class SqlCharClassifierTests
{
    [Theory]
    [InlineData(' ', true)]
    [InlineData('\t', true)]
    [InlineData('\r', true)]
    [InlineData('\n', true)]
    [InlineData('a', false)]
    [InlineData('1', false)]
    [InlineData('_', false)]
    [InlineData('$', false)]
    [InlineData('\0', false)]
    public void IsWhitespace_WithVariousCharacters_ReturnsExpectedResult(char ch, bool expected)
    {
        // Act
        var result = SqlCharClassifier.IsWhitespace(ch);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData('a', true)]
    [InlineData('z', true)]
    [InlineData('A', true)]
    [InlineData('Z', true)]
    [InlineData('_', true)]
    [InlineData('α', true)] // Greek alpha
    [InlineData('ñ', true)] // Spanish n with tilde
    [InlineData('0', false)]
    [InlineData('9', false)]
    [InlineData('$', false)]
    [InlineData(' ', false)]
    [InlineData('-', false)]
    public void IsIdentifierStart_WithVariousCharacters_ReturnsExpectedResult(char ch, bool expected)
    {
        // Act
        var result = SqlCharClassifier.IsIdentifierStart(ch);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData('a', true)]
    [InlineData('Z', true)]
    [InlineData('_', true)]
    [InlineData('0', true)]
    [InlineData('9', true)]
    [InlineData('$', true)]
    [InlineData('α', true)]
    [InlineData(' ', false)]
    [InlineData('-', false)]
    [InlineData('!', false)]
    [InlineData('@', false)]
    public void IsIdentifierPart_WithVariousCharacters_ReturnsExpectedResult(char ch, bool expected)
    {
        // Act
        var result = SqlCharClassifier.IsIdentifierPart(ch);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData('0', true)]
    [InlineData('9', true)]
    [InlineData('5', true)]
    [InlineData('a', false)]
    [InlineData('Z', false)]
    [InlineData('_', false)]
    [InlineData(' ', false)]
    [InlineData('-', false)]
    public void IsDigit_WithVariousCharacters_ReturnsExpectedResult(char ch, bool expected)
    {
        // Act
        var result = SqlCharClassifier.IsDigit(ch);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData('+', true)]
    [InlineData('-', true)]
    [InlineData('*', true)]
    [InlineData('/', true)]
    [InlineData('%', true)]
    [InlineData('^', true)]
    [InlineData('<', true)]
    [InlineData('>', true)]
    [InlineData('=', true)]
    [InlineData('!', true)]
    [InlineData('|', true)]
    [InlineData('&', true)]
    [InlineData('~', true)]
    [InlineData('#', true)]
    [InlineData('a', false)]
    [InlineData('0', false)]
    [InlineData(' ', false)]
    [InlineData('_', false)]
    [InlineData('$', false)]
    [InlineData('.', false)]
    [InlineData(',', false)]
    public void IsOperatorChar_WithVariousCharacters_ReturnsExpectedResult(char ch, bool expected)
    {
        // Act
        var result = SqlCharClassifier.IsOperatorChar(ch);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsWhitespace_AllWhitespaceTypes_Recognized()
    {
        // Arrange
        var whitespaces = new[] { ' ', '\t', '\r', '\n' };

        // Act & Assert
        foreach (var ws in whitespaces)
        {
            Assert.True(SqlCharClassifier.IsWhitespace(ws), $"Character '{ws}' should be whitespace");
        }
    }

    [Fact]
    public void IsIdentifierStart_AllLetters_Recognized()
    {
        // Arrange - test a-z and A-Z
        var letters = Enumerable.Range('a', 26).Concat(Enumerable.Range('A', 26)).Select(i => (char)i);

        // Act & Assert
        foreach (var letter in letters)
        {
            Assert.True(SqlCharClassifier.IsIdentifierStart(letter), $"Letter '{letter}' should be identifier start");
        }
    }

    [Fact]
    public void IsIdentifierPart_AllDigits_Recognized()
    {
        // Arrange
        var digits = Enumerable.Range('0', 10).Select(i => (char)i);

        // Act & Assert
        foreach (var digit in digits)
        {
            Assert.True(SqlCharClassifier.IsIdentifierPart(digit), $"Digit '{digit}' should be identifier part");
        }
    }

    [Fact]
    public void IsDigit_AllDigits_Recognized()
    {
        // Arrange
        var digits = Enumerable.Range('0', 10).Select(i => (char)i);

        // Act & Assert
        foreach (var digit in digits)
        {
            Assert.True(SqlCharClassifier.IsDigit(digit), $"Character '{digit}' should be digit");
        }
    }

    [Fact]
    public void IsOperatorChar_AllOperatorCharacters_Recognized()
    {
        // Arrange
        var operators = new[] { '+', '-', '*', '/', '%', '^', '<', '>', '=', '!', '|', '&', '~', '#' };

        // Act & Assert
        foreach (var op in operators)
        {
            Assert.True(SqlCharClassifier.IsOperatorChar(op), $"Character '{op}' should be operator char");
        }
    }

    [Fact]
    public void IsIdentifierStart_Underscore_IsValid()
    {
        // Act & Assert
        Assert.True(SqlCharClassifier.IsIdentifierStart('_'));
    }

    [Fact]
    public void IsIdentifierPart_DollarSign_IsValid()
    {
        // Act & Assert
        Assert.True(SqlCharClassifier.IsIdentifierPart('$'));
    }

    [Fact]
    public void IsIdentifierStart_DollarSign_IsNotValid()
    {
        // Act & Assert
        Assert.False(SqlCharClassifier.IsIdentifierStart('$'));
    }

    [Fact]
    public void CharClassifier_PunctuationCharacters_NotRecognizedAsOperators()
    {
        // Arrange
        var punctuation = new[] { '.', ',', ';', '(', ')', '[', ']', '{', '}', '"', '\'' };

        // Act & Assert
        foreach (var p in punctuation)
        {
            Assert.False(SqlCharClassifier.IsOperatorChar(p), $"Punctuation '{p}' should not be operator char");
        }
    }

    [Fact]
    public void CharClassifier_UnicodeLetters_RecognizedAsIdentifierStart()
    {
        // Arrange - various unicode letters
        var unicodeLetters = new[] { 'ñ', 'é', 'ü', 'α', 'β', 'γ', 'Ω', 'ж', 'い' };

        // Act & Assert
        foreach (var letter in unicodeLetters)
        {
            Assert.True(SqlCharClassifier.IsIdentifierStart(letter), $"Unicode letter '{letter}' should be identifier start");
        }
    }

    [Fact]
    public void CharClassifier_NullCharacter_NotRecognizedAsAnything()
    {
        // Arrange
        var nullChar = '\0';

        // Act & Assert
        Assert.False(SqlCharClassifier.IsWhitespace(nullChar));
        Assert.False(SqlCharClassifier.IsIdentifierStart(nullChar));
        Assert.False(SqlCharClassifier.IsIdentifierPart(nullChar));
        Assert.False(SqlCharClassifier.IsDigit(nullChar));
        Assert.False(SqlCharClassifier.IsOperatorChar(nullChar));
    }

    [Fact]
    public void CharClassifier_ControlCharacters_NotRecognizedAsValidIdentifierParts()
    {
        // Arrange - some control characters
        var controlChars = new[] { '\x01', '\x02', '\x03', '\x1F' };

        // Act & Assert
        foreach (var cc in controlChars)
        {
            Assert.False(SqlCharClassifier.IsIdentifierStart(cc));
            Assert.False(SqlCharClassifier.IsIdentifierPart(cc));
        }
    }
}
