using PgCs.Core.Parser;
using PgCs.Core.Types.Schema;

namespace PgCs.Core.Tests.Unit.Parser;

/// <summary>
/// Tests for ParseResult focusing on usage patterns
/// </summary>
public sealed class ParseResultTests
{
    [Fact]
    public void ParseResult_SupportsSuccessPattern()
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
    }

    [Fact]
    public void ParseResult_SupportsFailurePattern()
    {
        // Act
        var result = ParseResult<PgTable>.Failure("Parse error", line: 10, column: 25);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Parse error", result.Error);
        Assert.Equal(10, result.ErrorLine);
        Assert.Equal(25, result.ErrorColumn);
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
}
