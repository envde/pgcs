using PgCs.Core.Schema.Common;
using PgCs.SchemaAnalyzer.Tante;

namespace PgCs.SchemaAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для SchemaDefinitionDetector - детектора типов объектов схемы
/// </summary>
public sealed class SchemaDefinitionDetectorTests
{
    [Theory]
    [InlineData("CREATE TYPE user_status AS ENUM ('active', 'inactive');", SchemaObjectType.Types)]
    [InlineData("CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100));", SchemaObjectType.Types)]
    [InlineData("CREATE DOMAIN email AS VARCHAR(255) CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@');", SchemaObjectType.Types)]
    public void DetectObjectType_CreateType_ReturnsTypes(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("CREATE TABLE users (id SERIAL PRIMARY KEY);", SchemaObjectType.Tables)]
    [InlineData("CREATE TABLE orders (id BIGSERIAL, user_id INTEGER);", SchemaObjectType.Tables)]
    public void DetectObjectType_CreateTable_ReturnsTables(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("CREATE INDEX idx_users_email ON users (email);", SchemaObjectType.Indexes)]
    [InlineData("CREATE UNIQUE INDEX idx_users_username ON users (username);", SchemaObjectType.Indexes)]
    [InlineData("CREATE INDEX idx_users_preferences ON users USING GIN (preferences);", SchemaObjectType.Indexes)]
    public void DetectObjectType_CreateIndex_ReturnsIndexes(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("CREATE VIEW active_users AS SELECT * FROM users WHERE status = 'active';", SchemaObjectType.Views)]
    [InlineData("CREATE OR REPLACE VIEW user_stats AS SELECT COUNT(*) FROM users;", SchemaObjectType.Views)]
    [InlineData("CREATE MATERIALIZED VIEW category_stats AS SELECT * FROM categories;", SchemaObjectType.Views)]
    public void DetectObjectType_CreateView_ReturnsViews(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("CREATE FUNCTION update_timestamp() RETURNS TRIGGER AS $$ BEGIN RETURN NEW; END; $$ LANGUAGE plpgsql;", SchemaObjectType.Functions)]
    [InlineData("CREATE OR REPLACE FUNCTION get_user(user_id INT) RETURNS TEXT AS $$ BEGIN RETURN 'test'; END; $$ LANGUAGE plpgsql;", SchemaObjectType.Functions)]
    [InlineData("CREATE PROCEDURE update_stats() AS $$ BEGIN UPDATE stats SET count = count + 1; END; $$ LANGUAGE plpgsql;", SchemaObjectType.Functions)]
    public void DetectObjectType_CreateFunction_ReturnsFunctions(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("CREATE TRIGGER update_timestamp BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION update_timestamp();", SchemaObjectType.Triggers)]
    [InlineData("CREATE TRIGGER validate_email BEFORE INSERT ON users FOR EACH ROW EXECUTE FUNCTION validate_email();", SchemaObjectType.Triggers)]
    public void DetectObjectType_CreateTrigger_ReturnsTriggers(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("ALTER TABLE orders ADD CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id);", SchemaObjectType.Constraints)]
    [InlineData("ALTER TABLE products ADD CONSTRAINT check_price CHECK (price >= 0);", SchemaObjectType.Constraints)]
    public void DetectObjectType_AlterTableAddConstraint_ReturnsConstraints(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("COMMENT ON TYPE user_status IS 'Possible user statuses';", SchemaObjectType.Comments)]
    [InlineData("COMMENT ON TABLE users IS 'Main users table';", SchemaObjectType.Comments)]
    [InlineData("COMMENT ON COLUMN users.email IS 'User email address';", SchemaObjectType.Comments)]
    [InlineData("COMMENT ON INDEX idx_users_email IS 'Index for email lookup';", SchemaObjectType.Comments)]
    [InlineData("COMMENT ON VIEW active_users IS 'Active users view';", SchemaObjectType.Comments)]
    [InlineData("COMMENT ON FUNCTION update_timestamp() IS 'Updates timestamp';", SchemaObjectType.Comments)]
    [InlineData("COMMENT ON TRIGGER update_timestamp ON users IS 'Auto-update trigger';", SchemaObjectType.Comments)]
    public void DetectObjectType_CommentOn_ReturnsComments(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("SELECT * FROM users;", SchemaObjectType.None)]
    [InlineData("INSERT INTO users (username) VALUES ('test');", SchemaObjectType.None)]
    [InlineData("UPDATE users SET status = 'active';", SchemaObjectType.None)]
    [InlineData("DELETE FROM users WHERE id = 1;", SchemaObjectType.None)]
    public void DetectObjectType_DmlStatements_ReturnsNone(string sql, SchemaObjectType expected)
    {
        // Act
        var result = SchemaDefinitionDetector.DetectObjectType(sql);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DetectObjectType_EmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SchemaDefinitionDetector.DetectObjectType(""));
    }

    [Fact]
    public void DetectObjectType_WhitespaceOnly_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SchemaDefinitionDetector.DetectObjectType("   \n\t  "));
    }

    [Theory]
    [InlineData("COMMENT ON TYPE user_status IS 'test';", "TYPE")]
    [InlineData("COMMENT ON DOMAIN email IS 'test';", "DOMAIN")]
    [InlineData("COMMENT ON TABLE users IS 'test';", "TABLE")]
    [InlineData("COMMENT ON COLUMN users.email IS 'test';", "COLUMN")]
    [InlineData("COMMENT ON INDEX idx_users_email IS 'test';", "INDEX")]
    [InlineData("COMMENT ON VIEW active_users IS 'test';", "VIEW")]
    [InlineData("COMMENT ON FUNCTION update_timestamp() IS 'test';", "FUNCTION")]
    [InlineData("COMMENT ON TRIGGER update_timestamp ON users IS 'test';", "TRIGGER")]
    [InlineData("COMMENT ON CONSTRAINT fk_user ON orders IS 'test';", "CONSTRAINT")]
    public void ExtractCommentOnObjectType_ReturnsCorrectType(string sql, string expectedType)
    {
        // Act
        var result = SchemaDefinitionDetector.ExtractCommentOnObjectType(sql);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Fact]
    public void ExtractCommentOnObjectType_NotCommentOn_ReturnsNull()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT);";

        // Act
        var result = SchemaDefinitionDetector.ExtractCommentOnObjectType(sql);

        // Assert
        Assert.Null(result);
    }
}
