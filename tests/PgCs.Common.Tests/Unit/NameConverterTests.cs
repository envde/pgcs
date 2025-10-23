using PgCs.Common.Services;

namespace PgCs.Common.Tests.Unit;

/// <summary>
/// Тесты для NameConverter - конвертер имен из PostgreSQL в C# конвенции
/// </summary>
public sealed class NameConverterTests
{
    private readonly NameConverter _converter = new();

    #region ToClassName Tests

    [Fact]
    public void ToClassName_WithSnakeCase_ReturnsSingularPascalCase()
    {
        // Arrange
        const string input = "user_profiles";

        // Act
        var result = _converter.ToClassName(input);

        // Assert
        Assert.Equal("UserProfile", result);
    }

    [Fact]
    public void ToClassName_WithPlural_ReturnsSingular()
    {
        // Arrange
        const string input = "users";

        // Act
        var result = _converter.ToClassName(input);

        // Assert
        Assert.Equal("User", result);
    }

    [Fact]
    public void ToClassName_WithUpperSnakeCase_ReturnsPascalCase()
    {
        // Arrange
        const string input = "ACTIVE_USERS";

        // Act
        var result = _converter.ToClassName(input);

        // Assert
        Assert.Equal("ActiveUser", result);
    }

    [Fact]
    public void ToClassName_WithSingleWord_ReturnsCapitalized()
    {
        // Arrange
        const string input = "posts";

        // Act
        var result = _converter.ToClassName(input);

        // Assert
        Assert.Equal("Post", result);
    }

    #endregion

    #region ToPropertyName Tests

    [Fact]
    public void ToPropertyName_WithSnakeCase_ReturnsPascalCase()
    {
        // Arrange
        const string input = "user_id";

        // Act
        var result = _converter.ToPropertyName(input);

        // Assert
        Assert.Equal("UserId", result);
    }

    [Fact]
    public void ToPropertyName_WithUpperSnakeCase_ReturnsPascalCase()
    {
        // Arrange
        const string input = "USER_NAME";

        // Act
        var result = _converter.ToPropertyName(input);

        // Assert
        Assert.Equal("UserName", result);
    }

    [Fact]
    public void ToPropertyName_WithSingleWord_ReturnsCapitalized()
    {
        // Arrange
        const string input = "email";

        // Act
        var result = _converter.ToPropertyName(input);

        // Assert
        Assert.Equal("Email", result);
    }

    [Fact]
    public void ToPropertyName_WithMultipleWords_ReturnsPascalCase()
    {
        // Arrange
        const string input = "created_at_timestamp";

        // Act
        var result = _converter.ToPropertyName(input);

        // Assert
        Assert.Equal("CreatedAtTimestamp", result);
    }

    #endregion

    #region ToEnumMemberName Tests

    [Fact]
    public void ToEnumMemberName_WithSnakeCase_ReturnsPascalCase()
    {
        // Arrange
        const string input = "active_user";

        // Act
        var result = _converter.ToEnumMemberName(input);

        // Assert
        Assert.Equal("ActiveUser", result);
    }

    [Fact]
    public void ToEnumMemberName_WithUpperSnakeCase_ReturnsPascalCase()
    {
        // Arrange
        const string input = "PENDING_APPROVAL";

        // Act
        var result = _converter.ToEnumMemberName(input);

        // Assert
        Assert.Equal("PendingApproval", result);
    }

    [Fact]
    public void ToEnumMemberName_WithKebabCase_ReturnsPascalCase()
    {
        // Arrange
        const string input = "in-progress";

        // Act
        var result = _converter.ToEnumMemberName(input);

        // Assert
        Assert.Equal("InProgress", result);
    }

    #endregion

    #region ToMethodName Tests

    [Fact]
    public void ToMethodName_WithSnakeCase_ReturnsPascalCase()
    {
        // Arrange
        const string input = "get_user_by_id";

        // Act
        var result = _converter.ToMethodName(input);

        // Assert
        Assert.Equal("GetUserById", result);
    }

    [Fact]
    public void ToMethodName_WithUnderscores_ReturnsPascalCase()
    {
        // Arrange
        const string input = "calculate_total_amount";

        // Act
        var result = _converter.ToMethodName(input);

        // Assert
        Assert.Equal("CalculateTotalAmount", result);
    }

    #endregion

    #region ToParameterName Tests

    [Fact]
    public void ToParameterName_WithSnakeCase_ReturnsCamelCase()
    {
        // Arrange
        const string input = "user_id";

        // Act
        var result = _converter.ToParameterName(input);

        // Assert
        Assert.Equal("userId", result);
    }

    [Fact]
    public void ToParameterName_WithSingleWord_ReturnsLowercase()
    {
        // Arrange
        const string input = "email";

        // Act
        var result = _converter.ToParameterName(input);

        // Assert
        Assert.Equal("email", result);
    }

    [Fact]
    public void ToParameterName_WithMultipleWords_ReturnsCamelCase()
    {
        // Arrange
        const string input = "first_name";

        // Act
        var result = _converter.ToParameterName(input);

        // Assert
        Assert.Equal("firstName", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ToClassName_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        const string input = "";

        // Act
        var result = _converter.ToClassName(input);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ToPropertyName_WithSpaces_ReturnsPascalCase()
    {
        // Arrange
        const string input = "user name";

        // Act
        var result = _converter.ToPropertyName(input);

        // Assert
        Assert.Equal("UserName", result);
    }

    [Fact]
    public void ToEnumMemberName_WithMixedDelimiters_ReturnsPascalCase()
    {
        // Arrange
        const string input = "active_user-status";

        // Act
        var result = _converter.ToEnumMemberName(input);

        // Assert
        Assert.Equal("ActiveUserStatus", result);
    }

    [Fact]
    public void ToClassName_WithNumbersInName_PreservesNumbers()
    {
        // Arrange
        const string input = "user_profile_v2";

        // Act
        var result = _converter.ToClassName(input);

        // Assert
        Assert.Contains("V2", result);
    }

    #endregion
}
