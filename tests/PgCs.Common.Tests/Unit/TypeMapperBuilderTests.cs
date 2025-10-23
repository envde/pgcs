using PgCs.Common.Services;

namespace PgCs.Common.Tests.Unit;

/// <summary>
/// Тесты для TypeMapperBuilder - fluent builder для кастомизации маппинга типов
/// </summary>
public sealed class TypeMapperBuilderTests
{
    #region Basic Custom Mappings

    [Fact]
    public void Builder_MapType_OverridesStandardMapping()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .MapType("uuid", "System.Guid")
            .Build();

        // Act
        var result = mapper.MapType("uuid", false, false);

        // Assert
        Assert.Equal("System.Guid", result);
    }

    [Fact]
    public void Builder_MapTypes_AddsMultipleMappings()
    {
        // Arrange
        var mappings = new Dictionary<string, string>
        {
            ["uuid"] = "Guid",
            ["jsonb"] = "JsonDocument",
            ["inet"] = "IPAddress"
        };

        var mapper = TypeMapperBuilder.Create()
            .MapTypes(mappings)
            .Build();

        // Act
        var result1 = mapper.MapType("uuid", false, false);
        var result2 = mapper.MapType("jsonb", false, false);
        var result3 = mapper.MapType("inet", false, false);

        // Assert
        Assert.Equal("Guid", result1);
        Assert.Equal("JsonDocument", result2);
        Assert.Equal("IPAddress", result3);
    }

    #endregion

    #region Namespace Configuration

    [Fact]
    public void Builder_AddNamespace_ReturnsCustomNamespace()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .AddNamespace("uuid", "System")
            .Build();

        // Act
        var result = mapper.GetRequiredNamespace("uuid");

        // Assert
        Assert.Equal("System", result);
    }

    [Fact]
    public void Builder_AddNamespaces_AddsMultipleNamespaces()
    {
        // Arrange
        var namespaces = new Dictionary<string, string>
        {
            ["uuid"] = "System",
            ["inet"] = "System.Net",
            ["jsonb"] = "System.Text.Json"
        };

        var mapper = TypeMapperBuilder.Create()
            .AddNamespaces(namespaces)
            .Build();

        // Act
        var ns1 = mapper.GetRequiredNamespace("uuid");
        var ns2 = mapper.GetRequiredNamespace("inet");
        var ns3 = mapper.GetRequiredNamespace("jsonb");

        // Assert
        Assert.Equal("System", ns1);
        Assert.Equal("System.Net", ns2);
        Assert.Equal("System.Text.Json", ns3);
    }

    #endregion

    #region Default Type Configuration

    [Fact]
    public void Builder_WithDefaultTypeForUnknown_UsesCustomDefault()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .WithDefaultTypeForUnknown("dynamic")
            .Build();

        // Act
        var result = mapper.MapType("unknown_custom_type", false, false);

        // Assert
        Assert.Equal("dynamic", result);
    }

    [Fact]
    public void Builder_WithDefaultTypeForUnknown_String_WorksCorrectly()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .WithDefaultTypeForUnknown("string")
            .Build();

        // Act
        var result = mapper.MapType("custom_enum", false, false);

        // Assert
        Assert.Equal("string", result);
    }

    #endregion

    #region Standard Mappings Control

    [Fact]
    public void Builder_ClearStandardMappings_UsesOnlyCustom()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .ClearStandardMappings()
            .MapType("text", "CustomString")
            .WithDefaultTypeForUnknown("object")
            .Build();

        // Act
        var customResult = mapper.MapType("text", false, false);
        var unknownResult = mapper.MapType("integer", false, false);

        // Assert
        Assert.Equal("CustomString", customResult);
        Assert.Equal("object", unknownResult); // integer не в custom, fallback на default
    }

    [Fact]
    public void Builder_UseStandardMappings_IncludesBuiltInTypes()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .UseStandardMappings()
            .Build();

        // Act
        var result = mapper.MapType("integer", false, false);

        // Assert
        Assert.Equal("int", result);
    }

    #endregion

    #region JSON Presets

    [Fact]
    public void Builder_UseSystemTextJson_ConfiguresJsonTypes()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .UseSystemTextJson()
            .Build();

        // Act
        var jsonResult = mapper.MapType("json", false, false);
        var jsonbResult = mapper.MapType("jsonb", false, false);
        var jsonNamespace = mapper.GetRequiredNamespace("json");
        var jsonbNamespace = mapper.GetRequiredNamespace("jsonb");

        // Assert
        Assert.Equal("JsonDocument", jsonResult);
        Assert.Equal("JsonDocument", jsonbResult);
        Assert.Equal("System.Text.Json", jsonNamespace);
        Assert.Equal("System.Text.Json", jsonbNamespace);
    }

    [Fact]
    public void Builder_UseNewtonsoftJson_ConfiguresJObjectTypes()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .UseNewtonsoftJson()
            .Build();

        // Act
        var jsonResult = mapper.MapType("json", false, false);
        var jsonbResult = mapper.MapType("jsonb", false, false);
        var jsonNamespace = mapper.GetRequiredNamespace("json");

        // Assert
        Assert.Equal("JObject", jsonResult);
        Assert.Equal("JObject", jsonbResult);
        Assert.Equal("Newtonsoft.Json.Linq", jsonNamespace);
    }

    [Fact]
    public void Builder_UseStringForJson_ConfiguresStringTypes()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .UseStringForJson()
            .Build();

        // Act
        var jsonResult = mapper.MapType("json", false, false);
        var jsonbResult = mapper.MapType("jsonb", false, false);

        // Assert
        Assert.Equal("string", jsonResult);
        Assert.Equal("string", jsonbResult);
    }

    #endregion

    #region NodaTime Preset

    [Fact]
    public void Builder_UseNodaTime_ConfiguresDateTimeTypes()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .UseNodaTime()
            .Build();

        // Act
        var timestampResult = mapper.MapType("timestamp", false, false);
        var dateResult = mapper.MapType("date", false, false);
        var timeResult = mapper.MapType("time", false, false);
        var intervalResult = mapper.MapType("interval", false, false);
        var timestampNamespace = mapper.GetRequiredNamespace("timestamp");

        // Assert
        Assert.Equal("Instant", timestampResult);
        Assert.Equal("LocalDate", dateResult);
        Assert.Equal("LocalTime", timeResult);
        Assert.Equal("Period", intervalResult);
        Assert.Equal("NodaTime", timestampNamespace);
    }

    #endregion

    #region NetTopologySuite Preset

    [Fact]
    public void Builder_UseNetTopologySuite_ConfiguresGeometryTypes()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .UseNetTopologySuite()
            .Build();

        // Act
        var geometryResult = mapper.MapType("geometry", false, false);
        var geographyResult = mapper.MapType("geography", false, false);
        var pointResult = mapper.MapType("point", false, false);
        var polygonResult = mapper.MapType("polygon", false, false);
        var geometryNamespace = mapper.GetRequiredNamespace("geometry");

        // Assert
        Assert.Equal("Geometry", geometryResult);
        Assert.Equal("Geometry", geographyResult);
        Assert.Equal("Point", pointResult);
        Assert.Equal("Polygon", polygonResult);
        Assert.Equal("NetTopologySuite.Geometries", geometryNamespace);
    }

    #endregion

    #region Nullable and Array Handling

    [Fact]
    public void Builder_CustomMapping_HandlesNullableValueTypes()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .MapType("custom_int", "int")
            .Build();

        // Act
        var nonNullable = mapper.MapType("custom_int", false, false);
        var nullable = mapper.MapType("custom_int", true, false);

        // Assert
        Assert.Equal("int", nonNullable);
        Assert.Equal("int?", nullable);
    }

    [Fact]
    public void Builder_CustomMapping_HandlesArrays()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .MapType("custom_text", "string")
            .Build();

        // Act
        var scalar = mapper.MapType("custom_text", false, false);
        var array = mapper.MapType("custom_text", false, true);
        var nullableArray = mapper.MapType("custom_text", true, true);

        // Assert
        Assert.Equal("string", scalar);
        Assert.Equal("string[]", array);
        Assert.Equal("string[]?", nullableArray);
    }

    [Fact]
    public void Builder_CustomMapping_HandlesReferenceTypes()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .MapType("custom_json", "string")
            .Build();

        // Act
        var nonNullable = mapper.MapType("custom_json", false, false);
        var nullable = mapper.MapType("custom_json", true, false);

        // Assert
        Assert.Equal("string", nonNullable);
        Assert.Equal("string", nullable); // reference types don't get ?
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Builder_CombinedConfiguration_AppliesAllSettings()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .UseSystemTextJson()
            .MapType("uuid", "Guid")
            .AddNamespace("uuid", "System")
            .WithDefaultTypeForUnknown("dynamic")
            .Build();

        // Act
        var jsonResult = mapper.MapType("jsonb", false, false);
        var uuidResult = mapper.MapType("uuid", false, false);
        var unknownResult = mapper.MapType("unknown", false, false);
        var uuidNamespace = mapper.GetRequiredNamespace("uuid");

        // Assert
        Assert.Equal("JsonDocument", jsonResult);
        Assert.Equal("Guid", uuidResult);
        Assert.Equal("dynamic", unknownResult);
        Assert.Equal("System", uuidNamespace);
    }

    [Fact]
    public void Builder_ChainedPresets_LastWins()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .UseNewtonsoftJson() // json → JObject
            .UseSystemTextJson() // json → JsonDocument (overwrites)
            .Build();

        // Act
        var result = mapper.MapType("json", false, false);

        // Assert
        Assert.Equal("JsonDocument", result);
    }

    #endregion

    #region Type Parameter Cleaning

    [Fact]
    public void Builder_CustomMapping_CleansTypeParameters()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .MapType("varchar", "string")
            .Build();

        // Act
        var result1 = mapper.MapType("varchar(100)", false, false);
        var result2 = mapper.MapType("varchar(255)", false, false);

        // Assert
        Assert.Equal("string", result1);
        Assert.Equal("string", result2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Builder_MapType_NullPostgresType_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = TypeMapperBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.MapType(null!, "string"));
    }

    [Fact]
    public void Builder_MapType_EmptyPostgresType_ThrowsArgumentException()
    {
        // Arrange
        var builder = TypeMapperBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.MapType("", "string"));
    }

    [Fact]
    public void Builder_MapType_NullCSharpType_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = TypeMapperBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.MapType("uuid", null!));
    }

    [Fact]
    public void Builder_MapTypes_NullDictionary_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = TypeMapperBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.MapTypes(null!));
    }

    [Fact]
    public void Builder_AddNamespace_NullPostgresType_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = TypeMapperBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddNamespace(null!, "System"));
    }

    [Fact]
    public void Builder_AddNamespace_NullNamespace_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = TypeMapperBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddNamespace("uuid", null!));
    }

    [Fact]
    public void Builder_WithDefaultTypeForUnknown_NullType_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = TypeMapperBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithDefaultTypeForUnknown(null!));
    }

    [Fact]
    public void Builder_WithDefaultTypeForUnknown_EmptyType_ThrowsArgumentException()
    {
        // Arrange
        var builder = TypeMapperBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithDefaultTypeForUnknown(""));
    }

    #endregion

    #region Case Insensitivity

    [Fact]
    public void Builder_CustomMapping_CaseInsensitive()
    {
        // Arrange
        var mapper = TypeMapperBuilder.Create()
            .MapType("UUID", "Guid")
            .Build();

        // Act
        var result1 = mapper.MapType("uuid", false, false);
        var result2 = mapper.MapType("UUID", false, false);
        var result3 = mapper.MapType("Uuid", false, false);

        // Assert
        Assert.Equal("Guid", result1);
        Assert.Equal("Guid", result2);
        Assert.Equal("Guid", result3);
    }

    #endregion
}
