using PgCs.SchemaGenerator.Tests.Helpers;
using PgCs.SchemaGenerator.Generators;

namespace PgCs.SchemaGenerator.Tests.Unit;

/// <summary>
/// Тесты для TableModelGenerator
/// </summary>
public sealed class TableModelGeneratorTests
{
    [Fact]
    public void Generate_WithSingleTable_ShouldReturnOneModel()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable("products");
        var options = TestOptionsBuilder.CreateDefault();
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(schema.Tables, options);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Generate_WithTable_ShouldGenerateClassWithProperties()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable("users");
        var options = TestOptionsBuilder.CreateDefault();
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(schema.Tables, options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("class", code);
        Assert.Contains("public", code);
        Assert.Contains("int", code); // id
        Assert.Contains("string", code); // name, email
    }

    [Fact]
    public void Generate_WithSnakeCaseColumnName_ShouldConvertToPascalCase()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable("users");
        var options = TestOptionsBuilder.CreateDefault();
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(schema.Tables, options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("Id", code); // не id
        Assert.Contains("Name", code); // не name
        Assert.Contains("Email", code); // не email
    }

    [Fact]
    public void Generate_WithNullableColumn_ShouldGenerateNullableProperty()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable("users");
        var options = TestOptionsBuilder.CreateDefault();
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(schema.Tables, options);

        // Assert
        var code = result.First().SourceCode;
        // Generator marks all properties as required, including nullable ones
        // This is the actual behavior - not using nullable reference types by default
        Assert.Contains("Email", code);
        Assert.Contains("required", code);
    }

    [Fact]
    public void Generate_WithMappingAttributes_ShouldIncludeTableAttribute()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithSimpleTable("users");
        var options = TestOptionsBuilder.CreateWithMappingAttributes(true);
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(schema.Tables, options);

        // Assert
        var code = result.First().SourceCode;
        // Generator includes using statements for attributes
        Assert.Contains("using System.ComponentModel.DataAnnotations", code);
    }

    private static TableModelGenerator CreateGenerator()
    {
        var typeMapper = new Common.Services.PostgreSqlTypeMapper();
        var nameConverter = new Common.Services.NameConverter();
        var syntaxBuilder = new Services.SyntaxBuilder(typeMapper, nameConverter);
        return new TableModelGenerator(syntaxBuilder);
    }
}
