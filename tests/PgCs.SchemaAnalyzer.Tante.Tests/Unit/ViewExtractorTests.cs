using PgCs.Core.Extraction.Block;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для ViewExtractor
/// </summary>
public sealed class ViewExtractorTests
{
    private readonly IViewExtractor _extractor = new ViewExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidViewBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW my_view AS
            SELECT id, name FROM users;
        ");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithMaterializedView_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE MATERIALIZED VIEW my_mat_view AS
            SELECT * FROM orders;
        ");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id SERIAL PRIMARY KEY);");

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

    #region Extract Simple View Tests

    [Fact]
    public void Extract_SimpleView_ReturnsValidDefinition()
    {
        // Arrange
        // var block = CreateBlock(@"
        //     CREATE VIEW active_users AS
        //     SELECT id, username, email
        //     FROM users
        //     WHERE is_active = TRUE;
        // ");
        
        var bx = new BlockExtractor();
        var block = bx.Extract(@"
            CREATE VIEW active_users AS
            SELECT 
                    id,
                    username, 
                    email
            FROM users
            WHERE is_active = TRUE;
        ").First();

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("active_users", result.Name);
        Assert.Null(result.Schema);
        Assert.False(result.IsMaterialized);
        Assert.False(result.WithCheckOption);
        Assert.False(result.IsSecurityBarrier);
        Assert.Contains("SELECT", result.Query);
        Assert.Contains("FROM users", result.Query);
        
        Assert.Equal(3, result.Columns.Count);
    }

    [Fact]
    public void Extract_ViewWithSchema_ExtractsSchemaCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW app.user_summary AS
            SELECT id, username FROM users;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user_summary", result.Name);
        Assert.Equal("app", result.Schema);
    }

    [Fact]
    public void Extract_ViewWithComment_ExtractsCommentCorrectly()
    {
        // Arrange
        var block = CreateBlock(
            @"CREATE VIEW summary AS SELECT * FROM data;",
            "Summary of all data"
        );

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("summary", result.Name);
        Assert.Equal("Summary of all data", result.SqlComment);
    }

    #endregion

    #region Materialized View Tests

    [Fact]
    public void Extract_MaterializedView_SetsMaterializedFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE MATERIALIZED VIEW order_stats AS
            SELECT 
                user_id,
                COUNT(*) AS order_count,
                SUM(total) AS total_amount
            FROM orders
            GROUP BY user_id;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("order_stats", result.Name);
        Assert.True(result.IsMaterialized);
        Assert.Contains("SELECT", result.Query);
        Assert.Contains("GROUP BY", result.Query);
    }

    [Fact]
    public void Extract_MaterializedViewWithSchema_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE MATERIALIZED VIEW analytics.daily_stats AS
            SELECT date, count(*) FROM events GROUP BY date;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("daily_stats", result.Name);
        Assert.Equal("analytics", result.Schema);
        Assert.True(result.IsMaterialized);
    }

    #endregion

    #region OR REPLACE Tests

    [Fact]
    public void Extract_ViewWithOrReplace_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE OR REPLACE VIEW user_list AS
            SELECT id, name FROM users;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user_list", result.Name);
        Assert.Contains("SELECT id, name", result.Query);
    }

    #endregion

    #region WITH CHECK OPTION Tests

    [Fact]
    public void Extract_ViewWithCheckOption_SetsFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW active_products AS
            SELECT id, name, price
            FROM products
            WHERE is_active = TRUE
            WITH CHECK OPTION;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("active_products", result.Name);
        Assert.True(result.WithCheckOption);
        Assert.Contains("WHERE is_active = TRUE", result.Query);
        Assert.DoesNotContain("WITH CHECK OPTION", result.Query);
    }

    #endregion

    #region SECURITY BARRIER Tests

    [Fact]
    public void Extract_ViewWithSecurityBarrierTrue_SetsFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW secure_data WITH (security_barrier = true) AS
            SELECT id, data FROM sensitive_table;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("secure_data", result.Name);
        Assert.True(result.IsSecurityBarrier);
    }

    [Fact]
    public void Extract_ViewWithSecurityBarrierOn_SetsFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW secure_view WITH (security_barrier = on) AS
            SELECT * FROM users;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSecurityBarrier);
    }

    [Fact]
    public void Extract_ViewWithSecurityBarrierFalse_DoesNotSetFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW normal_view WITH (security_barrier = false) AS
            SELECT * FROM public_data;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSecurityBarrier);
    }

    #endregion

    #region Complex Query Tests

    [Fact]
    public void Extract_ViewWithComplexQuery_ExtractsQueryCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW user_order_summary AS
            SELECT
                u.id,
                u.username,
                u.email,
                COUNT(DISTINCT o.id) AS total_orders,
                COALESCE(SUM(o.total), 0) AS total_spent
            FROM users u
            LEFT JOIN orders o ON u.id = o.user_id
            WHERE u.is_active = TRUE
            GROUP BY u.id, u.username, u.email;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user_order_summary", result.Name);
        Assert.Contains("COUNT(DISTINCT o.id)", result.Query);
        Assert.Contains("LEFT JOIN", result.Query);
        Assert.Contains("GROUP BY", result.Query);
    }

    [Fact]
    public void Extract_ViewWithSubquery_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW high_value_customers AS
            SELECT id, username
            FROM (
                SELECT u.id, u.username, SUM(o.total) as total
                FROM users u
                JOIN orders o ON u.id = o.user_id
                GROUP BY u.id, u.username
            ) subq
            WHERE total > 1000;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("high_value_customers", result.Name);
        Assert.Contains("SELECT id, username", result.Query);
        Assert.Contains("FROM (", result.Query);
    }

    [Fact]
    public void Extract_ViewWithCTE_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW monthly_sales AS
            WITH monthly_totals AS (
                SELECT 
                    DATE_TRUNC('month', created_at) as month,
                    SUM(total) as total
                FROM orders
                GROUP BY DATE_TRUNC('month', created_at)
            )
            SELECT * FROM monthly_totals ORDER BY month;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("monthly_sales", result.Name);
        Assert.Contains("WITH monthly_totals AS", result.Query);
    }

    #endregion

    #region Column List Tests

    [Fact]
    public void Extract_ViewWithExplicitColumns_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW user_info (user_id, user_name, user_email) AS
            SELECT id, name, email FROM users;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user_info", result.Name);
        Assert.Contains("SELECT id, name, email", result.Query);
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_WithNullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_WithNonViewBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE test (id INT);");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_WithEmptyBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_ViewWithoutSelectQuery_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE VIEW incomplete_view");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_WithUppercaseSQL_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW USER_LIST AS
            SELECT ID, NAME FROM USERS;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USER_LIST", result.Name);
    }

    [Fact]
    public void Extract_WithMixedCase_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CrEaTe ViEw MyView As
            SeLeCt * FrOm data;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyView", result.Name);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_RealWorldViewFromExample_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE VIEW active_users_with_orders AS
            SELECT
                u.id,
                u.username,
                u.email,
                u.full_name,
                u.status,
                u.is_premium,
                u.balance,
                u.loyalty_points,
                COUNT(DISTINCT o.id) AS total_orders,
                COALESCE(SUM(o.total), 0) AS total_spent,
                COALESCE(AVG(o.total), 0) AS average_order_value,
                MAX(o.created_at) AS last_order_date,
                ARRAY_AGG(DISTINCT o.status ORDER BY o.status) FILTER (WHERE o.status IS NOT NULL) AS order_statuses,
                COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'pending') AS pending_orders,
                COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'delivered') AS completed_orders
            FROM users u
            LEFT JOIN orders o ON u.id = o.user_id AND o.cancelled_at IS NULL
            WHERE u.status = 'active' AND u.is_deleted = FALSE
            GROUP BY u.id, u.username, u.email, u.full_name, u.status, u.is_premium, u.balance, u.loyalty_points;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("active_users_with_orders", result.Name);
        Assert.False(result.IsMaterialized);
        Assert.Contains("COUNT(DISTINCT o.id)", result.Query);
        Assert.Contains("ARRAY_AGG", result.Query);
        Assert.Contains("GROUP BY", result.Query);
    }

    [Fact]
    public void Extract_RealWorldMaterializedViewFromExample_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE MATERIALIZED VIEW category_statistics AS
            SELECT
                c.id,
                c.name,
                c.slug,
                c.level,
                c.parent_id,
                COUNT(oi.id) AS total_items_sold,
                COALESCE(SUM(oi.quantity), 0) AS total_quantity,
                COALESCE(SUM(oi.total_price), 0) AS total_revenue,
                COALESCE(AVG(oi.unit_price), 0) AS avg_price,
                COALESCE(MIN(oi.unit_price), 0) AS min_price,
                COALESCE(MAX(oi.unit_price), 0) AS max_price,
                COUNT(DISTINCT oi.order_id) AS unique_orders,
                COUNT(DISTINCT o.user_id) AS unique_customers,
                MAX(o.created_at) AS last_order_date
            FROM categories c
            LEFT JOIN order_items oi ON c.id = oi.category_id
            LEFT JOIN orders o ON oi.order_id = o.id AND o.cancelled_at IS NULL
            WHERE c.is_active = TRUE
            GROUP BY c.id, c.name, c.slug, c.level, c.parent_id;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("category_statistics", result.Name);
        Assert.True(result.IsMaterialized);
        Assert.Contains("LEFT JOIN", result.Query);
        Assert.Contains("WHERE c.is_active = TRUE", result.Query);
    }

    #endregion

    #region Helper Methods

    private static SqlBlock CreateBlock(string sql, string? comment = null)
    {
        return new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            HeaderComment = comment,
            StartLine = 1,
            EndLine = sql.Split('\n').Length
        };
    }

    #endregion
}
