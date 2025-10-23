using PgCs.Common.Services;

namespace PgCs.Common.Tests.Unit;

/// <summary>
/// Тесты для NameConversionStrategyBuilder - fluent builder для кастомизации конвертации имен
/// </summary>
public sealed class NameConversionStrategyBuilderTests
{
    #region Case Style Configuration

    [Fact]
    public void Builder_UseCamelCaseForClasses_ReturnsCorrectCase()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .UseCamelCaseForClasses()
            .Build();

        // Act
        var result = converter.ToClassName("user_profile");

        // Assert
        Assert.Equal("userProfile", result);
    }

    [Fact]
    public void Builder_UseCamelCaseForProperties_ReturnsCorrectCase()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .UseCamelCaseForProperties()
            .Build();

        // Act
        var result = converter.ToPropertyName("user_name");

        // Assert
        Assert.Equal("userName", result);
    }

    [Fact]
    public void Builder_UsePascalCaseForParameters_ReturnsCorrectCase()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .UsePascalCaseForParameters()
            .Build();

        // Act
        var result = converter.ToParameterName("user_id");

        // Assert
        Assert.Equal("UserId", result);
    }

    #endregion

    #region Singularization / Pluralization

    [Fact]
    public void Builder_DoNotSingularizeClassNames_KeepsPlural()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .DoNotSingularizeClassNames()
            .Build();

        // Act
        var result = converter.ToClassName("users");

        // Assert
        Assert.Equal("Users", result);
    }

    [Fact]
    public void Builder_SingularizeClassNames_ConvertsToPascalAndSingular()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .SingularizeClassNames()
            .Build();

        // Act
        var result = converter.ToClassName("categories");

        // Assert
        Assert.Equal("Category", result);
    }

    #endregion

    #region Prefix / Suffix Removal

    [Fact]
    public void Builder_RemovePrefix_RemovesSpecifiedPrefix()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .RemovePrefix("tbl_", "v_")
            .Build();

        // Act
        var result1 = converter.ToClassName("tbl_users");
        var result2 = converter.ToClassName("v_active_users");

        // Assert
        Assert.Equal("User", result1);
        Assert.Equal("ActiveUser", result2);
    }

    [Fact]
    public void Builder_RemoveSuffix_RemovesSpecifiedSuffix()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .RemoveSuffix("_table", "_tmp")
            .Build();

        // Act
        var result1 = converter.ToClassName("users_table");
        var result2 = converter.ToClassName("orders_tmp");

        // Assert
        Assert.Equal("User", result1);
        Assert.Equal("Order", result2);
    }

    [Fact]
    public void Builder_RemovePrefixAndSuffix_RemovesBoth()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .RemovePrefix("tbl_")
            .RemoveSuffix("_temp")
            .Build();

        // Act
        var result = converter.ToClassName("tbl_users_temp");

        // Assert
        Assert.Equal("User", result);
    }

    #endregion

    #region Custom Rules

    [Fact]
    public void Builder_AddCustomRule_AppliesRegexReplacement()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .AddCustomRule(@"^usr_", "User")
            .Build();

        // Act
        var result = converter.ToClassName("usr_profile");

        // Assert
        Assert.Contains("User", result);
    }

    [Fact]
    public void Builder_AddTransformation_AppliesCustomFunction()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .AddTransformation(name => name.Replace("usr_", "user_"))
            .Build();

        // Act
        var result = converter.ToClassName("usr_profile");

        // Assert
        Assert.Equal("UserProfile", result);
    }

    [Fact]
    public void Builder_MultipleTransformations_AppliesInOrder()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .AddTransformation(name => name.Replace("usr_", "user_"))
            .AddTransformation(name => name.Replace("prof", "profile"))
            .Build();

        // Act
        var result = converter.ToClassName("usr_prof");

        // Assert
        Assert.Equal("UserProfile", result);
    }

    #endregion

    #region Quick Presets

    [Fact]
    public void Builder_UseStandardCSharpConventions_ConfiguresCorrectly()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .UseStandardCSharpConventions()
            .Build();

        // Act
        var className = converter.ToClassName("users");
        var propertyName = converter.ToPropertyName("user_name");
        var parameterName = converter.ToParameterName("user_id");

        // Assert
        Assert.Equal("User", className); // PascalCase + singular
        Assert.Equal("UserName", propertyName); // PascalCase
        Assert.Equal("userId", parameterName); // camelCase
    }

    [Fact]
    public void Builder_UseCamelCaseEverywhere_ConfiguresAllCamelCase()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .UseCamelCaseEverywhere()
            .Build();

        // Act
        var className = converter.ToClassName("user_profile");
        var propertyName = converter.ToPropertyName("user_name");
        var methodName = converter.ToMethodName("get_user");
        var parameterName = converter.ToParameterName("user_id");

        // Assert
        Assert.Equal("userProfile", className);
        Assert.Equal("userName", propertyName);
        Assert.Equal("getUser", methodName);
        Assert.Equal("userId", parameterName);
    }

    [Fact]
    public void Builder_UsePascalCaseEverywhere_ConfiguresAllPascalCase()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .UsePascalCaseEverywhere()
            .Build();

        // Act
        var className = converter.ToClassName("user_profile");
        var propertyName = converter.ToPropertyName("user_name");
        var methodName = converter.ToMethodName("get_user");
        var parameterName = converter.ToParameterName("user_id");

        // Assert
        Assert.Equal("UserProfile", className);
        Assert.Equal("UserName", propertyName);
        Assert.Equal("GetUser", methodName);
        Assert.Equal("UserId", parameterName);
    }

    [Fact]
    public void Builder_RemoveStandardTablePrefixes_RemovesCommonPrefixes()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .RemoveStandardTablePrefixes()
            .Build();

        // Act
        var result1 = converter.ToClassName("tbl_users");
        var result2 = converter.ToClassName("table_orders");
        var result3 = converter.ToClassName("t_products");

        // Assert
        Assert.Equal("User", result1);
        Assert.Equal("Order", result2);
        Assert.Equal("Product", result3);
    }

    [Fact]
    public void Builder_RemoveStandardViewPrefixes_RemovesCommonPrefixes()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .RemoveStandardViewPrefixes()
            .Build();

        // Act
        var result1 = converter.ToClassName("v_active_users");
        var result2 = converter.ToClassName("view_orders");
        var result3 = converter.ToClassName("vw_products");

        // Assert
        Assert.Equal("ActiveUser", result1);
        Assert.Equal("Order", result2);
        Assert.Equal("Product", result3);
    }

    [Fact]
    public void Builder_RemoveStandardProcedurePrefixes_RemovesCommonPrefixes()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .RemoveStandardProcedurePrefixes()
            .Build();

        // Act
        var result1 = converter.ToMethodName("sp_get_users");
        var result2 = converter.ToMethodName("proc_insert_order");
        var result3 = converter.ToMethodName("fn_calculate_total");
        var result4 = converter.ToMethodName("udf_format_date");

        // Assert
        Assert.Equal("GetUsers", result1);
        Assert.Equal("InsertOrder", result2);
        Assert.Equal("CalculateTotal", result3);
        Assert.Equal("FormatDate", result4);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Builder_CombinedConfiguration_AppliesAllRules()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .RemovePrefix("tbl_")
            .RemoveSuffix("_tmp")
            .AddCustomRule(@"_old$", "Legacy")
            .UsePascalCaseForClasses()
            .SingularizeClassNames()
            .Build();

        // Act
        var result = converter.ToClassName("tbl_users_tmp");

        // Assert
        Assert.Equal("User", result);
    }

    [Fact]
    public void Builder_ChainedConfiguration_ReturnsCorrectResults()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .UseStandardCSharpConventions()
            .RemoveStandardTablePrefixes()
            .RemoveStandardViewPrefixes()
            .Build();

        // Act
        var tableName = converter.ToClassName("tbl_users");
        var viewName = converter.ToClassName("v_active_users");
        var propertyName = converter.ToPropertyName("user_name");
        var paramName = converter.ToParameterName("user_id");

        // Assert
        Assert.Equal("User", tableName);
        Assert.Equal("ActiveUser", viewName);
        Assert.Equal("UserName", propertyName);
        Assert.Equal("userId", paramName);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Builder_EmptyPrefix_DoesNotThrow()
    {
        // Arrange & Act
        var converter = NameConversionStrategyBuilder.Create()
            .RemovePrefix("")
            .Build();

        var result = converter.ToClassName("users");

        // Assert
        Assert.Equal("User", result);
    }

    [Fact]
    public void Builder_NullTransformation_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = NameConversionStrategyBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddTransformation(null!));
    }

    [Fact]
    public void Builder_InvalidCulture_DoesNotThrowOnCreation()
    {
        // Arrange - CultureInfo конструктор не бросает исключение, создает "invalid-culture" как custom
        var builder = NameConversionStrategyBuilder.Create();

        // Act - построение успешно, культура будет использована как есть
        var converter = builder.WithHumanizerCulture("invalid-culture").Build();
        var result = converter.ToClassName("users");

        // Assert - метод работает, просто использует fallback для неизвестной культуры
        Assert.Equal("User", result);
    }

    [Fact]
    public void Builder_WithInvariantCulture_WorksCorrectly()
    {
        // Arrange
        var converter = NameConversionStrategyBuilder.Create()
            .WithInvariantCulture()
            .Build();

        // Act
        var result = converter.ToClassName("users");

        // Assert
        Assert.Equal("User", result);
    }

    #endregion

    #region Enum Member Names

    [Fact]
    public void Builder_EnumMemberName_AlwaysUsesPascalCase()
    {
        // Arrange - даже если настроен camelCase, enum members используют PascalCase
        var converter = NameConversionStrategyBuilder.Create()
            .UseCamelCaseEverywhere()
            .Build();

        // Act
        var result = converter.ToEnumMemberName("active_user");

        // Assert
        Assert.Equal("ActiveUser", result);
    }

    #endregion
}
