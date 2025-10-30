namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для SchemaAnalyzer - извлечение Composite типов
/// </summary>
public sealed class SchemaAnalyzerCompositeTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    #region ExtractComposites Tests

    [Fact]
    public void ExtractComposites_WithSingleComposite_ReturnsOne()
    {
        // Arrange
        var sql = "CREATE TYPE address AS (street VARCHAR(100), city VARCHAR(50));";

        // Act
        var result = _analyzer.ExtractComposites(sql);

        // Assert
        Assert.Single(result);
        Assert.Equal("address", result[0].Name);
        Assert.Equal(2, result[0].Attributes.Count);
    }

    [Fact]
    public void ExtractComposites_WithMultipleComposites_ReturnsAll()
    {
        // Arrange
        var sql = @"
CREATE TYPE address AS (street VARCHAR(100), city VARCHAR(50));
CREATE TYPE person AS (name VARCHAR(100), age INTEGER);
CREATE TYPE coordinates AS (latitude NUMERIC(9,6), longitude NUMERIC(9,6));
";

        // Act
        var result = _analyzer.ExtractComposites(sql);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, c => c.Name == "address");
        Assert.Contains(result, c => c.Name == "person");
        Assert.Contains(result, c => c.Name == "coordinates");
    }

    [Fact]
    public void ExtractComposites_WithMixedContent_ReturnsOnlyComposites()
    {
        // Arrange
        var sql = @"
CREATE TABLE users (id INT);

CREATE TYPE address AS (street VARCHAR(100));

CREATE TYPE status AS ENUM ('active', 'inactive');

CREATE TYPE person AS (name VARCHAR(100), age INT);
";

        // Act
        var result = _analyzer.ExtractComposites(sql);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.NotNull(c.Attributes));
    }

    [Fact]
    public void ExtractComposites_WithEmptySql_ThrowsArgumentException()
    {
        // Arrange
        var sql = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractComposites(sql));
    }

    [Fact]
    public void ExtractComposites_WithNullSql_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractComposites(null!));
    }

    #endregion

    #region ExtractComposites with Schema Tests

    [Fact]
    public void ExtractComposites_WithSchemaQualified_PreservesSchema()
    {
        // Arrange
        var sql = @"
CREATE TYPE public.address AS (street VARCHAR(100), city VARCHAR(50));
CREATE TYPE custom.person AS (name VARCHAR(100));
";

        // Act
        var result = _analyzer.ExtractComposites(sql);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Schema == "public" && c.Name == "address");
        Assert.Contains(result, c => c.Schema == "custom" && c.Name == "person");
    }

    #endregion

    #region ExtractComposites with Comments Tests

    [Fact]
    public void ExtractComposites_WithComments_PreservesComments()
    {
        // Arrange
        var sql = @"
-- Address composite type
CREATE TYPE address AS (street VARCHAR(100));
";

        // Act
        var result = _analyzer.ExtractComposites(sql);

        // Assert
        Assert.Single(result);
        Assert.Equal("Address composite type", result[0].SqlComment);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void ExtractComposites_WithAddressComposite_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
CREATE TYPE address AS (
    street VARCHAR(100),
    city VARCHAR(50),
    state CHAR(2),
    zip_code VARCHAR(10),
    country VARCHAR(50)
);
";

        // Act
        var result = _analyzer.ExtractComposites(sql);

        // Assert
        Assert.Single(result);
        var composite = result[0];
        Assert.Equal("address", composite.Name);
        Assert.Equal(5, composite.Attributes.Count);
        Assert.Contains(composite.Attributes, a => a.Name == "street" && a.DataType == "VARCHAR" && a.MaxLength == 100);
        Assert.Contains(composite.Attributes, a => a.Name == "state" && a.DataType == "CHAR" && a.MaxLength == 2);
    }

    [Fact]
    public void ExtractComposites_WithInventoryItemComposite_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
CREATE TYPE inventory_item AS (
    name VARCHAR(100),
    supplier_id INTEGER,
    price NUMERIC(10, 2)
);
";

        // Act
        var result = _analyzer.ExtractComposites(sql);

        // Assert
        Assert.Single(result);
        var composite = result[0];
        Assert.Equal("inventory_item", composite.Name);
        Assert.Equal(3, composite.Attributes.Count);
        Assert.Contains(composite.Attributes, a => a.Name == "price" && a.DataType == "NUMERIC" && a.NumericPrecision == 10 && a.NumericScale == 2);
    }

    [Fact]
    public void ExtractComposites_WithComplexTypesComposite_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
CREATE TYPE complex_type AS (
    id BIGINT,
    name VARCHAR(200),
    tags TEXT[],
    metadata JSONB,
    created_at TIMESTAMP,
    coordinates POINT
);
";

        // Act
        var result = _analyzer.ExtractComposites(sql);

        // Assert
        Assert.Single(result);
        var composite = result[0];
        Assert.Equal(6, composite.Attributes.Count);
        Assert.Contains(composite.Attributes, a => a.Name == "tags" && a.IsArray);
        Assert.Contains(composite.Attributes, a => a.Name == "metadata" && a.DataType == "JSONB");
    }

    #endregion
}
