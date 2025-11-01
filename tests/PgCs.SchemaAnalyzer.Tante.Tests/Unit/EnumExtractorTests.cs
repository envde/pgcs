using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для EnumExtractor
/// </summary>
public sealed class EnumExtractorTests
{
    private readonly IExtractor<EnumTypeDefinition> _extractor = new EnumExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidEnumBlock_ReturnsTrue()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TYPE status AS ENUM ('active', 'inactive');");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExtract_WithNullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
    }

    #endregion

    #region Extract Simple Enum Tests

    [Fact]
    public void Extract_SimpleEnum_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'inactive', 'pending');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("status", result.Definition.Name);
        Assert.Null(result.Definition.Schema);
        Assert.Equal(3, result.Definition.Values.Count);
        Assert.Equal("active", result.Definition.Values[0]);
        Assert.Equal("inactive", result.Definition.Values[1]);
        Assert.Equal("pending", result.Definition.Values[2]);
    }

    [Fact]
    public void Extract_EnumWithSchema_ReturnsDefinitionWithSchema()
    {
        // Arrange
        var sql = "CREATE TYPE public.order_status AS ENUM ('new', 'processing', 'completed');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("order_status", result.Definition.Name);
        Assert.Equal("public", result.Definition.Schema);
        Assert.Equal(3, result.Definition.Values.Count);
    }

    [Fact]
    public void Extract_EnumWithMultilineFormat_ReturnsValidDefinition()
    {
        // Arrange
        var sql = @"CREATE TYPE user_status AS ENUM (
    'active',
    'inactive',
    'suspended',
    'deleted'
);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("user_status", result.Definition.Name);
        Assert.Equal(4, result.Definition.Values.Count);
        Assert.Equal("active", result.Definition.Values[0]);
        Assert.Equal("deleted", result.Definition.Values[3]);
    }

    #endregion

    #region Extract with Comments Tests

    [Fact]
    public void Extract_EnumWithHeaderComment_PreservesComment()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'inactive');";
        var blocks = new List<SqlBlock>
        {
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                HeaderComment = "User account status enumeration",
                StartLine = 1,
                EndLine = 1
            }
        };

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("User account status enumeration", result.Definition.SqlComment);
    }

    [Fact]
    public void Extract_EnumBlock_PreservesRawSql()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'inactive');";
        var rawSql = "-- Status type\nCREATE TYPE status AS ENUM ('active', 'inactive');";
        var blocks = new List<SqlBlock>
        {
            new SqlBlock
            {
                Content = sql,
                RawContent = rawSql,
                StartLine = 1,
                EndLine = 2
            }
        };

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(rawSql, result.Definition.RawSql);
    }

    #endregion

    #region Extract Special Cases Tests

    [Fact]
    public void Extract_EnumWithSpacesInValues_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM (  'active'  ,  'inactive'  ,  'pending'  );";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(3, result.Definition.Values.Count);
        Assert.Equal("active", result.Definition.Values[0]);
    }

    [Fact]
    public void Extract_EnumWithEmptyValues_ReturnsFailure()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ();";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
    }

    [Fact]
    public void Extract_EnumWithSingleValue_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Single(result.Definition.Values);
        Assert.Equal("active", result.Definition.Values[0]);
    }

    [Fact]
    public void Extract_EnumWithSpecialCharactersInValues_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('in-progress', 'not_started', 'done!');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(3, result.Definition.Values.Count);
        Assert.Equal("in-progress", result.Definition.Values[0]);
        Assert.Equal("not_started", result.Definition.Values[1]);
        Assert.Equal("done!", result.Definition.Values[2]);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_EnumWithUpperCaseKeywords_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE STATUS AS ENUM ('ACTIVE', 'INACTIVE');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("STATUS", result.Definition.Name);
        Assert.Equal(2, result.Definition.Values.Count);
    }

    [Fact]
    public void Extract_EnumWithMixedCaseKeywords_ExtractsCorrectly()
    {
        // Arrange
        var sql = "Create Type status As Enum ('active', 'inactive');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("status", result.Definition.Name);
    }

    #endregion

    #region ValidationIssues Tests

    [Fact]
    public void Extract_WithDuplicateValues_ReturnsWarning()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'inactive', 'active', 'pending');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warning
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var warning = result.ValidationIssues.First(i => i.Code == "ENUM_DUPLICATE_VALUES");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, warning.Severity);
        Assert.Contains("active", warning.Message);
    }

    [Fact]
    public void Extract_WithEmptyValues_ReturnsWarning()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', '', 'inactive');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warning
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var warning = result.ValidationIssues.First(i => i.Code == "ENUM_EMPTY_VALUE");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, warning.Severity);
    }

    [Fact]
    public void Extract_WithTooManyValues_ReturnsWarning()
    {
        // Arrange
        var values = string.Join(", ", Enumerable.Range(1, 150).Select(i => $"'value{i}'"));
        var sql = $"CREATE TYPE large_enum AS ENUM ({values});";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warning
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var warning = result.ValidationIssues.First(i => i.Code == "ENUM_TOO_MANY_VALUES");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, warning.Severity);
        Assert.Contains("150", warning.Message);
    }

    [Fact]
    public void Extract_WithVeryLongValue_ReturnsWarning()
    {
        // Arrange
        var longValue = new string('x', 300);
        var sql = $"CREATE TYPE status AS ENUM ('active', '{longValue}', 'inactive');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warning
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var warning = result.ValidationIssues.First(i => i.Code == "ENUM_VALUE_TOO_LONG");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, warning.Severity);
    }

    [Fact]
    public void Extract_WithMultipleIssues_ReturnsAllWarnings()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'active', '', 'inactive');"; // Duplicates + empty
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warnings
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        Assert.Equal(2, result.ValidationIssues.Count);
        Assert.Contains(result.ValidationIssues, i => i.Code == "ENUM_DUPLICATE_VALUES");
        Assert.Contains(result.ValidationIssues, i => i.Code == "ENUM_EMPTY_VALUE");
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_WithNullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_NonEnumBlock_ReturnsNotApplicable()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    [Fact]
    public void Extract_InvalidEnumSyntax_ReturnsFailure()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TYPE status ENUM ('active', 'inactive');"); // Missing AS

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_PostgreSQL18EnumExample_ExtractsCorrectly()
    {
        // Arrange - Пример из реального Schema.sql
        var sql = "CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("user_status", result.Definition.Name);
        Assert.Equal(4, result.Definition.Values.Count);
        Assert.Equal("active", result.Definition.Values[0]);
        Assert.Equal("inactive", result.Definition.Values[1]);
        Assert.Equal("suspended", result.Definition.Values[2]);
        Assert.Equal("deleted", result.Definition.Values[3]);
    }

    [Fact]
    public void Extract_OrderStatusEnum_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("order_status", result.Definition.Name);
        Assert.Equal(5, result.Definition.Values.Count);
        Assert.Contains("pending", result.Definition.Values);
        Assert.Contains("delivered", result.Definition.Values);
    }

    [Fact]
    public void Extract_PaymentMethodEnum_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE payment_method AS ENUM ('credit_card', 'debit_card', 'paypal', 'bank_transfer', 'crypto', 'cash');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("payment_method", result.Definition.Name);
        Assert.Equal(6, result.Definition.Values.Count);
    }

    #endregion

    #region Helper Methods

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql)
    {
        return
        [
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                StartLine = 1,
                EndLine = 1
            }
        ];
    }

    #endregion
}
