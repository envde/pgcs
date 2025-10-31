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
        var result = _extractor.CanExtract([block]);

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
        var result = _extractor.CanExtract([block]);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id SERIAL PRIMARY KEY);");

        // Act
        var result = _extractor.CanExtract([block]);

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
        var bx = new BlockExtractor();
        var block = bx.Extract(@"
            CREATE VIEW active_users AS
            SELECT 
                    id, -- comment: Идентификатор пользователя; type: INTEGER; rename: ID;
                    username,  -- comment: Имя пользователя; type: VARCHAR(255);
                    email -- Почтовый адрес
            FROM users
            WHERE is_active = TRUE;
        ").First();

        // Act
        var result = _extractor.Extract([block]);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("active_users", result.Name);
        Assert.Null(result.Schema);
        Assert.False(result.IsMaterialized);
        Assert.False(result.WithCheckOption);
        Assert.False(result.IsSecurityBarrier);
        Assert.Contains("SELECT", result.Query);
        Assert.Contains("FROM users", result.Query);
        
        // Verify columns extracted from inline comments
        Assert.Equal(3, result.Columns.Count);
        
        // Column 1: id with full metadata
        var idColumn = result.Columns[0];
        Assert.Equal("id", idColumn.Name);
        Assert.Equal("INTEGER", idColumn.DataType);
        Assert.Equal("ID", idColumn.ReName);
        Assert.Equal("Идентификатор пользователя", idColumn.Comment);
        
        // Column 2: username with type
        var usernameColumn = result.Columns[1];
        Assert.Equal("username", usernameColumn.Name);
        Assert.Equal("VARCHAR(255)", usernameColumn.DataType);
        Assert.Null(usernameColumn.ReName);
        Assert.Equal("Имя пользователя", usernameColumn.Comment);
        
        // Column 3: email with a simple comment
        var emailColumn = result.Columns[2];
        Assert.Equal("email", emailColumn.Name);
        Assert.Equal("unknown", emailColumn.DataType); // No type specified
        Assert.Null(emailColumn.ReName);
        Assert.Equal("Почтовый адрес", emailColumn.Comment);
    }

    [Fact]
    public void Extract_ViewWithMultipleTablesInBlocks_ExtractsTypesFromTables()
    {
        // Arrange - создаем SQL с несколькими таблицами и VIEW
        var bx = new BlockExtractor();
        var allBlocks = bx.Extract(@"
            -- Таблица пользователей
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(100) NOT NULL,
                email VARCHAR(255) NOT NULL,
                created_at TIMESTAMP DEFAULT NOW()
            );

            -- Таблица заказов
            CREATE TABLE orders (
                id SERIAL PRIMARY KEY,
                user_id INTEGER REFERENCES users(id),
                order_number VARCHAR(50) NOT NULL,
                total DECIMAL(10, 2) NOT NULL,
                status VARCHAR(20) DEFAULT 'pending',
                created_at TIMESTAMP DEFAULT NOW()
            );

            -- VIEW объединяющий данные из обеих таблиц
            CREATE VIEW user_order_summary AS
            SELECT 
                u.id,          -- Идентификатор пользователя
                u.username,    -- Имя пользователя
                u.email,       -- Почтовый адрес
                o.order_number,-- Номер заказа
                o.total,       -- Сумма заказа в долларах
                o.status       -- Статус заказа
            FROM users u
            LEFT JOIN orders o ON u.id = o.user_id;
        ").ToList();

        // Проверяем, что получили 3 блока: users, orders, view
        Assert.Equal(3, allBlocks.Count);
        
        // Переупорядочиваем блоки: VIEW должен быть первым, затем таблицы
        // Это имитирует поведение SchemaAnalyzer, который передает блоки с помощью Skip(i).ToList()
        var viewBlock = allBlocks[2]; // VIEW - третий блок
        var tableBlocks = new[] { allBlocks[0], allBlocks[1] }; // users и orders
        var blocks = new[] { viewBlock }.Concat(tableBlocks).ToList();

        // Act - передаем блоки в ViewExtractor (VIEW первый, таблицы после)
        var result = _extractor.Extract(blocks);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("user_order_summary", result.Name);
        Assert.False(result.IsMaterialized);
        
        // Проверяем, что извлечено 6 колонок
        Assert.Equal(6, result.Columns.Count);
        
        // Проверяем id - должен быть SERIAL из users, с комментарием из VIEW
        var idColumn = result.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(idColumn);
        Assert.Equal("SERIAL", idColumn.DataType.ToUpper());
        Assert.Equal("Идентификатор пользователя", idColumn.Comment);
        
        // Проверяем username - должен быть VARCHAR(100)
        var usernameColumn = result.Columns.FirstOrDefault(c => c.Name == "username");
        Assert.NotNull(usernameColumn);
        Assert.Equal("VARCHAR", usernameColumn.DataType.ToUpper());
        Assert.Equal(100, usernameColumn.MaxLength);
        Assert.Equal("Имя пользователя", usernameColumn.Comment);
        
        // Проверяем email - должен быть VARCHAR(255)
        var emailColumn = result.Columns.FirstOrDefault(c => c.Name == "email");
        Assert.NotNull(emailColumn);
        Assert.Equal("VARCHAR", emailColumn.DataType.ToUpper());
        Assert.Equal(255, emailColumn.MaxLength);
        Assert.Equal("Почтовый адрес", emailColumn.Comment);
        
        // Проверяем order_number - должен быть VARCHAR(50)
        var orderNumberColumn = result.Columns.FirstOrDefault(c => c.Name == "order_number");
        Assert.NotNull(orderNumberColumn);
        Assert.Equal("VARCHAR", orderNumberColumn.DataType.ToUpper());
        Assert.Equal(50, orderNumberColumn.MaxLength);
        Assert.Equal("Номер заказа", orderNumberColumn.Comment);
        
        // Проверяем total - должен быть DECIMAL(10, 2)
        var totalColumn = result.Columns.FirstOrDefault(c => c.Name == "total");
        Assert.NotNull(totalColumn);
        Assert.Equal("DECIMAL", totalColumn.DataType.ToUpper());
        Assert.Equal(10, totalColumn.NumericPrecision);
        Assert.Equal(2, totalColumn.NumericScale);
        Assert.Equal("Сумма заказа в долларах", totalColumn.Comment);
        
        // Проверяем status - должен быть VARCHAR(20)
        var statusColumn = result.Columns.FirstOrDefault(c => c.Name == "status");
        Assert.NotNull(statusColumn);
        Assert.Equal("VARCHAR", statusColumn.DataType.ToUpper());
        Assert.Equal(20, statusColumn.MaxLength);
        Assert.Equal("Статус заказа", statusColumn.Comment);
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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        //TODO: Допиши этот тест чтобы в sql были необходимые таблицы (users, order) чтобы при создании view
        // можно было проверить извлекаются ли данные для колонок view из связанных (необходимых) таблиц.
        var sql = @"
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
        ";
        
        var blockExtractor = new BlockExtractor();
        var blocks = blockExtractor.Extract(sql);

        // Act
        var result = _extractor.Extract(blocks);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_WithEmptyBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("");

        // Act
        var result = _extractor.Extract([block]);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_ViewWithoutSelectQuery_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE VIEW incomplete_view");

        // Act
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
        var result = _extractor.Extract([block]);

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
