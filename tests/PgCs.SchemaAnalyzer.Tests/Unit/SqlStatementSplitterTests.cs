using PgCs.SchemaAnalyzer.Utils;

namespace PgCs.SchemaAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для SqlStatementSplitter - разделение SQL скрипта на statements
/// </summary>
public sealed class SqlStatementSplitterTests
{
    #region Semicolon Separator Tests

    [Fact]
    public void Split_WithSemicolonSeparator_ShouldSplitCorrectly()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT); CREATE TABLE orders (id INT);";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
    }

    [Fact]
    public void Split_WithSemicolonAndWhitespace_ShouldTrimStatements()
    {
        // Arrange
        var sql = "  CREATE TABLE users (id INT)  ;  \n  CREATE TABLE orders (id INT)  ;  ";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
    }

    [Fact]
    public void Split_WithSemicolonInString_ShouldNotSplitAtString()
    {
        // Arrange
        var sql = "CREATE TABLE users (name VARCHAR(100) DEFAULT 'John; Doe');";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Single(statements);
        Assert.Equal("CREATE TABLE users (name VARCHAR(100) DEFAULT 'John; Doe')", statements[0]);
    }

    #endregion

    #region Newline Separator Tests

    [Fact]
    public void Split_WithDoubleNewline_ShouldSplitCorrectly()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT)\n\nCREATE TABLE orders (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
    }

    [Fact]
    public void Split_WithDoubleNewlineAndWhitespace_ShouldSplitCorrectly()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT)\n  \n  \nCREATE TABLE orders (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
    }

    [Fact]
    public void Split_WithSingleNewline_ShouldNotSplit()
    {
        // Arrange
        var sql = "CREATE TABLE users (\n  id INT,\n  name VARCHAR(100)\n)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Single(statements);
        Assert.Contains("CREATE TABLE users", statements[0]);
    }

    [Fact]
    public void Split_WithMultipleNewlines_ShouldSplitOnlyOnce()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT)\n\n\n\nCREATE TABLE orders (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
    }

    [Fact]
    public void Split_WithWindowsLineEndings_ShouldSplitCorrectly()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT)\r\n\r\nCREATE TABLE orders (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
    }

    #endregion

    #region Mixed Separators Tests

    [Fact]
    public void Split_WithBothSemicolonAndNewline_ShouldSplitOnBoth()
    {
        // Arrange
        var sql = @"CREATE TABLE users (id INT);

CREATE TABLE orders (id INT)

CREATE TABLE products (id INT);";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(3, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
        Assert.Equal("CREATE TABLE products (id INT)", statements[2]);
    }

    [Fact]
    public void Split_WithNoSeparator_ShouldReturnSingleStatement()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Single(statements);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
    }

    #endregion

    #region Dollar Quote Tests

    [Fact]
    public void Split_WithDollarQuoteContainingNewlines_ShouldNotSplitInsideQuote()
    {
        // Arrange
        var sql = @"CREATE FUNCTION test()
RETURNS TEXT
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN 'Hello

World';
END;
$$

CREATE TABLE users (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("CREATE FUNCTION test()", statements[0]);
        Assert.Contains("Hello\n\nWorld", statements[0]);
        Assert.Equal("CREATE TABLE users (id INT)", statements[1]);
    }

    [Fact]
    public void Split_WithDollarQuoteContainingSemicolon_ShouldNotSplitInsideQuote()
    {
        // Arrange
        var sql = @"CREATE FUNCTION test() RETURNS void AS $$
BEGIN
    EXECUTE 'SELECT 1; SELECT 2;';
END;
$$ LANGUAGE plpgsql;";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Single(statements);
        Assert.Contains("SELECT 1; SELECT 2;", statements[0]);
    }

    [Fact]
    public void Split_WithTaggedDollarQuote_ShouldHandleCorrectly()
    {
        // Arrange
        var sql = @"CREATE FUNCTION test() RETURNS void AS $body$
BEGIN
    RETURN 'test';
END;
$body$ LANGUAGE plpgsql

CREATE TABLE users (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("$body$", statements[0]);
        Assert.Equal("CREATE TABLE users (id INT)", statements[1]);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Split_WithEmptyString_ShouldReturnEmptyList()
    {
        // Arrange
        var sql = "";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Empty(statements);
    }

    [Fact]
    public void Split_WithWhitespaceOnly_ShouldReturnEmptyList()
    {
        // Arrange
        var sql = "   \n\n  \t  \n  ";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Empty(statements);
    }

    [Fact]
    public void Split_WithMultipleSemicolons_ShouldIgnoreEmpty()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT);;;CREATE TABLE orders (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
    }

    [Fact]
    public void Split_WithCommentsBetweenStatements_ShouldPreserveComments()
    {
        // Arrange
        var sql = @"CREATE TABLE users (id INT)
-- This is a comment

CREATE TABLE orders (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("-- This is a comment", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Split_WithComplexSchema_ShouldSplitCorrectly()
    {
        // Arrange
        var sql = @"CREATE TYPE user_status AS ENUM ('active', 'inactive')

CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    status user_status NOT NULL DEFAULT 'active'
)

CREATE INDEX idx_users_username ON users(username)

CREATE FUNCTION get_active_users()
RETURNS SETOF users
LANGUAGE sql
AS $$
    SELECT * FROM users WHERE status = 'active'
$$";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(4, statements.Count);
        Assert.Contains("CREATE TYPE user_status", statements[0]);
        Assert.Contains("CREATE TABLE users", statements[1]);
        Assert.Contains("CREATE INDEX idx_users_username", statements[2]);
        Assert.Contains("CREATE FUNCTION get_active_users()", statements[3]);
    }

    [Fact]
    public void Split_WithMixedStyleSchema_ShouldHandleBothStyles()
    {
        // Arrange
        var sql = @"CREATE TABLE users (id INT);
CREATE TABLE orders (id INT)

CREATE TABLE products (id INT);

CREATE TABLE categories (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(4, statements.Count);
        Assert.Equal("CREATE TABLE users (id INT)", statements[0]);
        Assert.Equal("CREATE TABLE orders (id INT)", statements[1]);
        Assert.Equal("CREATE TABLE products (id INT)", statements[2]);
        Assert.Equal("CREATE TABLE categories (id INT)", statements[3]);
    }

    #endregion

    #region Statement Start Detection Tests

    [Fact]
    public void Split_WithConsecutiveStatements_ShouldSplitByKeyword()
    {
        // Arrange - statements идут один за другим без пустых строк
        var sql = @"CREATE TABLE a1 (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255)
)
CREATE TABLE a2 (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255)
)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("CREATE TABLE a1", statements[0]);
        Assert.Contains("CREATE TABLE a2", statements[1]);
    }

    [Fact]
    public void Split_WithMultipleConsecutiveStatements_ShouldSplitAll()
    {
        // Arrange
        var sql = @"CREATE TYPE user_role AS ENUM ('admin', 'user')
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    role user_role NOT NULL
)
CREATE INDEX idx_users_username ON users(username)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(3, statements.Count);
        Assert.Contains("CREATE TYPE user_role", statements[0]);
        Assert.Contains("CREATE TABLE users", statements[1]);
        Assert.Contains("CREATE INDEX idx_users_username", statements[2]);
    }

    [Fact]
    public void Split_WithMixedSeparators_ShouldHandleAll()
    {
        // Arrange - mix of ;, newlines, and consecutive statements
        var sql = @"CREATE TABLE t1 (id INT);
CREATE TABLE t2 (id INT)
CREATE TABLE t3 (id INT)

CREATE TABLE t4 (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(4, statements.Count);
        Assert.All(statements, s => Assert.Contains("CREATE TABLE", s));
    }

    [Fact]
    public void Split_WithDifferentKeywords_ShouldSplitAll()
    {
        // Arrange
        var sql = @"CREATE TABLE users (id INT)
ALTER TABLE users ADD COLUMN email VARCHAR(255)
DROP TABLE old_table
COMMENT ON TABLE users IS 'User table'";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(4, statements.Count);
        Assert.Contains("CREATE TABLE", statements[0]);
        Assert.Contains("ALTER TABLE", statements[1]);
        Assert.Contains("DROP TABLE", statements[2]);
        Assert.Contains("COMMENT ON", statements[3]);
    }

    [Fact]
    public void Split_WithCreateInsideString_ShouldNotSplit()
    {
        // Arrange - CREATE внутри строки не должно разделять
        var sql = @"CREATE TABLE users (
    name VARCHAR(100) DEFAULT 'CREATE USER'
)
CREATE TABLE orders (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("'CREATE USER'", statements[0]);
    }

    [Fact]
    public void Split_WithCreateInsideDollarQuote_ShouldNotSplit()
    {
        // Arrange
        var sql = @"CREATE FUNCTION test() RETURNS void AS $$
BEGIN
    EXECUTE 'CREATE TABLE temp (id INT)';
END;
$$
CREATE TABLE users (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("CREATE FUNCTION", statements[0]);
        Assert.Contains("'CREATE TABLE temp", statements[0]);
        Assert.Contains("CREATE TABLE users", statements[1]);
    }

    [Fact]
    public void Split_WithCommentBeforeKeyword_ShouldIncludeInStatement()
    {
        // Arrange
        var sql = @"CREATE TABLE users (id INT)
-- This is a comment
CREATE TABLE orders (id INT)";

        // Act
        var statements = SqlStatementSplitter.Split(sql);

        // Assert
        Assert.Equal(2, statements.Count);
        Assert.Contains("CREATE TABLE users", statements[0]);
        Assert.Contains("-- This is a comment", statements[0]);
        Assert.Contains("CREATE TABLE orders", statements[1]);
    }

    #endregion
}
