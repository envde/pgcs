namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Consolidated tests for SchemaAnalyzer - composite type extraction and validation
/// </summary>
public sealed class SchemaAnalyzerCompositeTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    /// <summary>
    /// Test 1: ExtractComposites - basic functionality, multiple composites, mixed content, schema-qualified, comments
    /// </summary>
    [Fact]
    public void ExtractComposites_BasicFunctionality_WorksCorrectly()
    {
        // Test 1.1: Single composite type
        var sql1 = "CREATE TYPE address AS (street VARCHAR(100), city VARCHAR(50));";
        
        var result1 = _analyzer.ExtractComposites(sql1);
        
        Assert.Single(result1);
        Assert.Equal("address", result1[0].Name);
        Assert.Equal(2, result1[0].Attributes.Count);
        
        // Test 1.2: Multiple composites
        var sql2 = @"
CREATE TYPE address AS (street VARCHAR(100), city VARCHAR(50));
CREATE TYPE person AS (name VARCHAR(100), age INTEGER);
CREATE TYPE coordinates AS (latitude NUMERIC(9,6), longitude NUMERIC(9,6));
";
        
        var result2 = _analyzer.ExtractComposites(sql2);
        
        Assert.Equal(3, result2.Count);
        Assert.Contains(result2, c => c.Name == "address");
        Assert.Contains(result2, c => c.Name == "person");
        Assert.Contains(result2, c => c.Name == "coordinates");
        
        // Test 1.3: Mixed content (composites + enums + tables)
        var sql3 = @"
CREATE TABLE users (id INT);

CREATE TYPE address AS (street VARCHAR(100));

CREATE TYPE status AS ENUM ('active', 'inactive');

CREATE TYPE person AS (name VARCHAR(100), age INT);
";
        
        var result3 = _analyzer.ExtractComposites(sql3);
        
        Assert.Equal(2, result3.Count);
        Assert.All(result3, c => Assert.NotNull(c.Attributes));
        
        // Test 1.4: Schema-qualified composite types
        var sql4 = @"
CREATE TYPE public.address AS (street VARCHAR(100), city VARCHAR(50));
CREATE TYPE custom.person AS (name VARCHAR(100));
";
        
        var result4 = _analyzer.ExtractComposites(sql4);
        
        Assert.Equal(2, result4.Count);
        Assert.Contains(result4, c => c.Schema == "public" && c.Name == "address");
        Assert.Contains(result4, c => c.Schema == "custom" && c.Name == "person");
        
        // Test 1.5: Composite with comments
        var sql5 = @"
-- Address composite type
CREATE TYPE address AS (street VARCHAR(100));
";
        
        var result5 = _analyzer.ExtractComposites(sql5);
        
        Assert.Single(result5);
        Assert.Equal("Address composite type", result5[0].SqlComment);
        
        // Test 1.6: Empty SQL should throw
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractComposites(""));
        
        // Test 1.7: Null SQL should throw
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractComposites(null!));
    }

    /// <summary>
    /// Test 2: Real-world composite types - address, inventory, complex types with arrays/JSONB
    /// </summary>
    [Fact]
    public void ExtractComposites_RealWorldExamples_ExtractsCorrectly()
    {
        // Test 2.1: Address composite (VARCHAR, CHAR with max length)
        var sql1 = @"
CREATE TYPE address AS (
    street VARCHAR(100),
    city VARCHAR(50),
    state CHAR(2),
    zip_code VARCHAR(10),
    country VARCHAR(50)
);
";
        
        var result1 = _analyzer.ExtractComposites(sql1);
        
        Assert.Single(result1);
        var composite1 = result1[0];
        Assert.Equal("address", composite1.Name);
        Assert.Equal(5, composite1.Attributes.Count);
        Assert.Contains(composite1.Attributes, a => a.Name == "street" && a.DataType == "VARCHAR" && a.MaxLength == 100);
        Assert.Contains(composite1.Attributes, a => a.Name == "state" && a.DataType == "CHAR" && a.MaxLength == 2);
        
        // Test 2.2: Inventory item composite (NUMERIC with precision/scale)
        var sql2 = @"
CREATE TYPE inventory_item AS (
    name VARCHAR(100),
    supplier_id INTEGER,
    price NUMERIC(10, 2)
);
";
        
        var result2 = _analyzer.ExtractComposites(sql2);
        
        Assert.Single(result2);
        var composite2 = result2[0];
        Assert.Equal("inventory_item", composite2.Name);
        Assert.Equal(3, composite2.Attributes.Count);
        Assert.Contains(composite2.Attributes, a => a.Name == "price" && a.DataType == "NUMERIC" && a.NumericPrecision == 10 && a.NumericScale == 2);
        
        // Test 2.3: Complex composite (arrays, JSONB, POINT, TIMESTAMP)
        var sql3 = @"
CREATE TYPE complex_type AS (
    id BIGINT,
    name VARCHAR(200),
    tags TEXT[],
    metadata JSONB,
    created_at TIMESTAMP,
    coordinates POINT
);
";
        
        var result3 = _analyzer.ExtractComposites(sql3);
        
        Assert.Single(result3);
        var composite3 = result3[0];
        Assert.Equal(6, composite3.Attributes.Count);
        Assert.Contains(composite3.Attributes, a => a.Name == "tags" && a.IsArray);
        Assert.Contains(composite3.Attributes, a => a.Name == "metadata" && a.DataType == "JSONB");
    }
}
