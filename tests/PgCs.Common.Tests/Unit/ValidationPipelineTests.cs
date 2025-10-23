using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.Common.Tests.Unit;

/// <summary>
/// Тесты для ValidationPipeline - Fluent API для комплексной валидации
/// </summary>
public sealed class ValidationPipelineTests
{
    #region Basic Configuration Tests

    [Fact]
    public void Create_ReturnsNewInstance()
    {
        // Act
        var pipeline = ValidationPipeline.Create();

        // Assert
        Assert.NotNull(pipeline);
    }

    [Fact]
    public void Validate_WithoutData_ReturnsValid()
    {
        // Arrange
        var pipeline = ValidationPipeline.Create();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(0, result.WarningCount);
    }

    #endregion

    #region Queries Validation Tests

    [Fact]
    public void CheckQueries_DuplicateMethodNames_ReturnsError()
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
                Parameters = new List<QueryParameter>()
            },
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users WHERE id = $1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Code == "DUPLICATE_METHOD_NAMES");
        Assert.True(result.ErrorCount > 0);
    }

    [Fact]
    public void CheckQueries_EmptySqlQuery_ReturnsError()
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
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Code == "EMPTY_SQL");
        Assert.True(result.ErrorCount > 0);
    }

    [Fact]
    public void CheckQueries_ValidQueries_NoErrors()
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
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int" }
                }
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    #endregion

    #region Parameters Validation Tests

    [Fact]
    public void CheckParameters_TooManyParameters_ReturnsWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "ComplexQuery",
                SqlQuery = "SELECT * FROM users WHERE col1=$1 AND col2=$2 AND col3=$3 AND col4=$4 AND col5=$5 AND col6=$6 AND col7=$7 AND col8=$8 AND col9=$9 AND col10=$10 AND col11=$11",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.Many,
                Parameters = Enumerable.Range(1, 11)
                    .Select(i => new QueryParameter
                    {
                        Name = $"param{i}",
                        PostgresType = "text",
                        CSharpType = "string"
                    })
                    .ToList()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckParameters();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.True(result.IsValid); // Warning, not error
        Assert.Contains(result.Issues, i => i.Code == "TOO_MANY_PARAMETERS");
        Assert.Equal(0, result.ErrorCount);
        Assert.True(result.WarningCount > 0);
    }

    [Fact]
    public void CheckParameters_FewParameters_NoWarning()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "SimpleQuery",
                SqlQuery = "SELECT * FROM users WHERE id = $1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int" }
                }
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckParameters();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    #endregion

    #region Return Types Validation Tests

    [Fact]
    public void CheckReturnTypes_MissingReturnType_ReturnsWarning()
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

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckReturnTypes();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.True(result.IsValid); // Warning, not error
        Assert.Contains(result.Issues, i => i.Code == "NO_RETURN_TYPE");
        Assert.Equal(0, result.ErrorCount);
        Assert.True(result.WarningCount > 0);
    }

    [Fact]
    public void CheckReturnTypes_ExecCardinality_NoWarning()
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
                ReturnType = null,
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "name", PostgresType = "text", CSharpType = "string" }
                }
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckReturnTypes();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, i => i.Code == "NO_RETURN_TYPE");
    }

    #endregion

    #region ValidateAll Tests

    [Fact]
    public void ValidateAll_CombinesAllQueryValidations()
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
                ReturnType = null,
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .ValidateAll();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.ErrorCount > 0);
        
        // Should have issues from multiple validators
        Assert.Contains(result.Issues, i => i.Code == "EMPTY_SQL");
        Assert.Contains(result.Issues, i => i.Code == "NO_RETURN_TYPE");
    }

    #endregion

    #region Filtering Tests

    [Fact]
    public void WithMinimumSeverity_FiltersWarnings()
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

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckReturnTypes()
            .WithMinimumSeverity(ValidationSeverity.Error);

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.True(result.IsValid); // No errors, only warnings
        Assert.Empty(result.Issues); // Warnings filtered out
    }

    [Fact]
    public void WithMinimumSeverity_IncludesErrors()
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
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries()
            .WithMinimumSeverity(ValidationSeverity.Error);

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Issues); // Errors included
        Assert.Contains(result.Issues, i => i.Code == "EMPTY_SQL");
    }

    #endregion

    #region StopOnFirstError Tests

    [Fact]
    public void StopOnFirstError_StopsAfterFirstError()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "Query1",
                SqlQuery = "",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            },
            new()
            {
                MethodName = "Query1", // Duplicate
                SqlQuery = "SELECT 1",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries()
            .StopOnFirstError();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Issues); // Should stop after first error
    }

    #endregion

    #region OnIssue Tests

    [Fact]
    public void OnIssue_InvokesHandlerForEachIssue()
    {
        // Arrange
        var invokedIssues = new List<ValidationIssue>();
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "Query1",
                SqlQuery = "",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.One,
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries()
            .OnIssue(issue => invokedIssues.Add(issue));

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.NotEmpty(invokedIssues);
        Assert.Equal(result.Issues.Count, invokedIssues.Count);
    }

    #endregion

    #region ValidateOrThrow Tests

    [Fact]
    public void ValidateOrThrow_WithErrors_ThrowsException()
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
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries();

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => pipeline.ValidateOrThrow());
        Assert.NotEmpty(exception.Issues);
        Assert.Contains("EMPTY_SQL", exception.Message);
    }

    [Fact]
    public void ValidateOrThrow_NoErrors_ReturnsResult()
    {
        // Arrange
        var queries = new List<QueryMetadata>
        {
            new()
            {
                MethodName = "GetUser",
                SqlQuery = "SELECT * FROM users",
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.Exec,
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries();

        // Act
        var result = pipeline.ValidateOrThrow();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }

    #endregion

    #region ValidationResult Tests

    [Fact]
    public void ValidationResult_GetReport_ReturnsFormattedReport()
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
                Parameters = new List<QueryParameter>()
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries();

        // Act
        var result = pipeline.Validate();
        var report = result.GetReport();

        // Assert
        Assert.NotNull(report);
        Assert.Contains("Validation Report", report);
        Assert.Contains("Valid: False", report);
        Assert.Contains("Errors:", report);
        Assert.Contains("EMPTY_SQL", report);
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public void ChainingMultipleValidators_Works()
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
                    Columns = new List<ReturnColumn>()
                },
                Parameters = new List<QueryParameter>
                {
                    new() { Name = "id", PostgresType = "integer", CSharpType = "int" }
                }
            }
        };

        var pipeline = ValidationPipeline.Create()
            .ForQueries(queries)
            .CheckQueries()
            .CheckParameters()
            .CheckReturnTypes();

        // Act
        var result = pipeline.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion
}
