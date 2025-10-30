using PgCs.Core.Extraction.Block;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для DomainExtractor
/// </summary>
public sealed class DomainExtractorTests
{
    private readonly IDomainExtractor _extractor = new DomainExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidDomainBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("CREATE DOMAIN email AS VARCHAR(255);");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id INT PRIMARY KEY);");

        // Act
        var result = _extractor.CanExtract(block);

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

    #region Extract Simple Domain Tests

    [Fact]
    public void Extract_SimpleDomain_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("email", result.Name);
        Assert.Null(result.Schema);
        Assert.Equal("VARCHAR(255)", result.BaseType);
        Assert.Equal(255, result.MaxLength);
    }

    [Fact]
    public void Extract_DomainWithSchema_ReturnsDefinitionWithSchema()
    {
        // Arrange
        var sql = "CREATE DOMAIN public.email AS VARCHAR(255);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("email", result.Name);
        Assert.Equal("public", result.Schema);
    }

    [Fact]
    public void Extract_DomainWithNumericType_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN positive_numeric AS NUMERIC(12, 2);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("positive_numeric", result.Name);
        Assert.Equal("NUMERIC(12, 2)", result.BaseType);
        Assert.Equal(12, result.NumericPrecision);
        Assert.Equal(2, result.NumericScale);
    }

    #endregion

    #region Extract with Constraints Tests

    [Fact]
    public void Extract_DomainWithNotNull_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255) NOT NULL;";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsNotNull);
    }

    [Fact]
    public void Extract_DomainWithDefault_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN status AS VARCHAR(20) DEFAULT 'active';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("'active'", result.DefaultValue);
    }

    [Fact]
    public void Extract_DomainWithCheckConstraint_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN positive_int AS INTEGER CHECK (VALUE > 0);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.CheckConstraints);
        Assert.Equal("VALUE > 0", result.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_DomainWithMultipleCheckConstraints_ExtractsAll()
    {
        // Arrange
        var sql = "CREATE DOMAIN percentage AS NUMERIC(5, 2) CHECK (VALUE >= 0) CHECK (VALUE <= 100);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.CheckConstraints.Count);
        Assert.Contains("VALUE >= 0", result.CheckConstraints);
        Assert.Contains("VALUE <= 100", result.CheckConstraints);
    }

    [Fact]
    public void Extract_DomainWithCollation_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN case_insensitive_text AS TEXT COLLATE \"en_US\";";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("en_US", result.Collation);
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
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("positive_numeric", result.Name);
        Assert.Equal("NUMERIC(12, 2)", result.BaseType);
        Assert.Equal("0", result.DefaultValue);
        Assert.True(result.IsNotNull);
        Assert.Single(result.CheckConstraints);
        Assert.Equal("VALUE >= 0", result.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_DomainWithComplexCheck_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255) CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Z|a-z]{2,}$');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.CheckConstraints);
        Assert.Contains("~*", result.CheckConstraints[0]);
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

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Email address with validation", result.SqlComment);
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

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rawSql, result.RawSql);
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
    public void Extract_NonDomainBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id INT PRIMARY KEY);");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_EmailDomainFromSchema_ExtractsCorrectly()
    {
        // Arrange - Пример из реального Schema.sql
        var sql = "CREATE DOMAIN email AS VARCHAR(255) CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Z|a-z]{2,}$');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("email", result.Name);
        Assert.Equal("VARCHAR(255)", result.BaseType);
        Assert.Single(result.CheckConstraints);
    }

    [Fact]
    public void Extract_PositiveNumericDomainFromSchema_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN positive_numeric AS NUMERIC(12, 2) CHECK (VALUE >= 0);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("positive_numeric", result.Name);
        Assert.Equal("NUMERIC(12, 2)", result.BaseType);
        Assert.Equal(12, result.NumericPrecision);
        Assert.Equal(2, result.NumericScale);
        Assert.Single(result.CheckConstraints);
        Assert.Equal("VALUE >= 0", result.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_PercentageDomainFromSchema_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN percentage AS NUMERIC(5, 2) CHECK (VALUE >= 0 AND VALUE <= 100);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("percentage", result.Name);
        Assert.Equal("NUMERIC(5, 2)", result.BaseType);
        Assert.Single(result.CheckConstraints);
        Assert.Contains("VALUE >= 0 AND VALUE <= 100", result.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_UsernameSlugDomainFromSchema_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN username_slug AS VARCHAR(50) CHECK (VALUE ~* '^[a-z0-9_-]+$');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("username_slug", result.Name);
        Assert.Equal("VARCHAR(50)", result.BaseType);
        Assert.Equal(50, result.MaxLength);
        Assert.Single(result.CheckConstraints);
    }

    #endregion

    #region Different Base Types Tests

    [Fact]
    public void Extract_DomainWithTextType_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN description AS TEXT NOT NULL;";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEXT", result.BaseType);
        Assert.True(result.IsNotNull);
    }

    [Fact]
    public void Extract_DomainWithIntegerType_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN age AS INTEGER CHECK (VALUE >= 0 AND VALUE <= 150);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INTEGER", result.BaseType);
    }

    [Fact]
    public void Extract_DomainWithTimestampType_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE DOMAIN created_timestamp AS TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL;";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TIMESTAMP", result.BaseType);
        Assert.Equal("CURRENT_TIMESTAMP", result.DefaultValue);
        Assert.True(result.IsNotNull);
    }

    #endregion

    #region Helper Methods

    private static SqlBlock CreateBlock(string sql)
    {
        return new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            StartLine = 1,
            EndLine = 1
        };
    }

    #endregion
}
