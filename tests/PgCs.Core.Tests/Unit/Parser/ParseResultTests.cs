using PgCs.Core.Parser;
using PgCs.Core.Parser.Metadata;
using PgCs.Core.Types.Schema;

namespace PgCs.Core.Tests.Unit.Parser;

/// <summary>
/// Comprehensive tests for ParseResult struct
/// Tests all success/failure scenarios and properties
/// </summary>
public sealed class ParseResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Arrange
        var table = new PgTable
        {
            Name = "users",
            Columns = new List<PgColumn>().AsReadOnly()
        };

        // Act
        var result = ParseResult<PgTable>.Success(table);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("users", result.Value.Name);
        Assert.Null(result.Error);
        Assert.Equal(0, result.ErrorLine);
        Assert.Equal(0, result.ErrorColumn);
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        // Act
        var result = ParseResult<PgTable>.Failure("Parse error");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("Parse error", result.Error);
        Assert.Equal(0, result.ErrorLine);
        Assert.Equal(0, result.ErrorColumn);
    }

    [Fact]
    public void Failure_WithLineAndColumn_StoresLocation()
    {
        // Act
        var result = ParseResult<PgTable>.Failure("Syntax error", line: 10, column: 25);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Syntax error", result.Error);
        Assert.Equal(10, result.ErrorLine);
        Assert.Equal(25, result.ErrorColumn);
    }

    [Fact]
    public void Success_WithNullValue_ThrowsOrHandlesGracefully()
    {
        // Act
        var result = ParseResult<PgTable>.Success(null!);

        // Assert
        Assert.True(result.IsSuccess);
        // Value can be null for reference types
    }

    [Fact]
    public void ParseResult_IsValueType_SupportsValueSemantics()
    {
        // Arrange
        var table = new PgTable
        {
            Name = "users",
            Columns = new List<PgColumn>().AsReadOnly()
        };

        // Act
        var result1 = ParseResult<PgTable>.Success(table);
        var result2 = ParseResult<PgTable>.Success(table);

        // Assert
        // Record structs support value equality
        Assert.Equal(result1.IsSuccess, result2.IsSuccess);
        Assert.Equal(result1.Value, result2.Value);
    }

    [Fact]
    public void ParseResult_SuccessAndFailure_AreDifferent()
    {
        // Arrange
        var table = new PgTable
        {
            Name = "users",
            Columns = new List<PgColumn>().AsReadOnly()
        };

        // Act
        var successResult = ParseResult<PgTable>.Success(table);
        var failureResult = ParseResult<PgTable>.Failure("Error");

        // Assert
        Assert.NotEqual(successResult.IsSuccess, failureResult.IsSuccess);
    }

    [Fact]
    public void ParseResult_CanBeUsedInConditional()
    {
        // Arrange
        var successResult = ParseResult<PgTable>.Success(new PgTable
        {
            Name = "users",
            Columns = new List<PgColumn>().AsReadOnly()
        });

        var failureResult = ParseResult<PgTable>.Failure("Error");

        // Act & Assert
        if (successResult.IsSuccess)
        {
            Assert.NotNull(successResult.Value);
        }
        else
        {
            Assert.Fail("Success result should be successful");
        }

        if (failureResult.IsSuccess)
        {
            Assert.Fail("Failure result should not be successful");
        }
        else
        {
            Assert.NotNull(failureResult.Error);
        }
    }

    [Fact]
    public void ParseResult_WithDifferentTypes_CanBeCreated()
    {
        // Arrange & Act
        var tableResult = ParseResult<PgTable>.Success(new PgTable
        {
            Name = "users",
            Columns = new List<PgColumn>().AsReadOnly()
        });

        var viewResult = ParseResult<PgView>.Success(new PgView
        {
            Name = "user_view",
            Query = "SELECT * FROM users"
        });

        // Assert
        Assert.True(tableResult.IsSuccess);
        Assert.True(viewResult.IsSuccess);
        Assert.IsType<PgTable>(tableResult.Value);
        Assert.IsType<PgView>(viewResult.Value);
    }

    [Fact]
    public void ParseResult_ErrorMessage_CanBeAnyString()
    {
        // Arrange
        var errorMessages = new[]
        {
            "Unexpected token",
            "Expected identifier",
            "Missing semicolon",
            "Invalid syntax",
            ""
        };

        // Act & Assert
        foreach (var error in errorMessages)
        {
            var result = ParseResult<PgTable>.Failure(error);
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Error);
        }
    }

    [Fact]
    public void ParseResult_ErrorLocation_CanBeZero()
    {
        // Act
        var result = ParseResult<PgTable>.Failure("Error", 0, 0);

        // Assert
        Assert.Equal(0, result.ErrorLine);
        Assert.Equal(0, result.ErrorColumn);
    }

    [Fact]
    public void ParseResult_ErrorLocation_CanBeArbitrary()
    {
        // Act
        var result = ParseResult<PgTable>.Failure("Error", line: 100, column: 50);

        // Assert
        Assert.Equal(100, result.ErrorLine);
        Assert.Equal(50, result.ErrorColumn);
    }

    [Fact]
    public void Success_DoesNotSetErrorProperties()
    {
        // Arrange
        var table = new PgTable
        {
            Name = "users",
            Columns = new List<PgColumn>().AsReadOnly()
        };

        // Act
        var result = ParseResult<PgTable>.Success(table);

        // Assert
        Assert.Null(result.Error);
        Assert.Equal(0, result.ErrorLine);
        Assert.Equal(0, result.ErrorColumn);
    }

    [Fact]
    public void Failure_DoesNotSetValue()
    {
        // Act
        var result = ParseResult<PgTable>.Failure("Error");

        // Assert
        Assert.Null(result.Value);
    }

    [Fact]
    public void ParseResult_CanBeStoredInCollection()
    {
        // Arrange
        var results = new List<ParseResult<PgTable>>
        {
            ParseResult<PgTable>.Success(new PgTable
            {
                Name = "users",
                Columns = new List<PgColumn>().AsReadOnly()
            }),
            ParseResult<PgTable>.Failure("Error 1"),
            ParseResult<PgTable>.Success(new PgTable
            {
                Name = "orders",
                Columns = new List<PgColumn>().AsReadOnly()
            }),
            ParseResult<PgTable>.Failure("Error 2")
        };

        // Act
        var successCount = results.Count(r => r.IsSuccess);
        var failureCount = results.Count(r => !r.IsSuccess);

        // Assert
        Assert.Equal(2, successCount);
        Assert.Equal(2, failureCount);
    }

    [Fact]
    public void ParseResult_WithComplexObject_PreservesAllProperties()
    {
        // Arrange
        var table = new PgTable
        {
            Name = "users",
            Schema = "public",
            Columns = new List<PgColumn>
            {
                new PgColumn
                {
                    Name = "id",
                    DataType = "integer",
                    IsPrimaryKey = true
                }
            }.AsReadOnly(),
            IsPartitioned = false,
            IsTemporary = false
        };

        // Act
        var result = ParseResult<PgTable>.Success(table);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("users", result.Value!.Name);
        Assert.Equal("public", result.Value.Schema);
        Assert.Single(result.Value.Columns);
        Assert.Equal("id", result.Value.Columns[0].Name);
    }
}
