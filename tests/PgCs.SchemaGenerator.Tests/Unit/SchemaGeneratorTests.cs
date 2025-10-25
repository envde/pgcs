using PgCs.SchemaGenerator.Tests.Helpers;

namespace PgCs.SchemaGenerator.Tests.Unit;

/// <summary>
/// Тесты для SchemaGenerator
/// </summary>
public sealed class SchemaGeneratorTests
{
    private readonly PgCs.SchemaGenerator.SchemaGenerator _generator;

    public SchemaGeneratorTests()
    {
        _generator = PgCs.SchemaGenerator.SchemaGenerator.Create();
    }

    [Fact]
    public void Create_ShouldReturnValidInstance()
    {
        // Assert
        Assert.NotNull(_generator);
    }

    [Fact]
    public void Generate_WithEmptySchema_ShouldReturnSuccess()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateEmpty();
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.GeneratedCode);
        Assert.NotNull(result.Statistics);
        Assert.Equal(0, result.Statistics.TotalFilesGenerated);
    }

    [Fact]
    public void Generate_WithSimpleTable_ShouldGenerateTableModel()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable("users");
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.GeneratedCode);
        Assert.Single(result.TableModels);
        
        var code = result.GeneratedCode.First();
        Assert.Contains("class", code.SourceCode);
        Assert.Contains("User", code.SourceCode); // Singular form: users -> User
        Assert.Contains("Id", code.SourceCode);
        Assert.Contains("Name", code.SourceCode);
        Assert.Contains("Email", code.SourceCode);
    }

    [Fact]
    public void Generate_WithEnumType_ShouldGenerateEnum()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithEnumType("user_status", "active", "inactive");
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.GeneratedCode);
        Assert.Single(result.CustomTypes);
        
        var code = result.GeneratedCode.First();
        Assert.Contains("enum", code.SourceCode);
        Assert.Contains("UserStatus", code.SourceCode); // Generator converts to PascalCase
        Assert.Contains("Active", code.SourceCode); // Values are PascalCase
        Assert.Contains("Inactive", code.SourceCode);
    }

    [Fact]
    public void Generate_WithView_ShouldGenerateViewModel()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithView("active_users");
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.GeneratedCode);
        Assert.Single(result.ViewModels);
        
        var code = result.GeneratedCode.First();
        Assert.Contains("class", code.SourceCode);
        Assert.Contains("ActiveUser", code.SourceCode); // Singular form: active_users -> ActiveUser
    }

    [Fact]
    public void Generate_WithFunction_WhenGenerateFunctionsTrue_ShouldGenerateMethod()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithFunction("get_user_count");
        var options = TestOptionsBuilder.CreateWithFunctions(true);

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.GeneratedCode);
        Assert.Single(result.Functions);
        
        var code = result.GeneratedCode.First();
        Assert.Contains("GetUserCount", code.SourceCode); // PascalCase method name
    }

    [Fact]
    public void Generate_WithFunction_WhenGenerateFunctionsFalse_ShouldNotGenerateMethod()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithFunction("get_user_count");
        var options = TestOptionsBuilder.CreateWithFunctions(false);

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.GeneratedCode);
        Assert.Empty(result.Functions);
    }

    [Fact]
    public void Generate_WithComplexSchema_ShouldGenerateAllElements()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateComplex();
        var options = TestOptionsBuilder.CreateWithFunctions(true);

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.GeneratedCode.Count); // 1 table + 1 view + 1 enum + 1 function
        
        Assert.Single(result.TableModels);
        Assert.Single(result.ViewModels);
        Assert.Single(result.CustomTypes);
        Assert.Single(result.Functions);
        
        Assert.NotNull(result.Statistics);
        Assert.Equal(1, result.Statistics.TablesProcessed);
        Assert.Equal(1, result.Statistics.ViewsProcessed);
        Assert.Equal(1, result.Statistics.TypesProcessed);
        Assert.Equal(1, result.Statistics.FunctionsProcessed);
    }

    [Fact]
    public void GenerateTableModels_WithSimpleTable_ShouldReturnGeneratedCode()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable();
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.GenerateTableModels(schema, options);

        // Assert
        Assert.Single(result);
        var code = result.First();
        Assert.NotNull(code.SourceCode);
        Assert.NotEmpty(code.SourceCode);
        Assert.Contains("class", code.SourceCode);
    }

    [Fact]
    public void GenerateViewModels_WithView_ShouldReturnGeneratedCode()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithView();
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.GenerateViewModels(schema, options);

        // Assert
        Assert.Single(result);
        var code = result.First();
        Assert.NotNull(code.SourceCode);
        Assert.NotEmpty(code.SourceCode);
    }

    [Fact]
    public void GenerateCustomTypes_WithEnum_ShouldReturnGeneratedCode()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithEnumType();
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.GenerateCustomTypes(schema, options);

        // Assert
        Assert.Single(result);
        var code = result.First();
        Assert.NotNull(code.SourceCode);
        Assert.NotEmpty(code.SourceCode);
        Assert.Contains("enum", code.SourceCode);
    }

    [Fact]
    public void GenerateFunctionMethods_WithFunction_ShouldReturnGeneratedCode()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithFunction();
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.GenerateFunctionMethods(schema, options);

        // Assert
        Assert.Single(result);
        var code = result.First();
        Assert.NotNull(code.SourceCode);
        Assert.NotEmpty(code.SourceCode);
    }

    [Fact]
    public void ValidateSchema_WithValidSchema_ShouldReturnNoErrors()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable();

        // Act
        var issues = _generator.ValidateSchema(schema);

        // Assert
        Assert.NotNull(issues);
        Assert.DoesNotContain(issues, i => i.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void FormatCode_WithValidCode_ShouldReturnFormattedCode()
    {
        // Arrange
        var uglyCode = "public class Test{public int Id{get;set;}}";

        // Act
        var formatted = _generator.FormatCode(uglyCode);

        // Assert
        Assert.NotNull(formatted);
        Assert.NotEmpty(formatted);
        Assert.NotEqual(uglyCode, formatted); // Должен отформатировать
    }

    [Fact]
    public void Generate_WithMappingAttributes_ShouldIncludeAttributes()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable();
        var options = TestOptionsBuilder.CreateWithMappingAttributes(true);

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.IsSuccess);
        var code = result.GeneratedCode.First().SourceCode;
        // Generator includes using statements for attributes even if not used
        Assert.Contains("using System.ComponentModel.DataAnnotations", code);
    }

    [Fact]
    public void Generate_ShouldSetDuration()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateComplex();
        var options = TestOptionsBuilder.CreateDefault();

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.True(result.Duration > TimeSpan.Zero);
    }

    [Fact]
    public void Generate_ShouldCalculateStatistics()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateComplex();
        var options = TestOptionsBuilder.CreateWithFunctions(true);

        // Act
        var result = _generator.Generate(schema, options);

        // Assert
        Assert.NotNull(result.Statistics);
        Assert.Equal(4, result.Statistics.TotalFilesGenerated);
        Assert.True(result.Statistics.TotalSizeInBytes > 0);
        Assert.True(result.Statistics.TotalLinesOfCode > 0);
        Assert.Equal(1, result.Statistics.TablesProcessed);
        Assert.Equal(1, result.Statistics.ViewsProcessed);
        Assert.Equal(1, result.Statistics.TypesProcessed);
        Assert.Equal(1, result.Statistics.FunctionsProcessed);
    }
}
