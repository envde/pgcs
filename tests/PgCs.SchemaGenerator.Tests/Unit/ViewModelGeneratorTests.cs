namespace PgCs.SchemaGenerator.Tests.Unit;

using PgCs.SchemaGenerator.Generators;
using PgCs.SchemaGenerator.Services;
using PgCs.Common.Services;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.SchemaGenerator.Tests.Helpers;

public class ViewModelGeneratorTests
{
    private readonly ViewModelGenerator _generator;
    private readonly SchemaGenerationOptions _options;

    public ViewModelGeneratorTests()
    {
        var typeMapper = new PostgreSqlTypeMapper();
        var nameConverter = new NameConverter();
        var syntaxBuilder = new SyntaxBuilder(typeMapper, nameConverter);
        _generator = new ViewModelGenerator(syntaxBuilder);
        _options = TestOptionsBuilder.CreateDefault();
    }

    [Fact]
    public void Generate_WithSingleView_ShouldReturnOneModel()
    {
        // Arrange
        var views = new List<ViewDefinition>
        {
            new()
            {
                Name = "active_users",
                Schema = "public",
                Query = "SELECT * FROM users WHERE active = true",
                Columns =
                [
                    new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false },
                    new ColumnDefinition { Name = "name", DataType = "text", IsNullable = false }
                ]
            }
        };

        // Act
        var result = _generator.Generate(views, _options);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Generate_WithView_ShouldGenerateClassWithProperties()
    {
        // Arrange
        var views = new List<ViewDefinition>
        {
            new()
            {
                Name = "user_summary",
                Schema = "public",
                Query = "SELECT user_id, full_name FROM users",
                Columns =
                [
                    new ColumnDefinition { Name = "user_id", DataType = "integer", IsNullable = false },
                    new ColumnDefinition { Name = "full_name", DataType = "text", IsNullable = false }
                ]
            }
        };

        // Act
        var result = _generator.Generate(views, _options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("public sealed record class UserSummary", code);
        Assert.Contains("public required int UserId", code);
        Assert.Contains("public required string FullName", code);
    }

    [Fact]
    public void Generate_WithViewComment_ShouldIncludeXmlDocumentation()
    {
        // Arrange
        var views = new List<ViewDefinition>
        {
            new()
            {
                Name = "active_users",
                Schema = "public",
                Query = "SELECT * FROM users WHERE active = true",
                Comment = "View of all active users",
                Columns =
                [
                    new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }
                ]
            }
        };

        // Act
        var result = _generator.Generate(views, _options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("View of all active users", code);
    }

    [Fact]
    public void Generate_WithMultipleViews_ShouldReturnMultipleModels()
    {
        // Arrange
        var views = new List<ViewDefinition>
        {
            new()
            {
                Name = "active_users",
                Schema = "public",
                Query = "SELECT * FROM users WHERE active = true",
                Columns = [new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }]
            },
            new()
            {
                Name = "inactive_users",
                Schema = "public",
                Query = "SELECT * FROM users WHERE active = false",
                Columns = [new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }]
            }
        };

        // Act
        var result = _generator.Generate(views, _options);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.TypeName == "ActiveUser");
        Assert.Contains(result, r => r.TypeName == "InactiveUser");
    }

    [Fact]
    public void Generate_ShouldSetCorrectCodeType()
    {
        // Arrange
        var views = new List<ViewDefinition>
        {
            new()
            {
                Name = "test_view",
                Schema = "public",
                Query = "SELECT 1",
                Columns = [new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }]
            }
        };

        // Act
        var result = _generator.Generate(views, _options);

        // Assert
        Assert.Equal(GeneratedFileType.ViewModel, result.First().CodeType);
    }

    [Fact]
    public void Generate_ShouldSetCorrectNamespace()
    {
        // Arrange
        var views = new List<ViewDefinition>
        {
            new()
            {
                Name = "test_view",
                Schema = "public",
                Query = "SELECT 1",
                Columns = [new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }]
            }
        };

        // Act
        var result = _generator.Generate(views, _options);

        // Assert
        Assert.Equal(_options.RootNamespace, result.First().Namespace);
    }
}
