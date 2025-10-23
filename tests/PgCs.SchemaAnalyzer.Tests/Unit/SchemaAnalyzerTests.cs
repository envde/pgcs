using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;

namespace PgCs.SchemaAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для SchemaAnalyzer - основного класса анализа схемы базы данных
/// </summary>
public sealed class SchemaAnalyzerTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    #region AnalyzeScript Tests

    [Fact]
    public void AnalyzeScript_WithValidSchema_ReturnsCompleteMetadata()
    {
        // Arrange
        const string sql = @"
            CREATE TYPE user_status AS ENUM ('active', 'inactive');
            
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(50) NOT NULL,
                status user_status DEFAULT 'active'
            );
            
            CREATE INDEX idx_users_username ON users(username);
        ";

        // Act
        var metadata = _analyzer.AnalyzeScript(sql);

        // Assert
        Assert.NotNull(metadata);
        Assert.NotEmpty(metadata.Tables);
        Assert.NotEmpty(metadata.Types);
        Assert.NotEmpty(metadata.Indexes);
    }

    [Fact]
    public void AnalyzeScript_WithEmptyString_ReturnsEmptyMetadata()
    {
        // Arrange
        const string sql = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _analyzer.AnalyzeScript(sql));
    }

    [Fact]
    public void AnalyzeScript_WithNullString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _analyzer.AnalyzeScript(null!));
    }

    [Fact]
    public void AnalyzeScript_WithComplexSchema_ExtractsAllElements()
    {
        // Arrange
        const string sql = @"
            CREATE TYPE order_status AS ENUM ('pending', 'completed');
            
            CREATE TABLE orders (
                id BIGSERIAL PRIMARY KEY,
                status order_status NOT NULL,
                total NUMERIC(10, 2) NOT NULL,
                created_at TIMESTAMP NOT NULL DEFAULT NOW()
            );
            
            CREATE TABLE order_items (
                id BIGSERIAL PRIMARY KEY,
                order_id BIGINT NOT NULL,
                quantity INTEGER NOT NULL,
                CONSTRAINT fk_order FOREIGN KEY (order_id) REFERENCES orders(id)
            );
            
            CREATE INDEX idx_orders_status ON orders(status);
            CREATE INDEX idx_order_items_order_id ON order_items(order_id);
            
            CREATE OR REPLACE FUNCTION get_order_total(order_id BIGINT)
            RETURNS NUMERIC AS $$
            BEGIN
                RETURN 0.0;
            END;
            $$ LANGUAGE plpgsql;
            
            CREATE TRIGGER update_timestamp
                BEFORE UPDATE ON orders
                FOR EACH ROW
                EXECUTE FUNCTION update_modified_column();
        ";

        // Act
        var metadata = _analyzer.AnalyzeScript(sql);

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
        Assert.Single(metadata.Types);
        Assert.Equal(2, metadata.Indexes.Count);
        Assert.Single(metadata.Functions);
        Assert.Single(metadata.Triggers);
        Assert.Single(metadata.Constraints);
    }

    [Fact]
    public void AnalyzeScript_SetsAnalyzedAtTimestamp()
    {
        // Arrange
        const string sql = "CREATE TABLE test (id INT);";
        var beforeAnalysis = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var metadata = _analyzer.AnalyzeScript(sql);
        var afterAnalysis = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.InRange(metadata.AnalyzedAt, beforeAnalysis, afterAnalysis);
    }

    #endregion

    #region ExtractTables Tests

    [Fact]
    public void ExtractTables_WithSimpleTable_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE products (
                id SERIAL PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                price NUMERIC(10, 2)
            );
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        var table = tables[0];
        Assert.Equal("products", table.Name);
        Assert.Equal(3, table.Columns.Count);
    }

    [Fact]
    public void ExtractTables_WithMultipleTables_ExtractsAll()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE table1 (id INT);
            CREATE TABLE table2 (id INT);
            CREATE TABLE table3 (id INT);
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Equal(3, tables.Count);
        Assert.Contains(tables, t => t.Name == "table1");
        Assert.Contains(tables, t => t.Name == "table2");
        Assert.Contains(tables, t => t.Name == "table3");
    }

    [Fact]
    public void ExtractTables_WithSchemaPrefix_ExtractsSchema()
    {
        // Arrange
        const string sql = "CREATE TABLE public.users (id INT);";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        Assert.Equal("public", tables[0].Schema);
        Assert.Equal("users", tables[0].Name);
    }

    [Fact]
    public void ExtractTables_WithNoTables_ReturnsEmptyList()
    {
        // Arrange
        const string sql = "CREATE TYPE status AS ENUM ('active');";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Empty(tables);
    }

    [Fact]
    public void ExtractTables_WithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractTables(null!));
    }

    #endregion

    #region ExtractTypes Tests

    [Fact]
    public void ExtractTypes_WithEnumType_ExtractsCorrectly()
    {
        // Arrange
        const string sql = "CREATE TYPE status AS ENUM ('active', 'inactive', 'pending');";

        // Act
        var types = _analyzer.ExtractTypes(sql);

        // Assert
        Assert.Single(types);
        var type = types[0];
        Assert.Equal("status", type.Name);
        Assert.Equal(TypeKind.Enum, type.Kind);
        Assert.Equal(3, type.EnumValues?.Count);
    }

    [Fact]
    public void ExtractTypes_WithMultipleTypes_ExtractsAll()
    {
        // Arrange
        const string sql = @"
            CREATE TYPE status AS ENUM ('active', 'inactive');
            CREATE TYPE priority AS ENUM ('low', 'medium', 'high');
        ";

        // Act
        var types = _analyzer.ExtractTypes(sql);

        // Assert
        Assert.Equal(2, types.Count);
        Assert.Contains(types, t => t.Name == "status");
        Assert.Contains(types, t => t.Name == "priority");
    }

    [Fact]
    public void ExtractTypes_WithCompositeType_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE TYPE address AS (
                street VARCHAR(255),
                city VARCHAR(100),
                zip_code VARCHAR(20)
            );
        ";

        // Act
        var types = _analyzer.ExtractTypes(sql);

        // Assert
        Assert.Single(types);
        var type = types[0];
        Assert.Equal("address", type.Name);
        Assert.Equal(TypeKind.Composite, type.Kind);
        Assert.NotNull(type.CompositeAttributes);
        Assert.Equal(3, type.CompositeAttributes.Count);
    }

    [Fact]
    public void ExtractTypes_WithDomainType_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE DOMAIN email AS VARCHAR(255)
                CHECK (VALUE ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');
        ";

        // Act
        var types = _analyzer.ExtractTypes(sql);

        // Assert
        Assert.Single(types);
        var type = types[0];
        Assert.Equal("email", type.Name);
        Assert.Equal(TypeKind.Domain, type.Kind);
    }

    [Fact]
    public void ExtractTypes_WithNoTypes_ReturnsEmptyList()
    {
        // Arrange
        const string sql = "CREATE TABLE test (id INT);";

        // Act
        var types = _analyzer.ExtractTypes(sql);

        // Assert
        Assert.Empty(types);
    }

    #endregion

    #region ExtractViews Tests

    [Fact]
    public void ExtractViews_WithSimpleView_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE VIEW active_users AS
            SELECT id, username, email
            FROM users
            WHERE status = 'active';
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("active_users", view.Name);
        Assert.False(view.IsMaterialized);
    }

    [Fact]
    public void ExtractViews_WithMaterializedView_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE MATERIALIZED VIEW user_statistics AS
            SELECT user_id, COUNT(*) as order_count
            FROM orders
            GROUP BY user_id;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("user_statistics", view.Name);
        Assert.True(view.IsMaterialized);
    }

    [Fact]
    public void ExtractViews_WithMultipleViews_ExtractsAll()
    {
        // Arrange
        const string sql = @"
            CREATE VIEW view1 AS SELECT 1;
            CREATE VIEW view2 AS SELECT 2;
            CREATE MATERIALIZED VIEW view3 AS SELECT 3;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Equal(3, views.Count);
        Assert.Equal(2, views.Count(v => !v.IsMaterialized));
        Assert.Single(views, v => v.IsMaterialized);
    }

    #endregion

    #region ExtractFunctions Tests

    [Fact]
    public void ExtractFunctions_WithSimpleFunction_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE FUNCTION get_user_count()
            RETURNS INTEGER AS $$
            BEGIN
                RETURN (SELECT COUNT(*) FROM users);
            END;
            $$ LANGUAGE plpgsql;
        ";

        // Act
        var functions = _analyzer.ExtractFunctions(sql);

        // Assert
        Assert.Single(functions);
        var function = functions[0];
        Assert.Equal("get_user_count", function.Name);
        Assert.NotNull(function.ReturnType);
    }

    [Fact]
    public void ExtractFunctions_WithParameters_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE FUNCTION calculate_total(
                price NUMERIC,
                quantity INTEGER,
                discount NUMERIC DEFAULT 0
            )
            RETURNS NUMERIC AS $$
            BEGIN
                RETURN price * quantity * (1 - discount / 100);
            END;
            $$ LANGUAGE plpgsql;
        ";

        // Act
        var functions = _analyzer.ExtractFunctions(sql);

        // Assert
        Assert.Single(functions);
        var function = functions[0];
        Assert.Equal("calculate_total", function.Name);
        Assert.Equal(3, function.Parameters.Count);
    }

    [Fact]
    public void ExtractFunctions_WithORReplace_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE OR REPLACE FUNCTION test_func()
            RETURNS VOID AS $$
            BEGIN
            END;
            $$ LANGUAGE plpgsql;
        ";

        // Act
        var functions = _analyzer.ExtractFunctions(sql);

        // Assert
        Assert.Single(functions);
        Assert.Equal("test_func", functions[0].Name);
    }

    #endregion

    #region ExtractIndexes Tests

    [Fact]
    public void ExtractIndexes_WithSimpleIndex_ExtractsCorrectly()
    {
        // Arrange
        const string sql = "CREATE INDEX idx_users_email ON users(email);";

        // Act
        var indexes = _analyzer.ExtractIndexes(sql);

        // Assert
        Assert.Single(indexes);
        var index = indexes[0];
        Assert.Equal("idx_users_email", index.Name);
        Assert.Equal("users", index.TableName);
        Assert.Single(index.Columns);
        Assert.Contains("email", index.Columns);
        Assert.False(index.IsUnique);
    }

    [Fact]
    public void ExtractIndexes_WithUniqueIndex_ExtractsCorrectly()
    {
        // Arrange
        const string sql = "CREATE UNIQUE INDEX idx_users_username ON users(username);";

        // Act
        var indexes = _analyzer.ExtractIndexes(sql);

        // Assert
        Assert.Single(indexes);
        Assert.True(indexes[0].IsUnique);
    }

    [Fact]
    public void ExtractIndexes_WithCompositeIndex_ExtractsAllColumns()
    {
        // Arrange
        const string sql = "CREATE INDEX idx_orders_user_date ON orders(user_id, created_at);";

        // Act
        var indexes = _analyzer.ExtractIndexes(sql);

        // Assert
        Assert.Single(indexes);
        var index = indexes[0];
        Assert.Equal(2, index.Columns.Count);
        Assert.Contains("user_id", index.Columns);
        Assert.Contains("created_at", index.Columns);
    }

    [Fact]
    public void ExtractIndexes_WithIndexMethod_ExtractsCorrectly()
    {
        // Arrange
        const string sql = "CREATE INDEX idx_data_jsonb ON table1 USING gin(data);";

        // Act
        var indexes = _analyzer.ExtractIndexes(sql);

        // Assert
        Assert.Single(indexes);
        Assert.Equal(IndexMethod.Gin, indexes[0].Method);
    }

    #endregion

    #region ExtractTriggers Tests

    [Fact]
    public void ExtractTriggers_WithSimpleTrigger_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            CREATE TRIGGER update_timestamp
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION update_modified_column();
        ";

        // Act
        var triggers = _analyzer.ExtractTriggers(sql);

        // Assert
        Assert.Single(triggers);
        var trigger = triggers[0];
        Assert.Equal("update_timestamp", trigger.Name);
        Assert.Equal("users", trigger.TableName);
        Assert.Equal("update_modified_column", trigger.FunctionName);
    }

    [Fact]
    public void ExtractTriggers_WithMultipleTriggers_ExtractsAll()
    {
        // Arrange
        const string sql = @"
            CREATE TRIGGER trigger1 AFTER INSERT ON table1 
                FOR EACH ROW EXECUTE FUNCTION func1();
            CREATE TRIGGER trigger2 BEFORE DELETE ON table2 
                FOR EACH ROW EXECUTE FUNCTION func2();
        ";

        // Act
        var triggers = _analyzer.ExtractTriggers(sql);

        // Assert
        Assert.Equal(2, triggers.Count);
        Assert.Contains(triggers, t => t.Name == "trigger1");
        Assert.Contains(triggers, t => t.Name == "trigger2");
    }

    #endregion

    #region ExtractConstraints Tests

    [Fact]
    public void ExtractConstraints_WithForeignKey_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            ALTER TABLE orders
            ADD CONSTRAINT fk_orders_user
            FOREIGN KEY (user_id) REFERENCES users(id);
        ";

        // Act
        var constraints = _analyzer.ExtractConstraints(sql);

        // Assert
        Assert.Single(constraints);
        var constraint = constraints[0];
        Assert.Equal("fk_orders_user", constraint.Name);
        Assert.Equal(ConstraintType.ForeignKey, constraint.Type);
        Assert.Equal("users", constraint.ReferencedTable);
    }

    [Fact]
    public void ExtractConstraints_WithUniqueConstraint_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            ALTER TABLE users
            ADD CONSTRAINT unique_email UNIQUE (email);
        ";

        // Act
        var constraints = _analyzer.ExtractConstraints(sql);

        // Assert
        Assert.Single(constraints);
        var constraint = constraints[0];
        Assert.Equal("unique_email", constraint.Name);
        Assert.Equal(ConstraintType.Unique, constraint.Type);
    }

    [Fact]
    public void ExtractConstraints_WithCheckConstraint_ExtractsCorrectly()
    {
        // Arrange
        const string sql = @"
            ALTER TABLE products
            ADD CONSTRAINT check_price CHECK (price > 0);
        ";

        // Act
        var constraints = _analyzer.ExtractConstraints(sql);

        // Assert
        Assert.Single(constraints);
        Assert.Equal(ConstraintType.Check, constraints[0].Type);
    }

    #endregion

    #region File and Directory Analysis Tests

    [Fact]
    public async Task AnalyzeFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        const string nonExistentPath = "/path/to/nonexistent/file.sql";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _analyzer.AnalyzeFileAsync(nonExistentPath).AsTask());
    }

    [Fact]
    public async Task AnalyzeFileAsync_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _analyzer.AnalyzeFileAsync(null!).AsTask());
    }

    [Fact]
    public async Task AnalyzeFileAsync_WithEmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _analyzer.AnalyzeFileAsync("").AsTask());
    }

    [Fact]
    public async Task AnalyzeDirectoryAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        const string nonExistentPath = "/path/to/nonexistent/directory";

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => _analyzer.AnalyzeDirectoryAsync(nonExistentPath).AsTask());
    }

    [Fact]
    public async Task AnalyzeDirectoryAsync_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _analyzer.AnalyzeDirectoryAsync(null!).AsTask());
    }

    #endregion
}
