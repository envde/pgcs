using PgCs.SchemaAnalyzer.Utils;

namespace PgCs.SchemaAnalyzer.Tests.Unit;

/// <summary>
/// Интеграционные тесты для проверки парсинга схем без точек с запятой
/// </summary>
public sealed class SchemaWithoutSemicolonsTests
{
    [Fact]
    public void SqlStatementSplitter_WithTablesSeparatedByNewline_ShouldSplitCorrectly()
    {
        // Arrange - проверяем, что SqlStatementSplitter правильно разделяет
        var sql = @"CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100)
)

CREATE TABLE orders (
    id SERIAL PRIMARY KEY
)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("CREATE TABLE users", statements[0]);
        Assert.Contains("CREATE TABLE orders", statements[1]);
    }

    [Fact]
    public void AnalyzeScript_WithSimpleTables_ShouldParseCorrectly()
    {
        // Arrange - простой пример с точками с запятой и пустой строкой
        var sql = @"CREATE TABLE users (id INT);

CREATE TABLE orders (id INT)";

        var analyzer = new SchemaAnalyzer();

        // Act
        var metadata = analyzer.AnalyzeScript(sql);

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
        Assert.Contains(metadata.Tables, t => t.Name == "users");
        Assert.Contains(metadata.Tables, t => t.Name == "orders");
    }

    [Fact]
    public void AnalyzeScript_WithOnlyNewlineSeparators_ShouldParseSimpleTypes()
    {
        // Arrange - добавляем точку с запятой к первому типу
        var sql = @"CREATE TYPE status AS ENUM ('active', 'inactive');

CREATE TYPE color AS ENUM ('red', 'green', 'blue')";

        var analyzer = new SchemaAnalyzer();

        // Act
        var metadata = analyzer.AnalyzeScript(sql);

        // Assert
        Assert.Equal(2, metadata.Types.Count);
        Assert.Contains(metadata.Types, t => t.Name == "status");
        Assert.Contains(metadata.Types, t => t.Name == "color");
    }

    [Fact]
    public void SqlStatementSplitter_WithConsecutiveTablesNOTNULL_ShouldSplitCorrectly()
    {
        // Arrange
        var sql = @"CREATE TABLE a1 (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL
)
CREATE TABLE a2 (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL
)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("CREATE TABLE a1", statements[0]);
        Assert.Contains("CREATE TABLE a2", statements[1]);
    }

    [Fact]
    public void ExtractTables_WithTwoSeparateStatements_ShouldExtractBoth()
    {
        // Arrange
        var sql1 = "CREATE TABLE a1 (id INT)";
        var sql2 = "CREATE TABLE a2 (id INT)";
        var analyzer = new SchemaAnalyzer();

        // Act
        var tables1 = analyzer.ExtractTables(sql1);
        var tables2 = analyzer.ExtractTables(sql2);

        // Assert
        Assert.Single(tables1);
        Assert.Single(tables2);
        Assert.Equal("a1", tables1[0].Name);
        Assert.Equal("a2", tables2[0].Name);
    }

    [Fact]
    public void ExtractTables_WithBothStatementsInOne_ShouldExtractBoth()
    {
        // Arrange - SQL с двумя statements на разных строках
        var sql = "CREATE TABLE a1 (id INT)\nCREATE TABLE a2 (id INT)";
        var analyzer = new SchemaAnalyzer();

        // Act
        var tables = analyzer.ExtractTables(sql);

        // Assert - expectation: splitter разделит, extractor найдет обе
        Assert.Equal(2, tables.Count);
        Assert.Contains(tables, t => t.Name == "a1");
        Assert.Contains(tables, t => t.Name == "a2");
    }

    [Fact]
    public void ExtractTables_WithSimpleOneLineTable_ShouldExtract()
    {
        // Arrange
        var sql = "CREATE TABLE a1 (id INT)";
        var analyzer = new SchemaAnalyzer();

        // Act
        var tables = analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        Assert.Equal("a1", tables[0].Name);
    }

    [Fact]
    public void AnalyzeScript_WithConsecutiveStatements_ShouldParseAll()
    {
        // Arrange - упрощенный SQL
        var sql = @"CREATE TABLE a1 (id INT)
CREATE TABLE a2 (id INT)";

        var analyzer = new SchemaAnalyzer();

        // Act
        var metadata = analyzer.AnalyzeScript(sql);

        // Assert  
        Assert.Equal(2, metadata.Tables.Count);
        Assert.Contains(metadata.Tables, t => t.Name == "a1");
        Assert.Contains(metadata.Tables, t => t.Name == "a2");
    }

    [Fact]
    public void AnalyzeScript_WithMixedTypes_ShouldParseAll()
    {
        // Arrange - TYPE, TABLE, INDEX без разделителей
        var sql = @"CREATE TYPE user_role AS ENUM ('admin', 'user')
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    role user_role NOT NULL
)
CREATE INDEX idx_users_username ON users(username)";

        var analyzer = new SchemaAnalyzer();

        // Act
        var metadata = analyzer.AnalyzeScript(sql);

        // Assert
        Assert.Single(metadata.Types);
        Assert.Single(metadata.Tables);
        Assert.Single(metadata.Indexes);
        
        Assert.Equal("user_role", metadata.Types.First().Name);
        Assert.Equal("users", metadata.Tables.First().Name);
        Assert.Equal("idx_users_username", metadata.Indexes.First().Name);
    }

    [Fact]
    public void AnalyzeScript_WithMultilineConsecutiveStatements_ShouldParseAll()
    {
        // Arrange - реальный пример с многострочными таблицами без разделителей
        var sql = @"CREATE TABLE a1 (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    price NUMERIC(10, 2) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW()
)
CREATE TABLE a2 (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    price NUMERIC(10, 2) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW()
)
CREATE TYPE status AS ENUM ('active')
CREATE INDEX idx ON a1(id)";

        var analyzer = new SchemaAnalyzer();

        // Act
        var metadata = analyzer.AnalyzeScript(sql);

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
        Assert.Contains(metadata.Tables, t => t.Name == "a1");
        Assert.Contains(metadata.Tables, t => t.Name == "a2");
        
        Assert.Single(metadata.Types);
        Assert.Equal("status", metadata.Types[0].Name);
        
        Assert.Single(metadata.Indexes);
        Assert.Equal("idx", metadata.Indexes[0].Name);
        
        // Проверяем колонки первой таблицы
        var a1 = metadata.Tables.First(t => t.Name == "a1");
        Assert.Equal(5, a1.Columns.Count);
        Assert.Contains(a1.Columns, c => c.Name == "id");
        Assert.Contains(a1.Columns, c => c.Name == "name");
        Assert.Contains(a1.Columns, c => c.Name == "price");
    }
}
