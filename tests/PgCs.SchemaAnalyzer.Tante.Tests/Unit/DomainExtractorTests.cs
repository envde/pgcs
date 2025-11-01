using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для DomainExtractor
/// </summary>
public sealed class DomainExtractorTests
{
    private readonly IExtractor<DomainTypeDefinition> _extractor = new DomainExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidDomainBlock_ReturnsTrue()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE DOMAIN email AS VARCHAR(255);");

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
    public void CanExtract_WithNullBlocks_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
    }

    #endregion

    #region Extract Simple Domain Tests

    [Fact]
    public void Extract_SimpleDomain_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("email", result.Definition.Name);
        Assert.Null(result.Definition.Schema);
        Assert.Equal("VARCHAR(255)", result.Definition.BaseType);
        Assert.Equal(255, result.Definition.MaxLength);
    }

    [Fact]
    public void Extract_DomainWithSchema_ReturnsDefinitionWithSchema()
    {
        // Arrange
        var sql = "CREATE DOMAIN public.email AS VARCHAR(255);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("email", result.Definition.Name);
        Assert.Equal("public", result.Definition.Schema);
    }

    [Fact]
    public void Extract_DomainWithNumericType_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN positive_numeric AS NUMERIC(12, 2);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("positive_numeric", result.Definition.Name);
        Assert.Equal("NUMERIC(12, 2)", result.Definition.BaseType);
        Assert.Equal(12, result.Definition.NumericPrecision);
        Assert.Equal(2, result.Definition.NumericScale);
    }

    #endregion

    #region Extract with Constraints Tests

    [Fact]
    public void Extract_DomainWithNotNull_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255) NOT NULL;";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.True(result.Definition.IsNotNull);
    }

    [Fact]
    public void Extract_DomainWithDefault_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN status AS VARCHAR(20) DEFAULT 'active';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("'active'", result.Definition.DefaultValue);
    }

    [Fact]
    public void Extract_DomainWithCheckConstraint_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN positive_int AS INTEGER CHECK (VALUE > 0);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Single(result.Definition.CheckConstraints);
        Assert.Equal("VALUE > 0", result.Definition.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_DomainWithMultipleCheckConstraints_ExtractsAll()
    {
        // Arrange
        var sql = "CREATE DOMAIN percentage AS NUMERIC(5, 2) CHECK (VALUE >= 0) CHECK (VALUE <= 100);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(2, result.Definition.CheckConstraints.Count);
        Assert.Contains("VALUE >= 0", result.Definition.CheckConstraints);
        Assert.Contains("VALUE <= 100", result.Definition.CheckConstraints);
    }

    [Fact]
    public void Extract_DomainWithCollation_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN case_insensitive_text AS TEXT COLLATE \"en_US\";";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("en_US", result.Definition.Collation);
    }

    #endregion

    #region Extract Complex Domains Tests

    [Fact]
    public void Extract_DomainWithAllFeatures_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"CREATE DOMAIN positive_numeric AS NUMERIC(12, 2)
    DEFAULT 0
    NOT NULL
    CHECK (VALUE >= 0);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("positive_numeric", result.Definition.Name);
        Assert.Equal("NUMERIC(12, 2)", result.Definition.BaseType);
        Assert.Equal("0", result.Definition.DefaultValue);
        Assert.True(result.Definition.IsNotNull);
        Assert.Single(result.Definition.CheckConstraints);
        Assert.Equal("VALUE >= 0", result.Definition.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_DomainWithComplexCheck_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255) CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Z|a-z]{2,}$');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Single(result.Definition.CheckConstraints);
        Assert.Contains("~*", result.Definition.CheckConstraints[0]);
    }

    #endregion

    #region Extract with Comments Tests

    [Fact]
    public void Extract_DomainWithHeaderComment_PreservesComment()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255);";
        var block = new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            HeaderComment = "Email address with validation",
            StartLine = 1,
            EndLine = 1
        };
        var blocks = new[] { block };

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("Email address with validation", result.Definition.SqlComment);
    }

    [Fact]
    public void Extract_DomainBlock_PreservesRawSql()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255);";
        var rawSql = "-- Email domain\nCREATE DOMAIN email AS VARCHAR(255);";
        var block = new SqlBlock
        {
            Content = sql,
            RawContent = rawSql,
            StartLine = 1,
            EndLine = 2
        };
        var blocks = new[] { block };

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(rawSql, result.Definition.RawSql);
    }

    #endregion

    #region ValidationIssues Tests

    [Fact]
    public void Extract_WithExcessiveStringLength_ReturnsWarning()
    {
        // Arrange
        var sql = "CREATE DOMAIN huge_text AS VARCHAR(50000);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warning
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var warning = result.ValidationIssues.First(i => i.Code == "DOMAIN_EXCESSIVE_LENGTH");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, warning.Severity);
        Assert.Contains("50000", warning.Message);
    }

    [Fact]
    public void Extract_WithInvalidNumericScale_ReturnsWarning()
    {
        // Arrange - scale > precision
        var sql = "CREATE DOMAIN bad_numeric AS NUMERIC(5, 10);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warning
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var warning = result.ValidationIssues.First(i => i.Code == "DOMAIN_INVALID_NUMERIC_PARAMS");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, warning.Severity);
        Assert.Contains("scale", warning.Message);
    }

    [Fact]
    public void Extract_WithExcessivePrecision_ReturnsWarning()
    {
        // Arrange
        var sql = "CREATE DOMAIN huge_numeric AS NUMERIC(2000, 2);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warning
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var warning = result.ValidationIssues.First(i => i.Code == "DOMAIN_EXCESSIVE_PRECISION");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, warning.Severity);
        Assert.Contains("2000", warning.Message);
    }

    [Fact]
    public void Extract_WithTooManyCheckConstraints_ReturnsWarning()
    {
        // Arrange
        var sql = "CREATE DOMAIN complex AS INTEGER CHECK (VALUE > 0) CHECK (VALUE < 100) CHECK (VALUE != 50) CHECK (VALUE != 75) CHECK (VALUE != 25) CHECK (VALUE != 10);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warning
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var warning = result.ValidationIssues.First(i => i.Code == "DOMAIN_TOO_MANY_CHECKS");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, warning.Severity);
        Assert.Contains("6", warning.Message);
    }

    [Fact]
    public void Extract_WithMultipleIssues_ReturnsAllWarnings()
    {
        // Arrange - excessive precision + too many checks
        var sql = "CREATE DOMAIN complex AS NUMERIC(2000, 2) CHECK (VALUE > 0) CHECK (VALUE < 100) CHECK (VALUE != 50) CHECK (VALUE != 75) CHECK (VALUE != 25) CHECK (VALUE != 10);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess); // Still succeeds, but with warnings
        Assert.NotNull(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        Assert.Equal(2, result.ValidationIssues.Count);
        Assert.Contains(result.ValidationIssues, i => i.Code == "DOMAIN_EXCESSIVE_PRECISION");
        Assert.Contains(result.ValidationIssues, i => i.Code == "DOMAIN_TOO_MANY_CHECKS");
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_WithNullBlocks_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_NonDomainBlock_ReturnsNotApplicable()
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
    public void Extract_InvalidDomainSyntax_ReturnsFailure()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE DOMAIN INVALID SYNTAX");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        var error = result.ValidationIssues.First(i => i.Code == "DOMAIN_PARSE_ERROR");
        Assert.Equal(ValidationIssue.ValidationSeverity.Error, error.Severity);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_EmailDomainFromSchema_ExtractsCorrectly()
    {
        // Arrange - Пример из реального Schema.sql
        var sql = "CREATE DOMAIN email AS VARCHAR(255) CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Z|a-z]{2,}$');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("email", result.Definition.Name);
        Assert.Equal("VARCHAR(255)", result.Definition.BaseType);
        Assert.Single(result.Definition.CheckConstraints);
    }

    [Fact]
    public void Extract_PositiveNumericDomainFromSchema_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN positive_numeric AS NUMERIC(12, 2) CHECK (VALUE >= 0);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("positive_numeric", result.Definition.Name);
        Assert.Equal("NUMERIC(12, 2)", result.Definition.BaseType);
        Assert.Equal(12, result.Definition.NumericPrecision);
        Assert.Equal(2, result.Definition.NumericScale);
        Assert.Single(result.Definition.CheckConstraints);
        Assert.Equal("VALUE >= 0", result.Definition.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_PercentageDomainFromSchema_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN percentage AS NUMERIC(5, 2) CHECK (VALUE >= 0 AND VALUE <= 100);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("percentage", result.Definition.Name);
        Assert.Equal("NUMERIC(5, 2)", result.Definition.BaseType);
        Assert.Single(result.Definition.CheckConstraints);
        Assert.Contains("VALUE >= 0 AND VALUE <= 100", result.Definition.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_UsernameSlugDomainFromSchema_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN username_slug AS VARCHAR(50) CHECK (VALUE ~* '^[a-z0-9_-]+$');";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("username_slug", result.Definition.Name);
        Assert.Equal("VARCHAR(50)", result.Definition.BaseType);
        Assert.Equal(50, result.Definition.MaxLength);
        Assert.Single(result.Definition.CheckConstraints);
    }

    #endregion

    #region Different Base Types Tests

    [Fact]
    public void Extract_DomainWithTextType_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN description AS TEXT NOT NULL;";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("TEXT", result.Definition.BaseType);
        Assert.True(result.Definition.IsNotNull);
    }

    [Fact]
    public void Extract_DomainWithIntegerType_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN age AS INTEGER CHECK (VALUE >= 0 AND VALUE <= 150);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("INTEGER", result.Definition.BaseType);
    }

    [Fact]
    public void Extract_DomainWithTimestampType_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN created_timestamp AS TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL;";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("TIMESTAMP", result.Definition.BaseType);
        Assert.Equal("CURRENT_TIMESTAMP", result.Definition.DefaultValue);
        Assert.True(result.Definition.IsNotNull);
    }

    #endregion

    #region Helper Methods

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql)
    {
        return new[]
        {
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                StartLine = 1,
                EndLine = 1
            }
        };
    }

    #endregion
}
