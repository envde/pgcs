using PgCs.Core.Parsing.Blocks;


namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor на реальных примерах из Schema.sql
/// </summary>
public sealed class BlockExtractorRealExamplesTests
{

    [Fact]
    public void Extract_RealEnumWithComment_ExtractsCorrectly()
    {
        // Arrange - пример из Schema.sql
        var sql = """
            -- ENUM для статуса пользователя
            CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');
            COMMENT ON TYPE user_status IS 'Возможные статусы пользователя в системе';
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(2, blocks.Count);
        Assert.Equal("ENUM для статуса пользователя", blocks[0].HeaderComment);
        Assert.Contains("CREATE TYPE user_status", blocks[0].Content);
        Assert.Contains("COMMENT ON TYPE user_status", blocks[1].Content);
    }

    [Fact]
    public void Extract_ComplexTableFromSchema_ExtractsCorrectly()
    {
        // Arrange - упрощённая версия таблицы users из Schema.sql
        var sql = """
            -- Таблица пользователей
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(50) NOT NULL UNIQUE,
                email VARCHAR(255) NOT NULL UNIQUE,
                status user_status DEFAULT 'active',
                created_at TIMESTAMP DEFAULT NOW(),
                CONSTRAINT check_username CHECK (LENGTH(username) >= 3)
            );
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Таблица пользователей", blocks[0].HeaderComment);
        Assert.Contains("CREATE TABLE users", blocks[0].Content);
        Assert.Contains("CONSTRAINT check_username", blocks[0].Content);
        Assert.True(blocks[0].LineCount >= 7);
    }

    [Fact]
    public void Extract_MultipleIndexesInSequence_ExtractsAll()
    {
        // Arrange - несколько индексов подряд
        var sql = """
            CREATE INDEX idx_users_email ON users (email) WHERE is_deleted = FALSE;
            CREATE INDEX idx_users_status ON users (status) WHERE is_deleted = FALSE;
            CREATE INDEX idx_users_created_at ON users (created_at DESC);
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(3, blocks.Count);
        Assert.Contains("idx_users_email", blocks[0].Content);
        Assert.Contains("idx_users_status", blocks[1].Content);
        Assert.Contains("idx_users_created_at", blocks[2].Content);
    }

    [Fact]
    public void Extract_ViewWithComplexSelect_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- Активные пользователи с заказами
            CREATE VIEW active_users_with_orders AS
            SELECT
                u.id,
                u.username,
                u.email,
                COUNT(o.id) AS order_count,
                SUM(o.total) AS total_spent
            FROM users u
            LEFT JOIN orders o ON u.id = o.user_id
            WHERE u.status = 'active' AND u.is_deleted = FALSE
            GROUP BY u.id, u.username, u.email;
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal("Активные пользователи с заказами", blocks[0].HeaderComment);
        Assert.Contains("CREATE VIEW active_users_with_orders", blocks[0].Content);
        Assert.Contains("LEFT JOIN", blocks[0].Content);
        Assert.Contains("GROUP BY", blocks[0].Content);
    }

    [Fact]
    public void Extract_MaterializedViewWithIndex_ExtractsAll()
    {
        // Arrange
        var sql = """
            CREATE MATERIALIZED VIEW category_statistics AS
            SELECT
                c.id,
                c.name,
                COUNT(oi.id) AS items_sold
            FROM categories c
            LEFT JOIN order_items oi ON c.id = oi.category_id
            GROUP BY c.id, c.name;
            
            CREATE UNIQUE INDEX idx_category_statistics_id ON category_statistics (id);
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(2, blocks.Count);
        Assert.Contains("CREATE MATERIALIZED VIEW", blocks[0].Content);
        Assert.Contains("CREATE UNIQUE INDEX", blocks[1].Content);
    }

    [Fact]
    public void Extract_DomainWithConstraints_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            -- Email с валидацией
            CREATE DOMAIN email AS VARCHAR(255)
                CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$');
            COMMENT ON DOMAIN email IS 'Email адрес с валидацией формата';
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(2, blocks.Count);
        Assert.Equal("Email с валидацией", blocks[0].HeaderComment);
        Assert.Contains("CREATE DOMAIN email", blocks[0].Content);
        Assert.Contains("CHECK", blocks[0].Content);
    }

    [Fact]
    public void Extract_EmptyLinesBetweenBlocks_SeparatesCorrectly()
    {
        // Arrange
        var sql = """
            CREATE TABLE table1 (id INT);
            
            
            CREATE TABLE table2 (id INT);
            
            CREATE TABLE table3 (id INT);
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(3, blocks.Count);
        Assert.All(blocks, block => Assert.Contains("CREATE TABLE", block.Content));
    }

    [Fact]
    public void Extract_StatementWithMultipleLines_PreservesStructure()
    {
        // Arrange
        var sql = """
            CREATE TABLE orders (
                id SERIAL,
                user_id INTEGER,
                total NUMERIC(10, 2),
                created_at TIMESTAMP,
                CONSTRAINT pk_orders PRIMARY KEY (id),
                CONSTRAINT fk_user FOREIGN KEY (user_id) 
                    REFERENCES users(id) 
                    ON DELETE CASCADE
            );
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Single(blocks);
        Assert.Equal(1, blocks[0].StartLine);
        Assert.Equal(10, blocks[0].EndLine);
        Assert.Equal(10, blocks[0].LineCount);
        Assert.Contains("ON DELETE CASCADE", blocks[0].Content);
    }

    [Fact]
    public void Extract_MixedCommentsAndStatements_HandlesCorrectly()
    {
        // Arrange
        var sql = """
            -- Первая таблица
            CREATE TABLE table1 (id INT);
            -- Комментарий к table1
            COMMENT ON TABLE table1 IS 'Описание первой таблицы';
            
            -- Вторая таблица
            CREATE TABLE table2 (id INT);
            -- Комментарий к table2
            COMMENT ON TABLE table2 IS 'Описание второй таблицы';
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(4, blocks.Count);
        Assert.Equal("Первая таблица", blocks[0].HeaderComment);
        Assert.Equal("Комментарий к table1", blocks[1].HeaderComment);
        Assert.Equal("Вторая таблица", blocks[2].HeaderComment);
        Assert.Equal("Комментарий к table2", blocks[3].HeaderComment);
    }
}
