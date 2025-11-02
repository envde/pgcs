namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Consolidated tests for SchemaAnalyzer - domain type extraction and validation
/// </summary>
public sealed class SchemaAnalyzerDomainTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    /// <summary>
    /// Test 1: ExtractDomains - basic functionality, multiple domains, mixed content, schema-qualified, constraints, comments
    /// </summary>
    [Fact]
    public void ExtractDomains_BasicFunctionality_WorksCorrectly()
    {
        // Test 1.1: Single domain
        var sql1 = "CREATE DOMAIN email AS VARCHAR(255);";
        
        var result1 = _analyzer.ExtractDomains(sql1);
        
        Assert.Single(result1);
        Assert.Equal("email", result1[0].Name);
        Assert.Equal("VARCHAR(255)", result1[0].BaseType);
        
        // Test 1.2: Multiple domains
        var sql2 = @"
CREATE DOMAIN email AS VARCHAR(255);
CREATE DOMAIN positive_int AS INTEGER CHECK (VALUE > 0);
CREATE DOMAIN percentage AS NUMERIC(5, 2) CHECK (VALUE >= 0 AND VALUE <= 100);
";
        
        var result2 = _analyzer.ExtractDomains(sql2);
        
        Assert.Equal(3, result2.Count);
        Assert.Contains(result2, d => d.Name == "email");
        Assert.Contains(result2, d => d.Name == "positive_int");
        Assert.Contains(result2, d => d.Name == "percentage");
        
        // Test 1.3: Mixed content (domains + tables + enums + composites)
        var sql3 = @"
CREATE TABLE users (id INT);

CREATE DOMAIN email AS VARCHAR(255);

CREATE TYPE status AS ENUM ('active', 'inactive');

CREATE DOMAIN positive_int AS INTEGER CHECK (VALUE > 0);

CREATE TYPE address AS (street VARCHAR(100));
";
        
        var result3 = _analyzer.ExtractDomains(sql3);
        
        Assert.Equal(2, result3.Count);
        Assert.All(result3, d => Assert.NotNull(d.BaseType));
        
        // Test 1.4: Schema-qualified domains
        var sql4 = @"
CREATE DOMAIN public.email AS VARCHAR(255);
CREATE DOMAIN custom.positive_int AS INTEGER CHECK (VALUE > 0);
";
        
        var result4 = _analyzer.ExtractDomains(sql4);
        
        Assert.Equal(2, result4.Count);
        Assert.Contains(result4, d => d.Schema == "public" && d.Name == "email");
        Assert.Contains(result4, d => d.Schema == "custom" && d.Name == "positive_int");
        
        // Test 1.5: Domains with constraints (NOT NULL, DEFAULT, CHECK)
        var sql5 = @"
CREATE DOMAIN email AS VARCHAR(255) NOT NULL;
CREATE DOMAIN status AS VARCHAR(20) DEFAULT 'active';
CREATE DOMAIN age AS INTEGER CHECK (VALUE >= 0 AND VALUE <= 150);
";
        
        var result5 = _analyzer.ExtractDomains(sql5);
        
        Assert.Equal(3, result5.Count);
        Assert.Contains(result5, d => d.Name == "email" && d.IsNotNull);
        Assert.Contains(result5, d => d.Name == "status" && d.DefaultValue == "'active'");
        Assert.Contains(result5, d => d.Name == "age" && d.CheckConstraints.Count > 0);
        
        // Test 1.6: Domain with comments
        var sql6 = @"
-- Email address domain
CREATE DOMAIN email AS VARCHAR(255);
";
        
        var result6 = _analyzer.ExtractDomains(sql6);
        
        Assert.Single(result6);
        Assert.Equal("Email address domain", result6[0].SqlComment);
        
        // Test 1.7: Empty SQL should throw
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractDomains(""));
        
        // Test 1.8: Null SQL should throw
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractDomains(null!));
    }

    /// <summary>
    /// Test 2: Real-world domain types - email, positive numeric, percentage, username slug
    /// </summary>
    [Fact]
    public void ExtractDomains_RealWorldExamples_ExtractsCorrectly()
    {
        // Test 2.1: Email domain with regex CHECK constraint
        var sql1 = @"
CREATE DOMAIN email AS VARCHAR(255) 
    CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');
";
        
        var result1 = _analyzer.ExtractDomains(sql1);
        
        Assert.Single(result1);
        var domain1 = result1[0];
        Assert.Equal("email", domain1.Name);
        Assert.Equal("VARCHAR(255)", domain1.BaseType);
        Assert.Equal(255, domain1.MaxLength);
        Assert.Single(domain1.CheckConstraints);
        Assert.Contains("~*", domain1.CheckConstraints[0]);
        
        // Test 2.2: Positive numeric domain with DEFAULT and CHECK
        var sql2 = @"
CREATE DOMAIN positive_numeric AS NUMERIC(12, 2) 
    DEFAULT 0 
    CHECK (VALUE >= 0);
";
        
        var result2 = _analyzer.ExtractDomains(sql2);
        
        Assert.Single(result2);
        var domain2 = result2[0];
        Assert.Equal("positive_numeric", domain2.Name);
        Assert.Equal("NUMERIC(12, 2)", domain2.BaseType);
        Assert.Equal(12, domain2.NumericPrecision);
        Assert.Equal(2, domain2.NumericScale);
        Assert.Equal("0", domain2.DefaultValue);
        Assert.Single(domain2.CheckConstraints);
        
        // Test 2.3: Percentage domain with range CHECK constraint
        var sql3 = @"
CREATE DOMAIN percentage AS NUMERIC(5, 2) 
    CHECK (VALUE >= 0 AND VALUE <= 100);
";
        
        var result3 = _analyzer.ExtractDomains(sql3);
        
        Assert.Single(result3);
        var domain3 = result3[0];
        Assert.Equal("percentage", domain3.Name);
        Assert.Equal("NUMERIC(5, 2)", domain3.BaseType);
        Assert.Equal(5, domain3.NumericPrecision);
        Assert.Equal(2, domain3.NumericScale);
        Assert.Single(domain3.CheckConstraints);
        Assert.Contains("VALUE >= 0 AND VALUE <= 100", domain3.CheckConstraints[0]);
        
        // Test 2.4: Username slug domain with regex validation
        var sql4 = @"
CREATE DOMAIN username_slug AS VARCHAR(50) 
    CHECK (VALUE ~* '^[a-z0-9_-]+$');
";
        
        var result4 = _analyzer.ExtractDomains(sql4);
        
        Assert.Single(result4);
        var domain4 = result4[0];
        Assert.Equal("username_slug", domain4.Name);
        Assert.Equal("VARCHAR(50)", domain4.BaseType);
        Assert.Equal(50, domain4.MaxLength);
        Assert.Single(domain4.CheckConstraints);
        Assert.Contains("~*", domain4.CheckConstraints[0]);
    }
}
