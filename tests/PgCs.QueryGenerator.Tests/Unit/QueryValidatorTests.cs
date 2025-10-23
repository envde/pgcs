using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.QueryGenerator.Services;

namespace PgCs.QueryGenerator.Tests.Unit;

/// <summary>
/// Тесты для QueryValidator - валидация метаданных запросов перед генерацией
/// </summary>
public sealed class QueryValidatorTests
{
    private readonly QueryValidator _validator = new();

    #region Empty Queries

    [Fact]
    public void Validate_EmptyQueryList_ReturnsWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>();

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        var issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Warning, issue.Severity);
        Assert.Equal("EMPTY_QUERIES", issue.Code);
    }

    #endregion

    #region Method Name Validation

    [Fact]
    public void Validate_EmptyMethodName_ReturnsError()
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
        var issues = _validator.Validate(queries);

        // Assert
        var error = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, error.Severity);
        Assert.Equal("EMPTY_METHOD_NAME", error.Code);
    }

    [Fact]
    public void Validate_DuplicateMethodNames_ReturnsError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users WHERE id = $1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            },
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users WHERE email = $1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        var error = Assert.Single(issues, i => i.Code == "DUPLICATE_METHOD_NAME");
        Assert.Equal(ValidationSeverity.Error, error.Severity);
        Assert.Contains("GetUser", error.Message);
    }

    [Fact]
    public void Validate_CaseInsensitiveDuplicates_ReturnsError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT 1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            },
            new()
            {
                MethodName = "getuser",
                SqlQuery = "SELECT 2",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Contains(issues, i => i.Code == "DUPLICATE_METHOD_NAME");
    }

    #endregion

    #region SQL Query Validation

        [Fact]
    public void Validate_EmptySqlQuery_ReturnsError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                ReturnType = new ReturnTypeInfo
                {
                    ModelName = "User",
                    Columns = new List<ReturnColumn>
                    {
                        new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false }
                    }
                },
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Contains(issues, i => i.Code == "EMPTY_SQL");
    }

    [Fact]
    public void Validate_WhitespaceSqlQuery_ReturnsError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "   \n\t  ",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Contains(issues, i => i.Code == "EMPTY_SQL");
    }

    #endregion

    #region Parameter Validation

    [Fact]
    public void Validate_EmptyParameterName_ReturnsError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users WHERE id = $1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "", PostgresType = "integer", CSharpType = "int" }
                }
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Contains(issues, i => i.Code == "EMPTY_PARAMETER_NAME");
    }

    [Fact]
    public void Validate_DuplicateParameterNames_ReturnsWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users WHERE id = $1 OR id = $2",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int" },
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int" }
                }
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        var warning = Assert.Single(issues, i => i.Code == "DUPLICATE_PARAMETER");
        Assert.Equal(ValidationSeverity.Warning, warning.Severity);
    }

    [Fact]
    public void Validate_MissingParameterType_ReturnsWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users WHERE id = $1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "userId", PostgresType = "", CSharpType = "int" }
                }
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Contains(issues, i => i.Code == "MISSING_PARAMETER_TYPE");
    }

    #endregion

    #region Return Type Validation

    [Fact]
    public void Validate_SelectWithoutReturnType_ReturnsError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                ReturnType = null,
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        var error = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, error.Severity);
        Assert.Equal("MISSING_RETURN_TYPE", error.Code);
    }

    [Fact]
    public void Validate_SelectWithExecCardinality_NoError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "DoSomething",
                SqlQuery = "SELECT 1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.Exec,
                ReturnType = null,
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_SelectWithNoReturnColumns_ReturnsWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                ReturnType = new ReturnTypeInfo
                {
                    ModelName = "User",
                    Columns = new List<ReturnColumn>()
                },
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        var warning = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Warning, warning.Severity);
        Assert.Equal("NO_RETURN_COLUMNS", warning.Code);
    }

    #endregion

    #region Column Validation

    [Fact]
    public void Validate_EmptyColumnName_ReturnsError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT id FROM users",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                ReturnType = new ReturnTypeInfo
                {
                    ModelName = "User",
                    Columns = new List<ReturnColumn>
                    {
                        new() { Name = "", PostgresType = "integer", CSharpType = "int", IsNullable = false }
                    }
                },
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Contains(issues, i => i.Code == "EMPTY_COLUMN_NAME");
    }

    [Fact]
    public void Validate_DuplicateColumnNames_ReturnsWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT id, id FROM users",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                ReturnType = new ReturnTypeInfo
                {
                    ModelName = "User",
                    Columns = new List<ReturnColumn>
                    {
                        new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false },
                        new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false }
                    }
                },
                Parameters = new List<QueryParameter>()
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Contains(issues, i => i.Code == "DUPLICATE_COLUMN");
    }

    #endregion

    #region Valid Queries

    [Fact]
    public void Validate_ValidSelectQuery_NoIssues()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
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
                    new() { Name = "userId", PostgresType = "integer", CSharpType = "int" }
                }
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ValidInsertQuery_NoIssues()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "InsertUser",
                SqlQuery = "INSERT INTO users (name) VALUES ($1)",
                QueryType = QueryType.Insert,
                ReturnCardinality = ReturnCardinality.Exec,
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "name", PostgresType = "text", CSharpType = "string" }
                }
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_MultipleValidQueries_NoIssues()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users WHERE id = $1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                ReturnType = new ReturnTypeInfo
                {
                    ModelName = "User",
                    Columns = new List<ReturnColumn>
                    {
                        new() { Name = "id", PostgresType = "integer", CSharpType = "int", IsNullable = false }
                    }
                },
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "userId", PostgresType = "integer", CSharpType = "int" }
                }
            },
            new()
            {
                MethodName = "DeleteUser",
                SqlQuery = "DELETE FROM users WHERE id = $1",
                QueryType = QueryType.Delete,
                ReturnCardinality = ReturnCardinality.Exec,
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "userId", PostgresType = "integer", CSharpType = "int" }
                }
            }
        };

        // Act
        var issues = _validator.Validate(queries);

        // Assert
        Assert.Empty(issues);
    }

    #endregion
}
