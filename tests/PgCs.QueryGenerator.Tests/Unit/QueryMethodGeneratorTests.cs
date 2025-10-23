using Microsoft.CodeAnalysis.CSharp;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.Services;
using PgCs.QueryGenerator.Generators;
using PgCs.QueryGenerator.Services;

namespace PgCs.QueryGenerator.Tests.Unit;

/// <summary>
/// Тесты для QueryMethodGenerator - генератор C# методов для SQL запросов
/// </summary>
public sealed class QueryMethodGeneratorTests
{
    private readonly QueryMethodGenerator _generator;
    private readonly INameConverter _nameConverter;
    private readonly ITypeMapper _typeMapper;

    public QueryMethodGeneratorTests()
    {
        _typeMapper = new PostgreSqlTypeMapper();
        _nameConverter = new NameConverter();
        var syntaxBuilder = new QuerySyntaxBuilder(_typeMapper, _nameConverter);
        _generator = new QueryMethodGenerator(syntaxBuilder, _nameConverter);
    }

    #region Generate Tests

    [Fact]
    public void Generate_SelectQuery_ReturnsValidMethod()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("GetUser", result.MethodName);
        Assert.NotEmpty(result.SourceCode);
        Assert.Contains("public async", result.SourceCode);
        Assert.Contains("GetUserAsync", result.SourceCode);
    }

    [Fact]
    public void Generate_InsertQuery_ReturnsMethodReturningInt()
    {
        // Arrange
        var query = CreateInsertQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("InsertUser", result.MethodName);
        Assert.Contains("Task<int>", result.SourceCode);
        Assert.Contains("ExecuteNonQueryAsync", result.SourceCode);
    }

    [Fact]
    public void Generate_UpdateQuery_ReturnsMethodReturningInt()
    {
        // Arrange
        var query = CreateUpdateQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UpdateUser", result.MethodName);
        Assert.Contains("Task<int>", result.SourceCode);
        Assert.Contains("ExecuteNonQueryAsync", result.SourceCode);
    }

    [Fact]
    public void Generate_DeleteQuery_ReturnsMethodReturningInt()
    {
        // Arrange
        var query = CreateDeleteQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("DeleteUser", result.MethodName);
        Assert.Contains("Task<int>", result.SourceCode);
        Assert.Contains("DELETE FROM", result.SqlQuery);
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void Generate_SelectOne_ReturnsNullableModel()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("Task<User?>", result.SourceCode);
        Assert.Contains("return null", result.SourceCode);
    }

    [Fact]
    public void Generate_SelectMany_ReturnsList()
    {
        // Arrange
        var query = new QueryMetadata
        {
            MethodName = "GetUsers",
            SqlQuery = "SELECT id, name FROM users",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.Many, // Many instead of One
            ReturnType = new ReturnTypeInfo
            {
                ModelName = "User",
                Columns = new List<ReturnColumn>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false },
                    new() { Name = "name", PostgresType = "text", CSharpType = "string", IsNullable = false }
                }
            },
            Parameters = new List<QueryParameter>
            {
                new() { Name = "user_id", PostgresType = "integer", CSharpType = "int" }
            }
        };
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("Task<List<User>>", result.SourceCode);
        Assert.Contains("new List<User>", result.SourceCode);
        Assert.Contains("while", result.SourceCode);
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void Generate_WithParameters_AddsParametersToMethod()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("int userId", result.SourceCode);
        Assert.Contains("AddWithValue", result.SourceCode);
    }

    [Fact]
    public void Generate_MultipleParameters_AddsAllParameters()
    {
        // Arrange
        var query = new QueryMetadata
        {
            MethodName = "GetUser",
            SqlQuery = "SELECT id, name FROM users WHERE id = $1 AND email = $2",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.One,
            ReturnType = new ReturnTypeInfo
            {
                ModelName = "User",
                Columns = new List<ReturnColumn>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false },
                    new() { Name = "name", PostgresType = "text", CSharpType = "string", IsNullable = false }
                }
            },
            Parameters = new List<QueryParameter>
            {
                new() { Name = "user_id", PostgresType = "integer", CSharpType = "int" },
                new() { Name = "email", PostgresType = "text", CSharpType = "string" }
            }
        };
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("int userId", result.SourceCode);
        Assert.Contains("string email", result.SourceCode);
    }

    [Fact]
    public void Generate_NoParameters_NoParameterStatements()
    {
        // Arrange
        var query = new QueryMetadata
        {
            MethodName = "GetAllUsers",
            SqlQuery = "SELECT id, name FROM users",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.Many,
            ReturnType = new ReturnTypeInfo
            {
                ModelName = "User",
                Columns = new List<ReturnColumn>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false },
                    new() { Name = "name", PostgresType = "text", CSharpType = "string", IsNullable = false }
                }
            },
            Parameters = new List<QueryParameter>() // No parameters
        };
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.DoesNotContain("AddWithValue", result.SourceCode);
    }

    #endregion

    #region Options Tests

    [Fact]
    public void Generate_WithCancellationSupport_AddsCancellationToken()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder()
            .WithCancellation()
            .Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("CancellationToken cancellationToken", result.SourceCode);
    }

    [Fact]
    public void Generate_WithTransactionSupport_AddsTransaction()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder()
            .WithTransactionSupport()
            .Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("NpgsqlTransaction? transaction", result.SourceCode);
    }

    [Fact]
    public void Generate_WithSqlInDocumentation_IncludesSqlInComments()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder()
            .IncludeSqlInDocs()
            .Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("///", result.SourceCode);
        // XML comments присутствуют
    }

    #endregion

    #region Code Structure Tests

    [Fact]
    public void Generate_SelectQuery_ContainsConnectionManagement()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("await using", result.SourceCode);
        Assert.Contains("OpenConnectionAsync", result.SourceCode);
    }

    [Fact]
    public void Generate_SelectQuery_ContainsCommandCreation()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("NpgsqlCommand", result.SourceCode);
        Assert.Contains(query.SqlQuery, result.SourceCode);
    }

    [Fact]
    public void Generate_SelectQuery_ContainsReaderExecution()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("ExecuteReaderAsync", result.SourceCode);
        Assert.Contains("ReadAsync", result.SourceCode);
    }

    [Fact]
    public void Generate_SelectQuery_ValidCSharpSyntax()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert - проверяем что код парсится без ошибок
        var syntaxTree = CSharpSyntaxTree.ParseText(result.SourceCode);
        var diagnostics = syntaxTree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(diagnostics);
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void Generate_Query_ContainsMethodSignature()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.NotEmpty(result.MethodSignature);
        Assert.Contains("public async", result.MethodSignature);
        Assert.Contains("GetUserAsync", result.MethodSignature);
    }

    [Fact]
    public void Generate_Query_SignatureMatchesSourceCode()
    {
        // Arrange
        var query = CreateSelectQuery();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(query, options);

        // Assert
        Assert.Contains("Task<User?>", result.MethodSignature);
        Assert.Contains("int userId", result.MethodSignature);
    }

    #endregion

    #region Helper Methods

    private static QueryMetadata CreateSelectQuery()
    {
        return new QueryMetadata
        {
            MethodName = "GetUser",
            SqlQuery = "SELECT id, name FROM users WHERE id = $1",
            QueryType = QueryType.Select,
            ReturnCardinality = ReturnCardinality.One,
            ReturnType = new ReturnTypeInfo
            {
                ModelName = "User",
                Columns = new List<ReturnColumn>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false },
                    new() { Name = "name", PostgresType = "text", CSharpType = "string", IsNullable = false }
                }
            },
            Parameters = new List<QueryParameter>
            {
                new() { Name = "user_id", PostgresType = "integer", CSharpType = "int" }
            }
        };
    }

    private static QueryMetadata CreateInsertQuery()
    {
        return new QueryMetadata
        {
            MethodName = "InsertUser",
            SqlQuery = "INSERT INTO users (name, email) VALUES ($1, $2)",
            QueryType = QueryType.Insert,
            ReturnCardinality = ReturnCardinality.Exec,
            Parameters = new List<QueryParameter>
            {
                new() { Name = "name", PostgresType = "text", CSharpType = "string" },
                new() { Name = "email", PostgresType = "text", CSharpType = "string" }
            }
        };
    }

    private static QueryMetadata CreateUpdateQuery()
    {
        return new QueryMetadata
        {
            MethodName = "UpdateUser",
            SqlQuery = "UPDATE users SET name = $1 WHERE id = $2",
            QueryType = QueryType.Update,
            ReturnCardinality = ReturnCardinality.Exec,
            Parameters = new List<QueryParameter>
            {
                new() { Name = "name", PostgresType = "text", CSharpType = "string" },
                new() { Name = "user_id", PostgresType = "integer", CSharpType = "int" }
            }
        };
    }

    private static QueryMetadata CreateDeleteQuery()
    {
        return new QueryMetadata
        {
            MethodName = "DeleteUser",
            SqlQuery = "DELETE FROM users WHERE id = $1",
            QueryType = QueryType.Delete,
            ReturnCardinality = ReturnCardinality.Exec,
            Parameters = new List<QueryParameter>
            {
                new() { Name = "user_id", PostgresType = "integer", CSharpType = "int" }
            }
        };
    }

    #endregion
}
