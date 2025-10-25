using PgCs.SchemaGenerator.Tests.Helpers;
using PgCs.SchemaGenerator.Generators;

namespace PgCs.SchemaGenerator.Tests.Unit;

/// <summary>
/// Тесты для CustomTypeGenerator
/// </summary>
public sealed class CustomTypeGeneratorTests
{
    [Fact]
    public void Generate_WithEnumType_ShouldReturnEnum()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithEnumType("status", "active", "inactive", "pending");
        var options = TestOptionsBuilder.CreateDefault();
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(schema.Types, options);

        // Assert
        Assert.Single(result);
        var code = result.First().SourceCode;
        Assert.Contains("enum", code);
        Assert.Contains("Status", code); // Generator converts to PascalCase
    }

    [Fact]
    public void Generate_WithEnumType_ShouldConvertValuesToPascalCase()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithEnumType("user_role", "admin", "user", "guest");
        var options = TestOptionsBuilder.CreateDefault();
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(schema.Types, options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("Admin", code);
        Assert.Contains("User", code);
        Assert.Contains("Guest", code);
    }

    [Fact]
    public void Generate_WithSnakeCaseTypeName_ShouldConvertToPascalCase()
    {
        // Arrange
        var schema = TestSchemaMetadataBuilder.CreateWithEnumType("user_account_status");
        var options = TestOptionsBuilder.CreateDefault();
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(schema.Types, options);

        // Assert
        var code = result.First().SourceCode;
        // Generator converts snake_case to PascalCase
        Assert.Contains("UserAccountStatus", code);
    }

    [Fact]
    public void Generate_WithMultipleEnums_ShouldReturnMultipleResults()
    {
        // Arrange
        var schema1 = TestSchemaMetadataBuilder.CreateWithEnumType("status", "active", "inactive");
        var schema2 = TestSchemaMetadataBuilder.CreateWithEnumType("role", "admin", "user");
        
        var combinedTypes = schema1.Types.Concat(schema2.Types).ToArray();
        var combinedSchema = new SchemaMetadata
        {
            Types = combinedTypes,
            Tables = [],
            Views = [],
            Functions = [],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Comments = new Dictionary<string, string>(),
            AnalyzedAt = DateTime.UtcNow
        };
        
        var options = TestOptionsBuilder.CreateDefault();
        var generator = CreateGenerator();

        // Act
        var result = generator.Generate(combinedSchema.Types, options);

        // Assert
        Assert.Equal(2, result.Count);
    }

    private static CustomTypeGenerator CreateGenerator()
    {
        var typeMapper = new Common.Services.PostgreSqlTypeMapper();
        var nameConverter = new Common.Services.NameConverter();
        var syntaxBuilder = new Services.SyntaxBuilder(typeMapper, nameConverter);
        return new CustomTypeGenerator(syntaxBuilder);
    }
}
