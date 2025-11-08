using PgCs.Core.Parser.Metadata;

namespace PgCs.Core.Tests.Unit.Parser.Metadata;

/// <summary>
/// Comprehensive tests for Comment record
/// Tests all properties and scenarios
/// </summary>
public sealed class CommentTests
{
    [Fact]
    public void Comment_WithText_CreatesCorrectly()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Text = "This is a comment"
        };

        // Assert
        Assert.Equal("This is a comment", comment.Text);
        Assert.Null(comment.Metadata);
        Assert.False(comment.IsHeader);
        Assert.True(comment.IsInline);
        Assert.Null(comment.LineNumber);
    }

    [Fact]
    public void Comment_WithMetadata_StoresMetadata()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            Comment = "User ID",
            ToName = "UserId",
            ToType = "long"
        };

        // Act
        var comment = new Comment
        {
            Text = "-- comment: User ID; to_name: UserId; to_type: long",
            Metadata = metadata
        };

        // Assert
        Assert.NotNull(comment.Metadata);
        Assert.Equal("User ID", comment.Metadata.Comment);
        Assert.Equal("UserId", comment.Metadata.ToName);
        Assert.Equal("long", comment.Metadata.ToType);
    }

    [Fact]
    public void Comment_HeaderComment_IsHeaderTrue()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Text = "Header comment",
            IsHeader = true
        };

        // Assert
        Assert.True(comment.IsHeader);
        Assert.False(comment.IsInline);
    }

    [Fact]
    public void Comment_InlineComment_IsInlineTrue()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Text = "Inline comment",
            IsHeader = false
        };

        // Assert
        Assert.False(comment.IsHeader);
        Assert.True(comment.IsInline);
    }

    [Fact]
    public void Comment_WithLineNumber_StoresLineNumber()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Text = "Comment on line 42",
            LineNumber = 42
        };

        // Assert
        Assert.Equal(42, comment.LineNumber);
    }

    [Fact]
    public void Comment_WithoutLineNumber_LineNumberIsNull()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Text = "Comment without line"
        };

        // Assert
        Assert.Null(comment.LineNumber);
    }

    [Fact]
    public void Comment_IsRecord_SupportsValueSemantics()
    {
        // Arrange
        var comment1 = new Comment
        {
            Text = "Same comment",
            IsHeader = true,
            LineNumber = 10
        };

        var comment2 = new Comment
        {
            Text = "Same comment",
            IsHeader = true,
            LineNumber = 10
        };

        // Act & Assert
        Assert.Equal(comment1, comment2);
    }

    [Fact]
    public void Comment_WithDifferentText_AreNotEqual()
    {
        // Arrange
        var comment1 = new Comment
        {
            Text = "Comment 1"
        };

        var comment2 = new Comment
        {
            Text = "Comment 2"
        };

        // Act & Assert
        Assert.NotEqual(comment1, comment2);
    }

    [Fact]
    public void Comment_EmptyText_IsValid()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Text = ""
        };

        // Assert
        Assert.Equal("", comment.Text);
    }

    [Fact]
    public void Comment_MultilineText_IsSupported()
    {
        // Arrange
        var multilineText = "Line 1\nLine 2\nLine 3";

        // Act
        var comment = new Comment
        {
            Text = multilineText
        };

        // Assert
        Assert.Equal(multilineText, comment.Text);
        Assert.Contains("\n", comment.Text);
    }

    [Fact]
    public void Comment_WithAllProperties_StoresCorrectly()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            Comment = "Full comment",
            ToName = "PropertyName",
            ToType = "string"
        };

        // Act
        var comment = new Comment
        {
            Text = "Complete comment with all properties",
            Metadata = metadata,
            IsHeader = true,
            LineNumber = 100
        };

        // Assert
        Assert.Equal("Complete comment with all properties", comment.Text);
        Assert.NotNull(comment.Metadata);
        Assert.True(comment.IsHeader);
        Assert.Equal(100, comment.LineNumber);
    }

    [Fact]
    public void Comment_DefaultIsHeader_IsFalse()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Text = "Default header test"
        };

        // Assert
        Assert.False(comment.IsHeader);
        Assert.True(comment.IsInline);
    }

    [Fact]
    public void Comment_WithSpecialCharacters_PreservesText()
    {
        // Arrange
        var specialText = "Comment with special chars: @#$%^&*()[]{}|\\<>?/";

        // Act
        var comment = new Comment
        {
            Text = specialText
        };

        // Assert
        Assert.Equal(specialText, comment.Text);
    }

    [Fact]
    public void Comment_WithUnicodeText_PreservesUnicode()
    {
        // Arrange
        var unicodeText = "Комментарий на русском языке 中文注释 日本語のコメント";

        // Act
        var comment = new Comment
        {
            Text = unicodeText
        };

        // Assert
        Assert.Equal(unicodeText, comment.Text);
    }
}
