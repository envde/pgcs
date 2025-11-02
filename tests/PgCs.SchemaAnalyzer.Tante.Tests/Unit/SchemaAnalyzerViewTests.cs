namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Consolidated tests for SchemaAnalyzer - view extraction
/// </summary>
public sealed class SchemaAnalyzerViewTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    [Fact]
    public void ExtractViews_BasicFunctionality_WorksCorrectly()
    {
        // Single view
        var sql1 = "CREATE VIEW active_users AS SELECT * FROM users WHERE status = 'active';";
        var result1 = _analyzer.ExtractViews(sql1);
        Assert.Single(result1);
        Assert.Equal("active_users", result1[0].Name);
        
        // Multiple views
        var sql2 = @"
CREATE VIEW v1 AS SELECT 1;
CREATE VIEW v2 AS SELECT 2;
CREATE MATERIALIZED VIEW mv AS SELECT 3;
";
        var result2 = _analyzer.ExtractViews(sql2);
        Assert.Equal(3, result2.Count);
        Assert.True(result2[2].IsMaterialized);
        
        // Schema-qualified
        var sql3 = "CREATE VIEW public.v AS SELECT 1;";
        var result3 = _analyzer.ExtractViews(sql3);
        Assert.Equal("public", result3[0].Schema);
        
        // With comments
        var sql4 = @"
-- Active users view
CREATE VIEW active_users AS SELECT 1;
";
        var result4 = _analyzer.ExtractViews(sql4);
        Assert.Equal("Active users view", result4[0].SqlComment);
        
        // Validation
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractViews(""));
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractViews(null!));
    }

    [Fact]
    public void ExtractViews_RealWorldExamples_ExtractsCorrectly()
    {
        var sql = @"
CREATE VIEW active_users AS 
    SELECT id, username, email 
    FROM users 
    WHERE status = 'active' 
    ORDER BY created_at DESC;

CREATE MATERIALIZED VIEW category_stats AS 
    SELECT category_id, COUNT(*) as total_items, SUM(revenue) as total_revenue
    FROM order_items
    GROUP BY category_id;
";
        var result = _analyzer.ExtractViews(sql);
        Assert.Equal(2, result.Count);
        Assert.False(result[0].IsMaterialized);
        Assert.True(result[1].IsMaterialized);
    }
}
