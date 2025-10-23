namespace PgCs.SchemaGenerator.Tests.Unit;

/// <summary>
/// Тесты для SchemaGenerationOptions
/// </summary>
public sealed class SchemaGenerationOptionsTests
{
    [Fact]
    public void CreateBuilder_ShouldReturnBuilder()
    {
        // Act
        var builder = SchemaGenerationOptions.CreateBuilder();

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void DefaultOptions_ShouldHaveExpectedDefaults()
    {
        // Arrange & Act
        var options = new SchemaGenerationOptions
        {
            RootNamespace = "Test",
            OutputDirectory = "/tmp"
        };

        // Assert
        Assert.False(options.UsePrimaryConstructors);
        Assert.True(options.GenerateMappingAttributes);
        Assert.True(options.GenerateValidationAttributes);
        Assert.True(options.GenerateFunctions);
        Assert.Equal(FileOrganization.ByType, options.FileOrganization);
    }

    [Fact]
    public void Options_WithCustomValues_ShouldSetCorrectly()
    {
        // Arrange & Act
        var options = new SchemaGenerationOptions
        {
            RootNamespace = "MyApp.Data",
            OutputDirectory = "/custom/path",
            UsePrimaryConstructors = true,
            GenerateMappingAttributes = false,
            GenerateValidationAttributes = false,
            GenerateFunctions = false,
            TablePrefix = "tbl_",
            TableSuffix = "_data",
            FileOrganization = FileOrganization.BySchema
        };

        // Assert
        Assert.Equal("MyApp.Data", options.RootNamespace);
        Assert.Equal("/custom/path", options.OutputDirectory);
        Assert.True(options.UsePrimaryConstructors);
        Assert.False(options.GenerateMappingAttributes);
        Assert.False(options.GenerateValidationAttributes);
        Assert.False(options.GenerateFunctions);
        Assert.Equal("tbl_", options.TablePrefix);
        Assert.Equal("_data", options.TableSuffix);
        Assert.Equal(FileOrganization.BySchema, options.FileOrganization);
    }

    [Fact]
    public void Options_WithMappings_ShouldSetCorrectly()
    {
        // Arrange & Act
        var options = new SchemaGenerationOptions
        {
            RootNamespace = "Test",
            OutputDirectory = "/tmp",
            TableNameMappings = new Dictionary<string, string>
            {
                ["users"] = "User",
                ["products"] = "Product"
            },
            ColumnNameMappings = new Dictionary<string, string>
            {
                ["user_id"] = "UserId",
                ["product_name"] = "ProductName"
            },
            CustomTypeMappings = new Dictionary<string, string>
            {
                ["uuid"] = "Guid",
                ["jsonb"] = "JsonDocument"
            }
        };

        // Assert
        Assert.NotNull(options.TableNameMappings);
        Assert.Equal(2, options.TableNameMappings.Count);
        Assert.Equal("User", options.TableNameMappings["users"]);
        
        Assert.NotNull(options.ColumnNameMappings);
        Assert.Equal(2, options.ColumnNameMappings.Count);
        
        Assert.NotNull(options.CustomTypeMappings);
        Assert.Equal(2, options.CustomTypeMappings.Count);
    }

    [Fact]
    public void Options_WithTablePatterns_ShouldSetCorrectly()
    {
        // Arrange & Act
        var options = new SchemaGenerationOptions
        {
            RootNamespace = "Test",
            OutputDirectory = "/tmp",
            ExcludeTablePatterns = ["^temp_.*", ".*_backup$"],
            IncludeTablePatterns = ["^user_.*", "^product_.*"]
        };

        // Assert
        Assert.NotNull(options.ExcludeTablePatterns);
        Assert.Equal(2, options.ExcludeTablePatterns.Count);
        Assert.Contains("^temp_.*", options.ExcludeTablePatterns);
        
        Assert.NotNull(options.IncludeTablePatterns);
        Assert.Equal(2, options.IncludeTablePatterns.Count);
        Assert.Contains("^user_.*", options.IncludeTablePatterns);
    }

    [Fact]
    public void Options_AsRecord_ShouldSupportWith()
    {
        // Arrange
        var original = new SchemaGenerationOptions
        {
            RootNamespace = "Test",
            OutputDirectory = "/tmp",
            GenerateFunctions = false
        };

        // Act
        var modified = original with { GenerateFunctions = true };

        // Assert
        Assert.False(original.GenerateFunctions);
        Assert.True(modified.GenerateFunctions);
        Assert.Equal(original.RootNamespace, modified.RootNamespace);
    }
}
