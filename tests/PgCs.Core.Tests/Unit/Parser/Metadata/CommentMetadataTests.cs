using PgCs.Core.Parser.Metadata;

namespace PgCs.Core.Tests.Unit.Parser.Metadata;

/// <summary>
/// Comprehensive tests for CommentMetadata record
/// Tests all metadata properties and scenarios
/// </summary>
public sealed class CommentMetadataTests
{
    [Fact]
    public void CommentMetadata_WithComment_StoresCorrectly()
    {
        // Arrange & Act
        var metadata = new CommentMetadata
        {
            Comment = "User identifier"
        };

        // Assert
        Assert.Equal("User identifier", metadata.Comment);
        Assert.Null(metadata.ToName);
        Assert.Null(metadata.ToType);
        Assert.Null(metadata.CustomFields);
    }

    [Fact]
    public void CommentMetadata_WithToName_StoresCorrectly()
    {
        // Arrange & Act
        var metadata = new CommentMetadata
        {
            ToName = "UserId"
        };

        // Assert
        Assert.Equal("UserId", metadata.ToName);
        Assert.Null(metadata.Comment);
        Assert.Null(metadata.ToType);
    }

    [Fact]
    public void CommentMetadata_WithToType_StoresCorrectly()
    {
        // Arrange & Act
        var metadata = new CommentMetadata
        {
            ToType = "long"
        };

        // Assert
        Assert.Equal("long", metadata.ToType);
        Assert.Null(metadata.Comment);
        Assert.Null(metadata.ToName);
    }

    [Fact]
    public void CommentMetadata_WithAllStandardFields_StoresCorrectly()
    {
        // Arrange & Act
        var metadata = new CommentMetadata
        {
            Comment = "User identifier",
            ToName = "UserId",
            ToType = "long"
        };

        // Assert
        Assert.Equal("User identifier", metadata.Comment);
        Assert.Equal("UserId", metadata.ToName);
        Assert.Equal("long", metadata.ToType);
    }

    [Fact]
    public void CommentMetadata_WithCustomFields_StoresCorrectly()
    {
        // Arrange
        var customFields = new Dictionary<string, string>
        {
            ["custom1"] = "value1",
            ["custom2"] = "value2"
        };

        // Act
        var metadata = new CommentMetadata
        {
            CustomFields = customFields
        };

        // Assert
        Assert.NotNull(metadata.CustomFields);
        Assert.Equal(2, metadata.CustomFields.Count);
        Assert.Equal("value1", metadata.CustomFields["custom1"]);
        Assert.Equal("value2", metadata.CustomFields["custom2"]);
    }

    [Fact]
    public void CommentMetadata_Empty_ReturnsEmptyMetadata()
    {
        // Act
        var empty = CommentMetadata.Empty;

        // Assert
        Assert.NotNull(empty);
        Assert.True(empty.IsEmpty);
        Assert.Null(empty.Comment);
        Assert.Null(empty.ToName);
        Assert.Null(empty.ToType);
        Assert.Null(empty.CustomFields);
    }

    [Fact]
    public void CommentMetadata_IsEmpty_WithNoData_ReturnsTrue()
    {
        // Arrange
        var metadata = new CommentMetadata();

        // Act & Assert
        Assert.True(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_IsEmpty_WithComment_ReturnsFalse()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            Comment = "Some comment"
        };

        // Act & Assert
        Assert.False(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_IsEmpty_WithToName_ReturnsFalse()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            ToName = "PropertyName"
        };

        // Act & Assert
        Assert.False(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_IsEmpty_WithToType_ReturnsFalse()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            ToType = "string"
        };

        // Act & Assert
        Assert.False(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_IsEmpty_WithCustomFields_ReturnsFalse()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            CustomFields = new Dictionary<string, string> { ["key"] = "value" }
        };

        // Act & Assert
        Assert.False(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_IsEmpty_WithEmptyStrings_ReturnsTrue()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            Comment = "   ",
            ToName = "",
            ToType = null
        };

        // Act & Assert
        Assert.True(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_IsEmpty_WithEmptyCustomFields_ReturnsTrue()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            CustomFields = new Dictionary<string, string>()
        };

        // Act & Assert
        Assert.True(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_IsRecord_SupportsValueSemantics()
    {
        // Arrange
        var metadata1 = new CommentMetadata
        {
            Comment = "Test",
            ToName = "Name",
            ToType = "Type"
        };

        var metadata2 = new CommentMetadata
        {
            Comment = "Test",
            ToName = "Name",
            ToType = "Type"
        };

        // Act & Assert
        Assert.Equal(metadata1, metadata2);
    }

    [Fact]
    public void CommentMetadata_WithDifferentComment_AreNotEqual()
    {
        // Arrange
        var metadata1 = new CommentMetadata
        {
            Comment = "Comment 1"
        };

        var metadata2 = new CommentMetadata
        {
            Comment = "Comment 2"
        };

        // Act & Assert
        Assert.NotEqual(metadata1, metadata2);
    }

    [Fact]
    public void CommentMetadata_ComplexScenario_AllFields()
    {
        // Arrange
        var customFields = new Dictionary<string, string>
        {
            ["validation"] = "required",
            ["max_length"] = "255",
            ["format"] = "email"
        };

        // Act
        var metadata = new CommentMetadata
        {
            Comment = "User email address",
            ToName = "EmailAddress",
            ToType = "string",
            CustomFields = customFields
        };

        // Assert
        Assert.Equal("User email address", metadata.Comment);
        Assert.Equal("EmailAddress", metadata.ToName);
        Assert.Equal("string", metadata.ToType);
        Assert.NotNull(metadata.CustomFields);
        Assert.Equal(3, metadata.CustomFields.Count);
        Assert.Equal("required", metadata.CustomFields["validation"]);
        Assert.Equal("255", metadata.CustomFields["max_length"]);
        Assert.Equal("email", metadata.CustomFields["format"]);
        Assert.False(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_ToTypeCases_SupportsDifferentTypes()
    {
        // Arrange & Act & Assert
        var types = new[] { "string", "int", "long", "decimal", "DateTime", "bool", "Guid" };

        foreach (var type in types)
        {
            var metadata = new CommentMetadata { ToType = type };
            Assert.Equal(type, metadata.ToType);
            Assert.False(metadata.IsEmpty);
        }
    }

    [Fact]
    public void CommentMetadata_ToNameCases_SupportsDifferentNamingConventions()
    {
        // Arrange & Act & Assert
        var names = new[] { "UserId", "user_id", "UserID", "USER_ID", "userId" };

        foreach (var name in names)
        {
            var metadata = new CommentMetadata { ToName = name };
            Assert.Equal(name, metadata.ToName);
            Assert.False(metadata.IsEmpty);
        }
    }

    [Fact]
    public void CommentMetadata_CustomFields_CanHaveArbitraryKeys()
    {
        // Arrange
        var customFields = new Dictionary<string, string>
        {
            ["min"] = "0",
            ["max"] = "100",
            ["step"] = "5",
            ["unit"] = "px",
            ["@annotation"] = "special"
        };

        // Act
        var metadata = new CommentMetadata
        {
            CustomFields = customFields
        };

        // Assert
        Assert.Equal(5, metadata.CustomFields!.Count);
        Assert.Equal("special", metadata.CustomFields["@annotation"]);
    }

    [Fact]
    public void CommentMetadata_CustomFields_ReadOnlyDictionary()
    {
        // Arrange
        var customFields = new Dictionary<string, string>
        {
            ["key"] = "value"
        };

        // Act
        var metadata = new CommentMetadata
        {
            CustomFields = customFields
        };

        // Assert
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(metadata.CustomFields);
    }

    [Fact]
    public void CommentMetadata_EmptySingleton_IsSameInstance()
    {
        // Act
        var empty1 = CommentMetadata.Empty;
        var empty2 = CommentMetadata.Empty;

        // Assert
        Assert.Same(empty1, empty2);
    }

    [Fact]
    public void CommentMetadata_WithWhitespaceOnlyComment_IsEmpty()
    {
        // Arrange
        var metadata = new CommentMetadata
        {
            Comment = "   \t\n  "
        };

        // Act & Assert
        Assert.True(metadata.IsEmpty);
    }

    [Fact]
    public void CommentMetadata_PartialData_IsNotEmpty()
    {
        // Arrange & Act & Assert
        var metadata1 = new CommentMetadata { Comment = "C" };
        Assert.False(metadata1.IsEmpty);

        var metadata2 = new CommentMetadata { ToName = "N" };
        Assert.False(metadata2.IsEmpty);

        var metadata3 = new CommentMetadata { ToType = "T" };
        Assert.False(metadata3.IsEmpty);
    }
}
