using Microsoft.CodeAnalysis.CSharp;
using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.Services;
using PgCs.QueryGenerator.Generators;
using PgCs.QueryGenerator.Services;

namespace PgCs.QueryGenerator.Tests.Unit;

/// <summary>
/// Тесты для QueryModelGenerator - генератор моделей для результатов и параметров
/// </summary>
public sealed class QueryModelGeneratorTests
{
    private readonly QueryModelGenerator _generator;
    private readonly ITypeMapper _typeMapper;

    public QueryModelGeneratorTests()
    {
        _typeMapper = new PostgreSqlTypeMapper();
        var nameConverter = new NameConverter();
        var syntaxBuilder = new QuerySyntaxBuilder(_typeMapper, nameConverter);
        _generator = new QueryModelGenerator(syntaxBuilder, _typeMapper);
    }

    #region GenerateResultModel Tests

    [Fact]
    public void GenerateResultModel_ValidQuery_ReturnsSuccess()
    {
        // Arrange
        var query = CreateQueryWithReturnType();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("User", result.ModelName);
        Assert.NotNull(result.Code);
    }

    [Fact]
    public void GenerateResultModel_NoReturnType_ReturnsFailure()
    {
        // Arrange
        var query = CreateQueryWithoutReturnType();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Empty(result.ModelName);
    }

    [Fact]
    public void GenerateResultModel_NoColumns_ReturnsFailure()
    {
        // Arrange
        var query = new QueryMetadata
        {
            MethodName = "GetUser",
            SqlQuery = "SELECT * FROM users",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.One,
            ReturnType = new ReturnTypeInfo
            {
                ModelName = "User",
                RequiresCustomModel = true,
                Columns = new List<ReturnColumn>() // Empty columns
            },
            Parameters = new List<QueryParameter>()
        };
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GenerateResultModel_ValidCSharpCode()
    {
        // Arrange
        var query = CreateQueryWithReturnType();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        var syntaxTree = CSharpSyntaxTree.ParseText(result.Code.SourceCode);
        var diagnostics = syntaxTree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void GenerateResultModel_ContainsProperties()
    {
        // Arrange
        var query = CreateQueryWithReturnType();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.Contains("public sealed record", result.Code.SourceCode);
        Assert.Contains("Id", result.Code.SourceCode);
        Assert.Contains("Name", result.Code.SourceCode);
    }

    [Fact]
    public void GenerateResultModel_UsesExplicitModelName()
    {
        // Arrange
        var query = new QueryMetadata
        {
            MethodName = "GetUser",
            SqlQuery = "SELECT id, name FROM users",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.One,
            ExplicitModelName = "CustomUserModel",
            ReturnType = new ReturnTypeInfo
            {
                ModelName = "User",
                RequiresCustomModel = true,
                Columns = new List<ReturnColumn>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false },
                    new() { Name = "name", PostgresType = "text", CSharpType = "string", IsNullable = false }
                }
            },
            Parameters = new List<QueryParameter>()
        };
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.Equal("CustomUserModel", result.ModelName);
    }

    [Fact]
    public void GenerateResultModel_GeneratesCorrectFileType()
    {
        // Arrange
        var query = CreateQueryWithReturnType();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.Equal(GeneratedFileType.ResultModel, result.Code.CodeType);
    }

    [Fact]
    public void GenerateResultModel_ReuseSchemaModels_ReturnsSuccessWithoutCode()
    {
        // Arrange
        var query = new QueryMetadata
        {
            MethodName = "GetUser",
            SqlQuery = "SELECT id, name FROM users",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.One,
            ReturnType = new ReturnTypeInfo
            {
                ModelName = "User",
                RequiresCustomModel = false, // Existing model
                Columns = new List<ReturnColumn>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false },
                    new() { Name = "name", PostgresType = "text", CSharpType = "string", IsNullable = false }
                }
            },
            Parameters = new List<QueryParameter>()
        };
        var options = QueryGenerationOptions.CreateBuilder()
            .ReuseSchemaModels()
            .Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("User", result.ModelName);
        Assert.Null(result.Code);
    }

    #endregion

    #region GenerateParameterModel Tests

    [Fact]
    public void GenerateParameterModel_AboveThreshold_ReturnsSuccess()
    {
        // Arrange
        var query = CreateQueryWithManyParameters(5);
        var options = QueryGenerationOptions.CreateBuilder()
            .WithParameterModels(3)
            .Build();

        // Act
        var result = _generator.GenerateParameterModel(query, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("GetUsersParameters", result.ModelName);
        Assert.NotNull(result.Code);
    }

    [Fact]
    public void GenerateParameterModel_BelowThreshold_ReturnsFailure()
    {
        // Arrange
        var query = CreateQueryWithManyParameters(2);
        var options = QueryGenerationOptions.CreateBuilder()
            .WithParameterModels(3)
            .Build();

        // Act
        var result = _generator.GenerateParameterModel(query, options);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GenerateParameterModel_Disabled_ReturnsFailure()
    {
        // Arrange
        var query = CreateQueryWithManyParameters(5);
        var options = QueryGenerationOptions.CreateBuilder()
            .WithoutParameterModels()
            .Build();

        // Act
        var result = _generator.GenerateParameterModel(query, options);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GenerateParameterModel_GeneratesCode()
    {
        // Arrange
        var query = CreateQueryWithManyParameters(5);
        var options = QueryGenerationOptions.CreateBuilder()
            .WithParameterModels(3)
            .Build();

        // Act
        var result = _generator.GenerateParameterModel(query, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Code);
        Assert.NotEmpty(result.Code.SourceCode);
        Assert.Contains("GetUsersParameters", result.Code.SourceCode);
    }

    [Fact]
    public void GenerateParameterModel_ContainsProperties()
    {
        // Arrange
        var query = CreateQueryWithManyParameters(3);
        var options = QueryGenerationOptions.CreateBuilder()
            .WithParameterModels(3)
            .Build();

        // Act
        var result = _generator.GenerateParameterModel(query, options);

        // Assert
        Assert.Contains("Param0", result.Code.SourceCode);
        Assert.Contains("Param1", result.Code.SourceCode);
        Assert.Contains("Param2", result.Code.SourceCode);
    }

    [Fact]
    public void GenerateParameterModel_GeneratesCorrectFileType()
    {
        // Arrange
        var query = CreateQueryWithManyParameters(5);
        var options = QueryGenerationOptions.CreateBuilder()
            .WithParameterModels(3)
            .Build();

        // Act
        var result = _generator.GenerateParameterModel(query, options);

        // Assert
        Assert.Equal(GeneratedFileType.ParameterModel, result.Code.CodeType);
    }

    #endregion

    #region Namespace and Type Tests

    [Fact]
    public void GenerateResultModel_UsesCorrectNamespace()
    {
        // Arrange
        var query = CreateQueryWithReturnType();
        var options = QueryGenerationOptions.CreateBuilder()
            .WithNamespace("MyApp.Data")
            .Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.Equal("MyApp.Data", result.Code.Namespace);
        Assert.Contains("namespace MyApp.Data", result.Code.SourceCode);
    }

    [Fact]
    public void GenerateParameterModel_UsesCorrectNamespace()
    {
        // Arrange
        var query = CreateQueryWithManyParameters(5);
        var options = QueryGenerationOptions.CreateBuilder()
            .WithNamespace("MyApp.Data")
            .WithParameterModels(3)
            .Build();

        // Act
        var result = _generator.GenerateParameterModel(query, options);

        // Assert
        Assert.Equal("MyApp.Data", result.Code.Namespace);
        Assert.Contains("MyApp.Data", result.Code.SourceCode);
    }

    [Fact]
    public void GenerateResultModel_ContainsRequiredUsings()
    {
        // Arrange
        var query = CreateQueryWithReturnType();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.GenerateResultModel(query, options);

        // Assert
        Assert.Contains("System", result.Code.SourceCode);
    }

    [Fact]
    public void GenerateParameterModel_ContainsRequiredUsings()
    {
        // Arrange
        var query = CreateQueryWithManyParameters(5);
        var options = QueryGenerationOptions.CreateBuilder()
            .WithParameterModels(3)
            .Build();

        // Act
        var result = _generator.GenerateParameterModel(query, options);

        // Assert
        Assert.Contains("System", result.Code.SourceCode);
    }

    #endregion

    #region Helper Methods

    private static QueryMetadata CreateQueryWithReturnType()
    {
        return new QueryMetadata
        {
            MethodName = "GetUser",
            SqlQuery = "SELECT id, name FROM users",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.One,
            ReturnType = new ReturnTypeInfo
            {
                ModelName = "User",
                RequiresCustomModel = true,
                Columns = new List<ReturnColumn>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false },
                    new() { Name = "name", PostgresType = "text", CSharpType = "string", IsNullable = false }
                }
            },
            Parameters = new List<QueryParameter>()
        };
    }

    private static QueryMetadata CreateQueryWithoutReturnType()
    {
        return new QueryMetadata
        {
            MethodName = "InsertUser",
            SqlQuery = "INSERT INTO users (name) VALUES ($1)",
            QueryType = QueryType.Insert,
            ReturnCardinality = ReturnCardinality.Exec,
            ReturnType = null,
            Parameters = new List<QueryParameter>
            {
                new() { Name = "name", PostgresType = "text", CSharpType = "string" }
            }
        };
    }

    private static QueryMetadata CreateQueryWithManyParameters(int count)
    {
        var parameters = new List<QueryParameter>();
        for (int i = 0; i < count; i++)
        {
            parameters.Add(new QueryParameter
            {
                Name = $"param{i}",
                PostgresType = "text",
                CSharpType = "string"
            });
        }

        return new QueryMetadata
        {
            MethodName = "GetUsers",
            SqlQuery = "SELECT * FROM users",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.Many,
            Parameters = parameters
        };
    }

    #endregion
}
