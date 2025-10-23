using PgCs.Common.CodeGeneration;
using PgCs.Common.Services;

namespace PgCs.Common.Tests.Unit;

/// <summary>
/// Тесты для GeneratorFactory - Fluent API factory для создания генераторов
/// </summary>
public sealed class GeneratorFactoryTests
{
    #region Basic Configuration Tests

    [Fact]
    public void Create_ReturnsNewInstance()
    {
        // Act
        var factory = GeneratorFactory.Create();

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void GetDependencies_WithDefaults_ReturnsAllDependencies()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        var (typeMapper, nameConverter, formatter) = factory.GetDependencies();

        // Assert
        Assert.NotNull(typeMapper);
        Assert.NotNull(nameConverter);
        Assert.NotNull(formatter);
        Assert.IsType<PostgreSqlTypeMapper>(typeMapper);
        Assert.IsType<NameConverter>(nameConverter);
        Assert.IsType<RoslynFormatter>(formatter);
    }

    [Fact]
    public void GetWriter_WithoutConfiguration_ReturnsNull()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        var writer = factory.GetWriter();

        // Assert
        Assert.Null(writer);
    }

    #endregion

    #region TypeMapper Configuration Tests

    [Fact]
    public void WithTypeMapper_CustomInstance_UsesCustomTypeMapper()
    {
        // Arrange
        var customMapper = new PostgreSqlTypeMapper();
        var factory = GeneratorFactory.Create();

        // Act
        factory.WithTypeMapper(customMapper);
        var (typeMapper, _, _) = factory.GetDependencies();

        // Assert
        Assert.Same(customMapper, typeMapper);
    }

    [Fact]
    public void WithTypeMapper_BuilderFunction_ConfiguresTypeMapper()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.WithTypeMapper(builder => builder
            .MapType("custom_type", "CustomType")
            .AddNamespace("custom_type", "Custom.Namespace"));
        var (typeMapper, _, _) = factory.GetDependencies();

        // Assert
        Assert.NotNull(typeMapper);
        var result = typeMapper.MapType("custom_type", false, false);
        Assert.Equal("CustomType", result);
    }

    [Fact]
    public void WithTypeMapper_NullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.WithTypeMapper((ITypeMapper)null!));
    }

    [Fact]
    public void WithTypeMapper_NullBuilderFunction_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            factory.WithTypeMapper((Func<TypeMapperBuilder, TypeMapperBuilder>)null!));
    }

    [Fact]
    public void WithDefaultTypeMapper_SetsPostgreSqlTypeMapper()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.WithDefaultTypeMapper();
        var (typeMapper, _, _) = factory.GetDependencies();

        // Assert
        Assert.IsType<PostgreSqlTypeMapper>(typeMapper);
    }

    #endregion

    #region NameConverter Configuration Tests

    [Fact]
    public void WithNameConverter_CustomInstance_UsesCustomNameConverter()
    {
        // Arrange
        var customConverter = new NameConverter();
        var factory = GeneratorFactory.Create();

        // Act
        factory.WithNameConverter(customConverter);
        var (_, nameConverter, _) = factory.GetDependencies();

        // Assert
        Assert.Same(customConverter, nameConverter);
    }

    [Fact]
    public void WithNameConverter_BuilderFunction_ConfiguresNameConverter()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.WithNameConverter(builder => builder
            .UsePascalCaseForClasses()
            .RemovePrefix("tbl_"));
        var (_, nameConverter, _) = factory.GetDependencies();

        // Assert
        Assert.NotNull(nameConverter);
        var result = nameConverter.ToClassName("tbl_users");
        Assert.Equal("User", result); // With prefix removed and PascalCase (Note: not pluralized to "Users")
    }

    [Fact]
    public void WithNameConverter_NullConverter_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.WithNameConverter((INameConverter)null!));
    }

    [Fact]
    public void WithNameConverter_NullBuilderFunction_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            factory.WithNameConverter((Func<NameConversionStrategyBuilder, NameConversionStrategyBuilder>)null!));
    }

    [Fact]
    public void WithDefaultNameConverter_SetsNameConverter()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.WithDefaultNameConverter();
        var (_, nameConverter, _) = factory.GetDependencies();

        // Assert
        Assert.IsType<NameConverter>(nameConverter);
    }

    #endregion

    #region Formatter Configuration Tests

    [Fact]
    public void WithFormatter_CustomInstance_UsesCustomFormatter()
    {
        // Arrange
        var customFormatter = new RoslynFormatter();
        var factory = GeneratorFactory.Create();

        // Act
        factory.WithFormatter(customFormatter);
        var (_, _, formatter) = factory.GetDependencies();

        // Assert
        Assert.Same(customFormatter, formatter);
    }

    [Fact]
    public void WithFormatter_NullFormatter_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.WithFormatter(null!));
    }

    [Fact]
    public void WithDefaultFormatter_SetsRoslynFormatter()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.WithDefaultFormatter();
        var (_, _, formatter) = factory.GetDependencies();

        // Assert
        Assert.IsType<RoslynFormatter>(formatter);
    }

    [Fact]
    public void WithoutFormatter_SetsNoOpFormatter()
    {
        // Arrange
        var factory = GeneratorFactory.Create();
        const string testCode = "class Test { }";

        // Act
        factory.WithoutFormatter();
        var (_, _, formatter) = factory.GetDependencies();
        var result = formatter.Format(testCode);

        // Assert
        Assert.NotNull(formatter);
        Assert.Equal(testCode, result); // No-op formatter returns unchanged code
    }

    #endregion

    #region Defaults Strategy Tests

    [Fact]
    public void WithoutDefaults_RequiresExplicitConfiguration()
    {
        // Arrange
        var factory = GeneratorFactory.Create().WithoutDefaults();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory.GetDependencies());
        Assert.Contains("TypeMapper not configured", exception.Message);
    }

    [Fact]
    public void WithoutDefaults_AfterConfiguringAll_Works()
    {
        // Arrange
        var factory = GeneratorFactory.Create()
            .WithoutDefaults()
            .WithDefaultTypeMapper()
            .WithDefaultNameConverter()
            .WithDefaultFormatter();

        // Act
        var (typeMapper, nameConverter, formatter) = factory.GetDependencies();

        // Assert
        Assert.NotNull(typeMapper);
        Assert.NotNull(nameConverter);
        Assert.NotNull(formatter);
    }

    [Fact]
    public void WithDefaults_AllowsPartialConfiguration()
    {
        // Arrange
        var customMapper = new PostgreSqlTypeMapper();
        var factory = GeneratorFactory.Create()
            .WithDefaults()
            .WithTypeMapper(customMapper);

        // Act
        var (typeMapper, nameConverter, formatter) = factory.GetDependencies();

        // Assert
        Assert.Same(customMapper, typeMapper);
        Assert.NotNull(nameConverter);
        Assert.NotNull(formatter);
    }

    #endregion

    #region Quick Presets Tests

    [Fact]
    public void UseSystemTextJsonPreset_ConfiguresAllDependencies()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.UseSystemTextJsonPreset();
        var (typeMapper, nameConverter, formatter) = factory.GetDependencies();

        // Assert
        Assert.NotNull(typeMapper);
        Assert.NotNull(nameConverter);
        Assert.NotNull(formatter);
        
        // Verify System.Text.Json mapping
        var jsonMapping = typeMapper.MapType("json", false, false);
        Assert.Contains("JsonDocument", jsonMapping);
    }

    [Fact]
    public void UseNodaTimePreset_ConfiguresNodaTimeMappings()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.UseNodaTimePreset();
        var (typeMapper, _, _) = factory.GetDependencies();

        // Assert
        var timestampMapping = typeMapper.MapType("timestamp", false, false);
        Assert.Equal("Instant", timestampMapping);
    }

    [Fact]
    public void UseNetTopologySuitePreset_ConfiguresPostGISMappings()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.UseNetTopologySuitePreset();
        var (typeMapper, _, _) = factory.GetDependencies();

        // Assert
        var geometryMapping = typeMapper.MapType("geometry", false, false);
        Assert.Equal("Geometry", geometryMapping);
    }

    [Fact]
    public void UseMinimalPreset_DisablesFormatting()
    {
        // Arrange
        var factory = GeneratorFactory.Create();
        const string testCode = "class Test { }";

        // Act
        factory.UseMinimalPreset();
        var (_, _, formatter) = factory.GetDependencies();
        var result = formatter.Format(testCode);

        // Assert
        Assert.Equal(testCode, result); // No formatting applied
    }

    [Fact]
    public void UseCustomizationPreset_RemovesPrefixesAndMapsUuid()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.UseCustomizationPreset();
        var (typeMapper, nameConverter, _) = factory.GetDependencies();

        // Assert
        // Check UUID mapping
        var uuidMapping = typeMapper.MapType("uuid", false, false);
        Assert.Equal("Guid", uuidMapping);
        
        // Check prefix removal
        var tableName = nameConverter.ToClassName("tbl_users");
        Assert.DoesNotContain("tbl_", tableName);
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public void ChainingMultipleConfigurations_Works()
    {
        // Arrange & Act
        var factory = GeneratorFactory.Create()
            .WithTypeMapper(builder => builder.UseSystemTextJson())
            .WithNameConverter(builder => builder.UseStandardCSharpConventions())
            .WithDefaultFormatter();

        var (typeMapper, nameConverter, formatter) = factory.GetDependencies();

        // Assert
        Assert.NotNull(typeMapper);
        Assert.NotNull(nameConverter);
        Assert.NotNull(formatter);
    }

    [Fact]
    public void MultiplePresets_LastOneWins()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        factory.UseSystemTextJsonPreset();
        factory.UseNodaTimePreset(); // This should override the previous preset

        var (typeMapper, _, _) = factory.GetDependencies();

        // Assert
        var timestampMapping = typeMapper.MapType("timestamp", false, false);
        Assert.Equal("Instant", timestampMapping); // NodaTime mapping
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void GetDependencies_CalledMultipleTimes_ReturnsSameInstances()
    {
        // Arrange
        var factory = GeneratorFactory.Create();

        // Act
        var deps1 = factory.GetDependencies();
        var deps2 = factory.GetDependencies();

        // Assert
        Assert.Same(deps1.TypeMapper, deps2.TypeMapper);
        Assert.Same(deps1.NameConverter, deps2.NameConverter);
        Assert.Same(deps1.Formatter, deps2.Formatter);
    }

    [Fact]
    public void WithoutDefaults_WithoutNameConverter_ThrowsSpecificError()
    {
        // Arrange
        var factory = GeneratorFactory.Create()
            .WithoutDefaults()
            .WithDefaultTypeMapper()
            .WithDefaultFormatter();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory.GetDependencies());
        Assert.Contains("NameConverter not configured", exception.Message);
    }

    [Fact]
    public void WithoutDefaults_WithoutFormatter_ThrowsSpecificError()
    {
        // Arrange
        var factory = GeneratorFactory.Create()
            .WithoutDefaults()
            .WithDefaultTypeMapper()
            .WithDefaultNameConverter();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory.GetDependencies());
        Assert.Contains("Formatter not configured", exception.Message);
    }

    #endregion
}
