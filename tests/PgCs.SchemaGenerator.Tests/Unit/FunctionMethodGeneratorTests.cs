namespace PgCs.SchemaGenerator.Tests.Unit;

using PgCs.SchemaGenerator.Generators;
using PgCs.Common.Services;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.SchemaGenerator.Tests.Helpers;

public class FunctionMethodGeneratorTests
{
    private readonly FunctionMethodGenerator _generator;
    private readonly SchemaGenerationOptions _options;

    public FunctionMethodGeneratorTests()
    {
        var typeMapper = new PostgreSqlTypeMapper();
        var nameConverter = new NameConverter();
        _generator = new FunctionMethodGenerator(typeMapper, nameConverter);
        _options = TestOptionsBuilder.CreateDefault();
    }

    [Fact]
    public void Generate_WithSingleFunction_ShouldReturnOneRepositoryClass()
    {
        // Arrange
        var functions = new List<FunctionDefinition>
        {
            new()
            {
                Name = "get_user_by_id",
                Schema = "public",
                Parameters = [],
                Language = "plpgsql",
                Body = "BEGIN RETURN NULL; END;",
                ReturnType = "void"
            }
        };

        // Act
        var result = _generator.Generate(functions, _options);

        // Assert
        Assert.Single(result);
        Assert.Equal("DatabaseFunctions", result.First().TypeName);
    }

    [Fact]
    public void Generate_WithFunction_ShouldGenerateMethodWithParameters()
    {
        // Arrange
        var functions = new List<FunctionDefinition>
        {
            new()
            {
                Name = "get_user_by_id",
                Schema = "public",
                Language = "plpgsql",
                Body = "BEGIN RETURN NULL; END;",
                ReturnType = "void",
                Parameters =
                [
                    new FunctionParameter
                    {
                        Name = "user_id",
                        DataType = "integer",
                        Mode = ParameterMode.In
                    }
                ]
            }
        };

        // Act
        var result = _generator.Generate(functions, _options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("GetUserById", code);
        Assert.Contains("NpgsqlConnection connection", code);
        Assert.Contains("int userId", code);
    }

    [Fact]
    public void Generate_WithMultipleFunctions_ShouldGenerateMultipleMethods()
    {
        // Arrange
        var functions = new List<FunctionDefinition>
        {
            new()
            {
                Name = "get_user",
                Schema = "public",
                Language = "plpgsql",
                Body = "BEGIN RETURN NULL; END;",
                ReturnType = "void",
                Parameters = []
            },
            new()
            {
                Name = "create_user",
                Schema = "public",
                Language = "plpgsql",
                Body = "BEGIN RETURN NULL; END;",
                ReturnType = "void",
                Parameters = []
            }
        };

        // Act
        var result = _generator.Generate(functions, _options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("GetUser", code);
        Assert.Contains("CreateUser", code);
    }

    [Fact]
    public void Generate_ShouldIncludeNpgsqlUsing()
    {
        // Arrange
        var functions = new List<FunctionDefinition>
        {
            new()
            {
                Name = "test_function",
                Schema = "public",
                Language = "plpgsql",
                Body = "BEGIN RETURN NULL; END;",
                ReturnType = "void",
                Parameters = []
            }
        };

        // Act
        var result = _generator.Generate(functions, _options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("using Npgsql;", code);
        Assert.Contains("using System.Threading.Tasks;", code);
    }

    [Fact]
    public void Generate_ShouldSetCorrectCodeType()
    {
        // Arrange
        var functions = new List<FunctionDefinition>
        {
            new()
            {
                Name = "test_function",
                Schema = "public",
                Language = "plpgsql",
                Body = "BEGIN RETURN NULL; END;",
                ReturnType = "void",
                Parameters = []
            }
        };

        // Act
        var result = _generator.Generate(functions, _options);

        // Assert
        Assert.Equal(GeneratedFileType.RepositoryClass, result.First().CodeType);
    }

    [Fact]
    public void Generate_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var functions = new List<FunctionDefinition>();

        // Act
        var result = _generator.Generate(functions, _options);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Generate_ShouldGenerateAsyncMethod()
    {
        // Arrange
        var functions = new List<FunctionDefinition>
        {
            new()
            {
                Name = "test_function",
                Schema = "public",
                Language = "plpgsql",
                Body = "BEGIN RETURN NULL; END;",
                ReturnType = "void",
                Parameters = []
            }
        };

        // Act
        var result = _generator.Generate(functions, _options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("async", code);
        Assert.Contains("Task", code);
        Assert.Contains("await", code);
    }

    [Fact]
    public void Generate_ShouldGenerateSqlQuery()
    {
        // Arrange
        var functions = new List<FunctionDefinition>
        {
            new()
            {
                Name = "get_user",
                Schema = "public",
                Language = "plpgsql",
                Body = "BEGIN RETURN NULL; END;",
                ReturnType = "void",
                Parameters = []
            }
        };

        // Act
        var result = _generator.Generate(functions, _options);

        // Assert
        var code = result.First().SourceCode;
        Assert.Contains("SELECT * FROM public.get_user()", code);
    }
}
