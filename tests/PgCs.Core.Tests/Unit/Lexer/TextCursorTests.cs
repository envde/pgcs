using PgCs.Core.Lexer;

namespace PgCs.Core.Tests.Unit.Lexer;

/// <summary>
/// Comprehensive tests for TextCursor class
/// Tests all navigation and text manipulation methods
/// </summary>
public sealed class TextCursorTests
{
    [Fact]
    public void Constructor_InitializesWithText()
    {
        // Arrange & Act
        var cursor = new TextCursor("test");

        // Assert
        Assert.Equal(0, cursor.Position);
        Assert.Equal(1, cursor.Line);
        Assert.Equal(1, cursor.Column);
        Assert.Equal('t', cursor.Current);
        Assert.False(cursor.IsAtEnd());
    }

    [Fact]
    public void Current_ReturnsCurrentCharacter()
    {
        // Arrange
        var cursor = new TextCursor("abc");

        // Act & Assert
        Assert.Equal('a', cursor.Current);
    }

    [Fact]
    public void Current_AtEnd_ReturnsNullCharacter()
    {
        // Arrange
        var cursor = new TextCursor("a");
        cursor.Advance();

        // Act
        var current = cursor.Current;

        // Assert
        Assert.Equal('\0', current);
    }

    [Fact]
    public void Advance_MovesToNextCharacter()
    {
        // Arrange
        var cursor = new TextCursor("abc");

        // Act
        var prevChar = cursor.Advance();

        // Assert
        Assert.Equal('a', prevChar);
        Assert.Equal(1, cursor.Position);
        Assert.Equal('b', cursor.Current);
    }

    [Fact]
    public void Advance_UpdatesLineOnNewline()
    {
        // Arrange
        var cursor = new TextCursor("a\nb");

        // Act
        cursor.Advance(); // 'a'
        cursor.Advance(); // '\n'

        // Assert
        Assert.Equal(2, cursor.Line);
        Assert.Equal(1, cursor.Column);
        Assert.Equal('b', cursor.Current);
    }

    [Fact]
    public void Advance_UpdatesColumnOnRegularCharacter()
    {
        // Arrange
        var cursor = new TextCursor("abc");

        // Act
        cursor.Advance();
        cursor.Advance();

        // Assert
        Assert.Equal(1, cursor.Line);
        Assert.Equal(3, cursor.Column);
    }

    [Fact]
    public void Advance_MultipleNewlines_UpdatesLineCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("a\n\n\nb");

        // Act
        cursor.Advance(); // 'a'
        cursor.Advance(); // '\n'
        cursor.Advance(); // '\n'
        cursor.Advance(); // '\n'

        // Assert
        Assert.Equal(4, cursor.Line);
        Assert.Equal(1, cursor.Column);
        Assert.Equal('b', cursor.Current);
    }

    [Fact]
    public void Peek_ReturnsNextCharacter()
    {
        // Arrange
        var cursor = new TextCursor("abc");

        // Act
        var next = cursor.Peek();

        // Assert
        Assert.Equal('b', next);
        Assert.Equal(0, cursor.Position); // Position unchanged
        Assert.Equal('a', cursor.Current);
    }

    [Fact]
    public void Peek_AtEnd_ReturnsNullCharacter()
    {
        // Arrange
        var cursor = new TextCursor("a");
        cursor.Advance();

        // Act
        var next = cursor.Peek();

        // Assert
        Assert.Equal('\0', next);
    }

    [Fact]
    public void Peek_AtSecondToLast_ReturnsNullCharacter()
    {
        // Arrange
        var cursor = new TextCursor("ab");

        // Act
        cursor.Advance();
        var next = cursor.Peek();

        // Assert
        Assert.Equal('\0', next);
    }

    [Fact]
    public void IsAtEnd_WithMoreText_ReturnsFalse()
    {
        // Arrange
        var cursor = new TextCursor("abc");

        // Act & Assert
        Assert.False(cursor.IsAtEnd());
    }

    [Fact]
    public void IsAtEnd_AtEnd_ReturnsTrue()
    {
        // Arrange
        var cursor = new TextCursor("a");
        cursor.Advance();

        // Act & Assert
        Assert.True(cursor.IsAtEnd());
    }

    [Fact]
    public void IsAtEnd_EmptyString_ReturnsTrue()
    {
        // Arrange
        var cursor = new TextCursor("");

        // Act & Assert
        Assert.True(cursor.IsAtEnd());
    }

    [Fact]
    public void MatchSequence_WithMatchingSequence_ReturnsTrue()
    {
        // Arrange
        var cursor = new TextCursor("hello world");

        // Act
        var matches = cursor.MatchSequence("hello");

        // Assert
        Assert.True(matches);
    }

    [Fact]
    public void MatchSequence_WithNonMatchingSequence_ReturnsFalse()
    {
        // Arrange
        var cursor = new TextCursor("hello world");

        // Act
        var matches = cursor.MatchSequence("goodbye");

        // Assert
        Assert.False(matches);
    }

    [Fact]
    public void MatchSequence_AtMiddleOfText_MatchesCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("hello world");
        cursor.Advance();
        cursor.Advance();
        cursor.Advance();
        cursor.Advance();
        cursor.Advance();
        cursor.Advance(); // Now at 'w'

        // Act
        var matches = cursor.MatchSequence("world");

        // Assert
        Assert.True(matches);
    }

    [Fact]
    public void MatchSequence_SequenceLongerThanRemaining_ReturnsFalse()
    {
        // Arrange
        var cursor = new TextCursor("hi");

        // Act
        var matches = cursor.MatchSequence("hello");

        // Assert
        Assert.False(matches);
    }

    [Fact]
    public void MatchSequence_EmptySequence_ReturnsTrue()
    {
        // Arrange
        var cursor = new TextCursor("test");

        // Act
        var matches = cursor.MatchSequence("");

        // Assert
        Assert.True(matches);
    }

    [Fact]
    public void GetTextSpan_ReturnsCorrectSpan()
    {
        // Arrange
        var cursor = new TextCursor("hello world");

        // Act
        var span = cursor.GetTextSpan(0, 5);

        // Assert
        Assert.Equal("hello", span.ToString());
    }

    [Fact]
    public void GetTextSpan_WithOffset_ReturnsCorrectSpan()
    {
        // Arrange
        var cursor = new TextCursor("hello world");

        // Act
        var span = cursor.GetTextSpan(6, 5);

        // Assert
        Assert.Equal("world", span.ToString());
    }

    [Fact]
    public void GetTextSpan_BeyondTextLength_TruncatesCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("hello");

        // Act
        var span = cursor.GetTextSpan(0, 100);

        // Assert
        Assert.Equal("hello", span.ToString());
        Assert.Equal(5, span.Length);
    }

    [Fact]
    public void GetTextSpan_StartBeyondLength_ReturnsEmptySpan()
    {
        // Arrange
        var cursor = new TextCursor("hello");

        // Act
        var span = cursor.GetTextSpan(10, 5);

        // Assert
        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void CreateSnapshot_SavesCurrentPosition()
    {
        // Arrange
        var cursor = new TextCursor("hello world");
        cursor.Advance();
        cursor.Advance();

        // Act
        var snapshot = cursor.CreateSnapshot();

        // Assert
        Assert.Equal(2, snapshot.Position);
        Assert.Equal(1, snapshot.Line);
        Assert.Equal(3, snapshot.Column);
    }

    [Fact]
    public void RestoreSnapshot_RestoresPosition()
    {
        // Arrange
        var cursor = new TextCursor("hello world");
        cursor.Advance();
        cursor.Advance();
        var snapshot = cursor.CreateSnapshot();

        cursor.Advance();
        cursor.Advance();
        cursor.Advance();

        // Act
        cursor.RestoreSnapshot(snapshot);

        // Assert
        Assert.Equal(2, cursor.Position);
        Assert.Equal(1, cursor.Line);
        Assert.Equal(3, cursor.Column);
        Assert.Equal('l', cursor.Current);
    }

    [Fact]
    public void RestoreSnapshot_AfterNewlines_RestoresLineAndColumn()
    {
        // Arrange
        var cursor = new TextCursor("line1\nline2\nline3");
        cursor.Advance(); // l
        cursor.Advance(); // i
        cursor.Advance(); // n
        cursor.Advance(); // e
        cursor.Advance(); // 1
        cursor.Advance(); // \n

        var snapshot = cursor.CreateSnapshot();

        cursor.Advance(); // l
        cursor.Advance(); // i
        cursor.Advance(); // n
        cursor.Advance(); // e

        // Act
        cursor.RestoreSnapshot(snapshot);

        // Assert
        Assert.Equal(6, cursor.Position);
        Assert.Equal(2, cursor.Line);
        Assert.Equal(1, cursor.Column);
        Assert.Equal('l', cursor.Current);
    }

    [Fact]
    public void Navigation_ThroughCompleteText_TracksPositionCorrectly()
    {
        // Arrange
        var cursor = new TextCursor("abc");

        // Act & Assert - Navigate through complete text
        Assert.Equal('a', cursor.Current);
        Assert.Equal(0, cursor.Position);

        cursor.Advance();
        Assert.Equal('b', cursor.Current);
        Assert.Equal(1, cursor.Position);

        cursor.Advance();
        Assert.Equal('c', cursor.Current);
        Assert.Equal(2, cursor.Position);

        cursor.Advance();
        Assert.True(cursor.IsAtEnd());
        Assert.Equal(3, cursor.Position);
    }

    [Fact]
    public void Cursor_WithComplexText_TracksLineAndColumnAccurately()
    {
        // Arrange
        var text = "line1\nline2\r\nline3";
        var cursor = new TextCursor(text);

        // Act & Assert
        // Line 1
        Assert.Equal(1, cursor.Line);
        Assert.Equal(1, cursor.Column);
        cursor.Advance(); // l
        cursor.Advance(); // i
        cursor.Advance(); // n
        cursor.Advance(); // e
        cursor.Advance(); // 1
        Assert.Equal(1, cursor.Line);
        Assert.Equal(6, cursor.Column);

        cursor.Advance(); // \n
        Assert.Equal(2, cursor.Line);
        Assert.Equal(1, cursor.Column);

        // Line 2
        cursor.Advance(); // l
        cursor.Advance(); // i
        cursor.Advance(); // n
        cursor.Advance(); // e
        cursor.Advance(); // 2
        Assert.Equal(2, cursor.Line);
        Assert.Equal(6, cursor.Column);

        cursor.Advance(); // \r
        Assert.Equal(2, cursor.Line);
        Assert.Equal(7, cursor.Column);

        cursor.Advance(); // \n
        Assert.Equal(3, cursor.Line);
        Assert.Equal(1, cursor.Column);
    }

    [Fact]
    public void Cursor_MultipleSnapshots_CanRestoreToAny()
    {
        // Arrange
        var cursor = new TextCursor("abcdefgh");

        cursor.Advance();
        cursor.Advance();
        var snapshot1 = cursor.CreateSnapshot(); // At position 2

        cursor.Advance();
        cursor.Advance();
        var snapshot2 = cursor.CreateSnapshot(); // At position 4

        cursor.Advance();
        cursor.Advance();

        // Act & Assert
        cursor.RestoreSnapshot(snapshot2);
        Assert.Equal(4, cursor.Position);
        Assert.Equal('e', cursor.Current);

        cursor.RestoreSnapshot(snapshot1);
        Assert.Equal(2, cursor.Position);
        Assert.Equal('c', cursor.Current);
    }

    [Fact]
    public void Cursor_WithTabCharacters_IncrementsColumnNormally()
    {
        // Arrange
        var cursor = new TextCursor("a\tb\tc");

        // Act & Assert
        Assert.Equal(1, cursor.Column);
        cursor.Advance(); // 'a'
        Assert.Equal(2, cursor.Column);
        cursor.Advance(); // '\t'
        Assert.Equal(3, cursor.Column);
        cursor.Advance(); // 'b'
        Assert.Equal(4, cursor.Column);
    }

    [Fact]
    public void Cursor_Position_AlwaysIncreases()
    {
        // Arrange
        var cursor = new TextCursor("test\nline\nthree");
        var positions = new List<int>();

        // Act
        while (!cursor.IsAtEnd())
        {
            positions.Add(cursor.Position);
            cursor.Advance();
        }

        // Assert
        for (int i = 1; i < positions.Count; i++)
        {
            Assert.True(positions[i] > positions[i - 1], "Position should always increase");
        }
    }
}
