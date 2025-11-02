namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Consolidated tests for SchemaAnalyzer - enum type extraction
/// </summary>
public sealed class SchemaAnalyzerEnumTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    [Fact]
    public void ExtractEnums_BasicFunctionality_WorksCorrectly()
    {
        // Single enum
        var sql1 = "CREATE TYPE status AS ENUM ('active', 'inactive');";
        var result1 = _analyzer.ExtractEnums(sql1);
        Assert.Single(result1);
        Assert.Equal("status", result1[0].Name);
        Assert.Equal(2, result1[0].Values.Count);
        
        // Multiple enums
        var sql2 = @"
CREATE TYPE status AS ENUM ('active', 'inactive');
CREATE TYPE priority AS ENUM ('low', 'medium', 'high');
CREATE TYPE color AS ENUM ('red', 'green', 'blue');
";
        var result2 = _analyzer.ExtractEnums(sql2);
        Assert.Equal(3, result2.Count);
        
        // Mixed content
        var sql3 = @"
CREATE TABLE users (id INT);
CREATE TYPE status AS ENUM ('active', 'inactive');
CREATE VIEW v AS SELECT 1;
CREATE TYPE priority AS ENUM ('low', 'high');
";
        var result3 = _analyzer.ExtractEnums(sql3);
        Assert.Equal(2, result3.Count);
        
        // Schema-qualified
        var sql4 = @"
CREATE TYPE public.status AS ENUM ('active');
CREATE TYPE app.priority AS ENUM ('low');
";
        var result4 = _analyzer.ExtractEnums(sql4);
        Assert.Equal(2, result4.Count);
        Assert.Equal("public", result4[0].Schema);
        
        // With comments
        var sql5 = @"
-- Status enum
CREATE TYPE status AS ENUM ('active');
";
        var result5 = _analyzer.ExtractEnums(sql5);
        Assert.Equal("Status enum", result5[0].SqlComment);
        
        // Validation
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractEnums(""));
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractEnums(null!));
    }

    [Fact]
    public void ExtractEnums_RealWorldExamples_ExtractsCorrectly()
    {
        var sql = @"
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');
CREATE TYPE priority AS ENUM ('low', 'medium', 'high', 'critical');
";
        var result = _analyzer.ExtractEnums(sql);
        Assert.Equal(3, result.Count);
        Assert.Equal(4, result[0].Values.Count);
        Assert.Equal(5, result[1].Values.Count);
        Assert.Equal(4, result[2].Values.Count);
    }
}
