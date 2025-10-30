using PgCs.SchemaAnalyzer.Tante;
using PgCs.Core.Schema.Common;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Интеграционные тесты для SchemaAnalyzer с извлечением представлений
/// </summary>
public sealed class SchemaAnalyzerViewTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    #region ExtractViews Tests

    [Fact]
    public void ExtractViews_WithSimpleView_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
            CREATE VIEW active_users AS
            SELECT id, username, email
            FROM users
            WHERE is_active = true;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("active_users", view.Name);
        Assert.Null(view.Schema);
        Assert.False(view.IsMaterialized);
        Assert.Contains("SELECT id, username, email", view.Query);
    }

    [Fact]
    public void ExtractViews_WithMultipleViews_ExtractsAll()
    {
        // Arrange
        var sql = @"
            CREATE VIEW user_summary AS
            SELECT id, username FROM users;

            CREATE VIEW order_summary AS
            SELECT id, order_number FROM orders;

            CREATE VIEW product_summary AS
            SELECT id, name FROM products;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Equal(3, views.Count);
        Assert.Contains(views, v => v.Name == "user_summary");
        Assert.Contains(views, v => v.Name == "order_summary");
        Assert.Contains(views, v => v.Name == "product_summary");
    }

    [Fact]
    public void ExtractViews_WithMaterializedView_SetsMaterializedFlag()
    {
        // Arrange
        var sql = @"
            CREATE MATERIALIZED VIEW sales_report AS
            SELECT 
                product_id,
                SUM(quantity) as total_sold,
                SUM(total_price) as total_revenue
            FROM orders
            GROUP BY product_id;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("sales_report", view.Name);
        Assert.True(view.IsMaterialized);
    }

    [Fact]
    public void ExtractViews_WithViewAndTable_ExtractsOnlyView()
    {
        // Arrange
        var sql = @"
            CREATE TABLE products (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100)
            );

            CREATE VIEW product_list AS
            SELECT id, name FROM products;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        Assert.Equal("product_list", views[0].Name);
    }

    [Fact]
    public void ExtractViews_WithComplexQuery_ExtractsQueryCorrectly()
    {
        // Arrange
        var sql = @"
            CREATE VIEW order_details AS
            SELECT 
                o.id,
                o.order_number,
                u.username,
                COUNT(oi.id) as item_count
            FROM orders o
            INNER JOIN users u ON o.user_id = u.id
            LEFT JOIN order_items oi ON o.id = oi.order_id
            GROUP BY o.id, o.order_number, u.username
            ORDER BY o.created_at DESC;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("order_details", view.Name);
        Assert.Contains("INNER JOIN", view.Query);
        Assert.Contains("LEFT JOIN", view.Query);
        Assert.Contains("GROUP BY", view.Query);
    }

    [Fact]
    public void ExtractViews_WithCheckOption_SetsFlag()
    {
        // Arrange
        var sql = @"
            CREATE VIEW updatable_users AS
            SELECT id, username, email
            FROM users
            WHERE is_active = true
            WITH CHECK OPTION;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("updatable_users", view.Name);
        Assert.True(view.WithCheckOption);
        Assert.DoesNotContain("WITH CHECK OPTION", view.Query);
    }

    [Fact]
    public void ExtractViews_WithSecurityBarrier_SetsFlag()
    {
        // Arrange
        var sql = @"
            CREATE VIEW secure_users WITH (security_barrier = true) AS
            SELECT id, username
            FROM users
            WHERE department_id = current_setting('app.current_department_id')::int;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("secure_users", view.Name);
        Assert.True(view.IsSecurityBarrier);
    }

    [Fact]
    public void ExtractViews_WithSchemaQualified_ExtractsSchema()
    {
        // Arrange
        var sql = @"
            CREATE VIEW analytics.user_metrics AS
            SELECT 
                user_id,
                COUNT(*) as activity_count,
                MAX(last_login) as last_seen
            FROM analytics.user_activity
            GROUP BY user_id;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("user_metrics", view.Name);
        Assert.Equal("analytics", view.Schema);
    }

    [Fact]
    public void ExtractViews_WithEmptyScript_ThrowsException()
    {
        // Arrange
        var sql = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractViews(sql));
    }

    [Fact]
    public void ExtractViews_WithOnlyComments_ReturnsEmptyList()
    {
        // Arrange
        var sql = @"
            -- This is just a comment
            /* 
             * Multi-line comment
             * No actual views here
             */
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Empty(views);
    }

    [Fact]
    public void ExtractViews_WithNullScript_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractViews(null!));
    }

    [Fact]
    public void ExtractViews_RealWorldViewFromExample_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
            -- Real-world example from Schema.sql
            CREATE VIEW active_users_with_orders AS
            SELECT 
                u.id,
                u.username,
                u.email,
                COUNT(o.id) as order_count,
                MAX(o.created_at) as last_order_date
            FROM users u
            INNER JOIN orders o ON u.id = o.user_id
            WHERE u.is_active = true
            GROUP BY u.id, u.username, u.email
            HAVING COUNT(o.id) > 0;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("active_users_with_orders", view.Name);
        Assert.False(view.IsMaterialized);
        Assert.Contains("INNER JOIN orders", view.Query);
        Assert.Contains("HAVING COUNT(o.id) > 0", view.Query);
    }

    [Fact]
    public void ExtractViews_RealWorldMaterializedViewFromExample_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
            -- Real-world materialized view
            CREATE MATERIALIZED VIEW category_statistics AS
            SELECT 
                c.id as category_id,
                c.name as category_name,
                COUNT(p.id) as product_count,
                AVG(oi.quantity * oi.unit_price) as avg_order_value,
                SUM(oi.quantity) as total_items_sold
            FROM categories c
            LEFT JOIN products p ON c.id = p.category_id
            LEFT JOIN order_items oi ON p.id = oi.product_id
            GROUP BY c.id, c.name;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Single(views);
        var view = views[0];
        Assert.Equal("category_statistics", view.Name);
        Assert.True(view.IsMaterialized);
        Assert.Contains("LEFT JOIN products", view.Query);
        Assert.Contains("AVG(oi.quantity * oi.unit_price)", view.Query);
    }

    [Fact]
    public void ExtractViews_WithMixedObjects_ExtractsOnlyViews()
    {
        // Arrange
        var sql = @"
            CREATE TYPE status_type AS ENUM ('pending', 'active', 'completed');

            CREATE TABLE orders (
                id SERIAL PRIMARY KEY,
                status status_type
            );

            CREATE VIEW active_orders AS
            SELECT id, status FROM orders WHERE status = 'active';

            CREATE MATERIALIZED VIEW order_stats AS
            SELECT status, COUNT(*) as count FROM orders GROUP BY status;

            CREATE FUNCTION get_order_count() RETURNS bigint AS $$
                SELECT COUNT(*) FROM orders;
            $$ LANGUAGE sql;
        ";

        // Act
        var views = _analyzer.ExtractViews(sql);

        // Assert
        Assert.Equal(2, views.Count);
        Assert.Contains(views, v => v.Name == "active_orders" && !v.IsMaterialized);
        Assert.Contains(views, v => v.Name == "order_stats" && v.IsMaterialized);
    }

    #endregion

    #region AnalyzeFileAsync Tests

    [Fact]
    public async Task AnalyzeFileAsync_WithViewsInFile_ExtractsViews()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var sql = @"
            CREATE VIEW test_view AS
            SELECT id, name FROM test_table;

            CREATE MATERIALIZED VIEW test_mat_view AS
            SELECT COUNT(*) as total FROM test_table;
        ";
        await File.WriteAllTextAsync(tempFile, sql);

        try
        {
            // Act
            var metadata = await _analyzer.AnalyzeFileAsync(tempFile);

            // Assert
            Assert.Equal(2, metadata.Views.Count);
            Assert.Contains(metadata.Views, v => v.Name == "test_view" && !v.IsMaterialized);
            Assert.Contains(metadata.Views, v => v.Name == "test_mat_view" && v.IsMaterialized);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion
}
