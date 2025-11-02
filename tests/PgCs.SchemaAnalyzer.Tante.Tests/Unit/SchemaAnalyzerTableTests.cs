namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Consolidated tests for SchemaAnalyzer - table extraction
/// </summary>
public sealed class SchemaAnalyzerTableTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    [Fact]
    public void ExtractTables_BasicFunctionality_WorksCorrectly()
    {
        // Single table
        var sql1 = "CREATE TABLE users (id INT, name VARCHAR(100));";
        var result1 = _analyzer.ExtractTables(sql1);
        Assert.Single(result1);
        Assert.Equal("users", result1[0].Name);
        Assert.True(result1[0].Columns.Count >= 1); // At least one column extracted
        
        // Multiple tables
        var sql2 = @"
CREATE TABLE users (id INT);
CREATE TABLE orders (id INT);
CREATE TABLE products (id INT);
";
        var result2 = _analyzer.ExtractTables(sql2);
        Assert.Equal(3, result2.Count);
        
        // Schema-qualified
        var sql3 = "CREATE TABLE public.users (id INT);";

        var result3 = _analyzer.ExtractTables(sql3);
        Assert.Equal("public", result3[0].Schema);
        
        // With comments
        var sql4 = @"
-- Users table
CREATE TABLE users (id INT);
";
        var result4 = _analyzer.ExtractTables(sql4);
        Assert.Equal("Users table", result4[0].SqlComment);
        
        // Validation
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractTables(""));
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractTables(null!));
    }

    [Fact]
    public void ExtractTables_RealWorldExamples_ExtractsCorrectly()
    {
        var sql = @"
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE orders (
    id BIGSERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    total NUMERIC(10,2) NOT NULL,
    status VARCHAR(20) DEFAULT 'pending'
);
";
        var result = _analyzer.ExtractTables(sql);
        Assert.Equal(2, result.Count);
        Assert.Equal(4, result[0].Columns.Count);
        Assert.Equal(4, result[1].Columns.Count);
    }
}
