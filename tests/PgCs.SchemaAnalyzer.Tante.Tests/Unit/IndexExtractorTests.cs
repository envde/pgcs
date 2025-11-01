using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для IndexExtractor
/// </summary>
public sealed class IndexExtractorTests
{
    private readonly IIndexExtractor _extractor = new IndexExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidIndexBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("CREATE INDEX idx_users_email ON users (email);");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithUniqueIndex_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("CREATE UNIQUE INDEX idx_users_email ON users (email);");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id INTEGER);");

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

    #region Extract Simple Index Tests

    [Fact]
    public void Extract_SimpleIndex_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_users_email ON users (email);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_email", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Single(result.Columns);
        Assert.Equal("email", result.Columns[0]);
        Assert.False(result.IsUnique);
        Assert.Equal(IndexMethod.BTree, result.Method);
        Assert.Null(result.Schema);
    }

    [Fact]
    public void Extract_UniqueIndex_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE UNIQUE INDEX idx_users_email ON users (email);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_email", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.True(result.IsUnique);
        Assert.Single(result.Columns);
        Assert.Equal("email", result.Columns[0]);
    }

    [Fact]
    public void Extract_IndexWithMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_users_name_email ON users (last_name, first_name, email);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_name_email", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(3, result.Columns.Count);
        Assert.Equal("last_name", result.Columns[0]);
        Assert.Equal("first_name", result.Columns[1]);
        Assert.Equal("email", result.Columns[2]);
    }

    [Fact]
    public void Extract_IndexWithSchema_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_users_email ON public.users (email);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_email", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal("public", result.Schema);
    }

    #endregion

    #region Extract Index with USING Method Tests

    [Fact]
    public void Extract_IndexWithBTreeMethod_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_users_email ON users USING btree (email);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_email", result.Name);
        Assert.Equal(IndexMethod.BTree, result.Method);
    }

    [Fact]
    public void Extract_IndexWithHashMethod_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_users_email ON users USING hash (email);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(IndexMethod.Hash, result.Method);
    }

    [Fact]
    public void Extract_IndexWithGistMethod_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_locations ON locations USING gist (coordinates);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(IndexMethod.Gist, result.Method);
    }

    [Fact]
    public void Extract_IndexWithGinMethod_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_tags ON posts USING gin (tags);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(IndexMethod.Gin, result.Method);
    }

    #endregion

    #region Extract Partial Index Tests

    [Fact]
    public void Extract_PartialIndex_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_active_users ON users (email) WHERE status = 'active';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_active_users", result.Name);
        Assert.True(result.IsPartial);
        Assert.NotNull(result.WhereClause);
        Assert.Equal("status = 'active'", result.WhereClause);
    }

    [Fact]
    public void Extract_PartialIndexWithComplexCondition_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_recent_orders ON orders (created_at) WHERE status = 'pending' AND amount > 100;";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_recent_orders", result.Name);
        Assert.True(result.IsPartial);
        Assert.NotNull(result.WhereClause);
        Assert.Equal("status = 'pending' AND amount > 100", result.WhereClause);
    }

    #endregion

    #region Extract Index with Expression Tests

    [Fact]
    public void Extract_IndexWithExpression_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_lower_email ON users (lower(email));";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_lower_email", result.Name);
        Assert.Single(result.Columns);
        Assert.Equal("lower(email)", result.Columns[0]);
    }

    [Fact]
    public void Extract_IndexWithMultipleExpressions_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_names ON users (lower(first_name), upper(last_name));";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Columns.Count);
        Assert.Equal("lower(first_name)", result.Columns[0]);
        Assert.Equal("upper(last_name)", result.Columns[1]);
    }

    #endregion

    #region Extract Index with INCLUDE Tests

    [Fact]
    public void Extract_IndexWithInclude_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_users_email ON users (email) INCLUDE (first_name, last_name);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_email", result.Name);
        Assert.Single(result.Columns);
        Assert.Equal("email", result.Columns[0]);
        Assert.NotNull(result.IncludeColumns);
        Assert.Equal(2, result.IncludeColumns.Count);
        Assert.Equal("first_name", result.IncludeColumns[0]);
        Assert.Equal("last_name", result.IncludeColumns[1]);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_UppercaseKeywords_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX IDX_USERS_EMAIL ON USERS (EMAIL);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("IDX_USERS_EMAIL", result.Name);
        Assert.Equal("USERS", result.TableName);
        Assert.Single(result.Columns);
        Assert.Equal("EMAIL", result.Columns[0]);
    }

    [Fact]
    public void Extract_MixedCase_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "Create Index IdxUsersEmail On Users (Email);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("IdxUsersEmail", result.Name);
        Assert.Equal("Users", result.TableName);
        Assert.Single(result.Columns);
        Assert.Equal("Email", result.Columns[0]);
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_InvalidSql_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id INTEGER);");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_NullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_EmptyBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_IndexWithoutColumns_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE INDEX idx_test ON users ();");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_RealWorldComplexIndex_ReturnsValidDefinition()
    {
        // Arrange
        var sql = @"CREATE UNIQUE INDEX idx_users_active_email 
                    ON public.users USING btree (email) 
                    WHERE status = 'active';";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_active_email", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal("public", result.Schema);
        Assert.True(result.IsUnique);
        Assert.Equal(IndexMethod.BTree, result.Method);
        Assert.True(result.IsPartial);
        Assert.Equal("status = 'active'", result.WhereClause);
    }

    [Fact]
    public void Extract_RealWorldExpressionIndex_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_users_lower_email ON users (lower(email)) WHERE email IS NOT NULL;";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_users_lower_email", result.Name);
        Assert.Single(result.Columns);
        Assert.Equal("lower(email)", result.Columns[0]);
        Assert.True(result.IsPartial);
        Assert.Equal("email IS NOT NULL", result.WhereClause);
    }

    [Fact]
    public void Extract_RealWorldGinIndex_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE INDEX idx_posts_search ON posts USING gin (to_tsvector('english', title || ' ' || body));";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("idx_posts_search", result.Name);
        Assert.Equal("posts", result.TableName);
        Assert.Equal(IndexMethod.Gin, result.Method);
        Assert.Single(result.Columns);
        Assert.Contains("to_tsvector", result.Columns[0]);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Создает SqlBlock для тестирования
    /// </summary>
    private static SqlBlock CreateBlock(string content, string? headerComment = null)
    {
        return new SqlBlock
        {
            Content = content,
            RawContent = content,
            HeaderComment = headerComment,
            StartLine = 1,
            EndLine = 1
        };
    }

    #endregion
}
