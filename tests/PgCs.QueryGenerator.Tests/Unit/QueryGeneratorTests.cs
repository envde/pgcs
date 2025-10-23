using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models.Options;

namespace PgCs.QueryGenerator.Tests.Unit;

/// <summary>
/// Тесты для QueryGenerator - главный класс генерации C# кода для SQL запросов
/// </summary>
public sealed class QueryGeneratorTests
{
    private readonly QueryGenerator _generator;

    public QueryGeneratorTests()
    {
        _generator = QueryGenerator.Create();
    }

    #region Generate Tests

    [Fact]
    public void Generate_EmptyQueryList_ReturnsResultWithWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>();
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(queries, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.ValidationIssues);
        Assert.Contains(result.ValidationIssues, i => i.Code == "EMPTY_QUERIES");
    }

    [Fact]
    public void Generate_InvalidQuery_ReturnsErrorsAndNoCode()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "",
                SqlQuery = "SELECT 1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            }
        };
        var options = QueryGenerationOptions.CreateBuilder().Build();

        // Act
        var result = _generator.Generate(queries, options);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationIssues, i => i.Severity == ValidationSeverity.Error);
        Assert.Empty(result.Methods);
    }

    #endregion

    #region ValidateQueries Tests

    [Fact]
    public void ValidateQueries_ValidQueries_ReturnsNoIssues()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            CreateValidSelectQuery("GetUser"),
            CreateValidInsertQuery("InsertUser")
        };

        // Act
        var issues = _generator.ValidateQueries(queries);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateQueries_InvalidQueries_ReturnsIssues()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "",
                SqlQuery = "SELECT 1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _generator.ValidateQueries(queries);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void ValidateQueries_DuplicateMethodNames_ReturnsError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            CreateValidSelectQuery("GetUser"),
            CreateValidSelectQuery("GetUser")
        };

        // Act
        var issues = _generator.ValidateQueries(queries);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Code == "DUPLICATE_METHOD_NAME");
    }

    [Fact]
    public void ValidateQueries_EmptyList_ReturnsWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>();

        // Act
        var issues = _generator.ValidateQueries(queries);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Code == "EMPTY_QUERIES");
        Assert.Contains(issues, i => i.Severity == ValidationSeverity.Warning);
    }

    #endregion

    #region Helper Methods

    private static QueryMetadata CreateValidSelectQuery(string methodName)
    {
        return new QueryMetadata
        {
            MethodName = methodName,
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
                new() { Name = "userId", PostgresType = "integer", CSharpType = "int" }
            }
        };
    }

    private static QueryMetadata CreateValidInsertQuery(string methodName)
    {
        return new QueryMetadata
        {
            MethodName = methodName,
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

    private static QueryMetadata CreateValidUpdateQuery(string methodName)
    {
        return new QueryMetadata
        {
            MethodName = methodName,
            SqlQuery = "UPDATE users SET name = $1 WHERE id = $2",
            QueryType = QueryType.Update,
            ReturnCardinality = ReturnCardinality.Exec,
            Parameters = new List<QueryParameter>
            {
                new() { Name = "name", PostgresType = "text", CSharpType = "string" },
                new() { Name = "userId", PostgresType = "integer", CSharpType = "int" }
            }
        };
    }

    private static QueryMetadata CreateValidDeleteQuery(string methodName)
    {
        return new QueryMetadata
        {
            MethodName = methodName,
            SqlQuery = "DELETE FROM users WHERE id = $1",
            QueryType = QueryType.Delete,
            ReturnCardinality = ReturnCardinality.Exec,
            Parameters = new List<QueryParameter>
            {
                new() { Name = "userId", PostgresType = "integer", CSharpType = "int" }
            }
        };
    }

    #endregion
}
