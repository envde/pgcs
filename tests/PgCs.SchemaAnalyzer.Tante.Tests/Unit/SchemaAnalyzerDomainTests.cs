namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для SchemaAnalyzer - извлечение Domain типов
/// </summary>
public sealed class SchemaAnalyzerDomainTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    #region ExtractDomains Tests

    [Fact]
    public void ExtractDomains_WithSingleDomain_ReturnsOne()
    {
        // Arrange
        var sql = "CREATE DOMAIN email AS VARCHAR(255);";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Single(result);
        Assert.Equal("email", result[0].Name);
        Assert.Equal("VARCHAR(255)", result[0].BaseType);
    }

    [Fact]
    public void ExtractDomains_WithMultipleDomains_ReturnsAll()
    {
        // Arrange
        var sql = @"
CREATE DOMAIN email AS VARCHAR(255);
CREATE DOMAIN positive_int AS INTEGER CHECK (VALUE > 0);
CREATE DOMAIN percentage AS NUMERIC(5, 2) CHECK (VALUE >= 0 AND VALUE <= 100);
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, d => d.Name == "email");
        Assert.Contains(result, d => d.Name == "positive_int");
        Assert.Contains(result, d => d.Name == "percentage");
    }

    [Fact]
    public void ExtractDomains_WithMixedContent_ReturnsOnlyDomains()
    {
        // Arrange
        var sql = @"
CREATE TABLE users (id INT);

CREATE DOMAIN email AS VARCHAR(255);

CREATE TYPE status AS ENUM ('active', 'inactive');

CREATE DOMAIN positive_int AS INTEGER CHECK (VALUE > 0);

CREATE TYPE address AS (street VARCHAR(100));
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.NotNull(d.BaseType));
    }

    [Fact]
    public void ExtractDomains_WithEmptySql_ThrowsArgumentException()
    {
        // Arrange
        var sql = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractDomains(sql));
    }

    [Fact]
    public void ExtractDomains_WithNullSql_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractDomains(null!));
    }

    #endregion

    #region ExtractDomains with Schema Tests

    [Fact]
    public void ExtractDomains_WithSchemaQualified_PreservesSchema()
    {
        // Arrange
        var sql = @"
CREATE DOMAIN public.email AS VARCHAR(255);
CREATE DOMAIN custom.positive_int AS INTEGER CHECK (VALUE > 0);
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Schema == "public" && d.Name == "email");
        Assert.Contains(result, d => d.Schema == "custom" && d.Name == "positive_int");
    }

    #endregion

    #region ExtractDomains with Constraints Tests

    [Fact]
    public void ExtractDomains_WithVariousConstraints_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
CREATE DOMAIN email AS VARCHAR(255) NOT NULL;
CREATE DOMAIN status AS VARCHAR(20) DEFAULT 'active';
CREATE DOMAIN age AS INTEGER CHECK (VALUE >= 0 AND VALUE <= 150);
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, d => d.Name == "email" && d.IsNotNull);
        Assert.Contains(result, d => d.Name == "status" && d.DefaultValue == "'active'");
        Assert.Contains(result, d => d.Name == "age" && d.CheckConstraints.Count > 0);
    }

    #endregion

    #region ExtractDomains with Comments Tests

    [Fact]
    public void ExtractDomains_WithComments_PreservesComments()
    {
        // Arrange
        var sql = @"
-- Email address domain
CREATE DOMAIN email AS VARCHAR(255);
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Single(result);
        Assert.Equal("Email address domain", result[0].SqlComment);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void ExtractDomains_WithEmailDomain_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
CREATE DOMAIN email AS VARCHAR(255) 
    CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Single(result);
        var domain = result[0];
        Assert.Equal("email", domain.Name);
        Assert.Equal("VARCHAR(255)", domain.BaseType);
        Assert.Equal(255, domain.MaxLength);
        Assert.Single(domain.CheckConstraints);
        Assert.Contains("~*", domain.CheckConstraints[0]);
    }

    [Fact]
    public void ExtractDomains_WithPositiveNumericDomain_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
CREATE DOMAIN positive_numeric AS NUMERIC(12, 2) 
    DEFAULT 0 
    CHECK (VALUE >= 0);
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Single(result);
        var domain = result[0];
        Assert.Equal("positive_numeric", domain.Name);
        Assert.Equal("NUMERIC(12, 2)", domain.BaseType);
        Assert.Equal(12, domain.NumericPrecision);
        Assert.Equal(2, domain.NumericScale);
        Assert.Equal("0", domain.DefaultValue);
        Assert.Single(domain.CheckConstraints);
    }

    [Fact]
    public void ExtractDomains_WithPercentageDomain_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
CREATE DOMAIN percentage AS NUMERIC(5, 2) 
    CHECK (VALUE >= 0 AND VALUE <= 100);
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Single(result);
        var domain = result[0];
        Assert.Equal("percentage", domain.Name);
        Assert.Equal("NUMERIC(5, 2)", domain.BaseType);
        Assert.Equal(5, domain.NumericPrecision);
        Assert.Equal(2, domain.NumericScale);
        Assert.Single(domain.CheckConstraints);
        Assert.Contains("VALUE >= 0 AND VALUE <= 100", domain.CheckConstraints[0]);
    }

    [Fact]
    public void ExtractDomains_WithUsernameSlugDomain_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
CREATE DOMAIN username_slug AS VARCHAR(50) 
    CHECK (VALUE ~* '^[a-z0-9_-]+$');
";

        // Act
        var result = _analyzer.ExtractDomains(sql);

        // Assert
        Assert.Single(result);
        var domain = result[0];
        Assert.Equal("username_slug", domain.Name);
        Assert.Equal("VARCHAR(50)", domain.BaseType);
        Assert.Equal(50, domain.MaxLength);
        Assert.Single(domain.CheckConstraints);
        Assert.Contains("~*", domain.CheckConstraints[0]);
    }

    #endregion
}
