using PgCs.Core.Parsing.Blocks;


namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Комплексные сценарии для BlockExtractor
/// </summary>
public sealed class BlockExtractorComplexTests
{

    [Fact]
    public void Extract_CompleteTableWithComments_ExtractsAll()
    {
        // Arrange
        var sql = """
            -- Таблица заказов
            CREATE TABLE orders (
                id SERIAL PRIMARY KEY,
                user_id INTEGER NOT NULL,
                total NUMERIC(10, 2) NOT NULL
            );
            
            COMMENT ON TABLE orders IS 'Заказы пользователей';
            COMMENT ON COLUMN orders.id IS 'Уникальный идентификатор';
            COMMENT ON COLUMN orders.user_id IS 'ID пользователя';
            
            CREATE INDEX idx_orders_user_id ON orders (user_id);
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(5, blocks.Count);
        Assert.Equal("Таблица заказов", blocks[0].HeaderComment);
        Assert.Contains("CREATE TABLE", blocks[0].Content);
        Assert.Contains("COMMENT ON TABLE", blocks[1].Content);
        Assert.Contains("COMMENT ON COLUMN", blocks[2].Content);
        Assert.Contains("CREATE INDEX", blocks[4].Content);
    }

    [Fact]
    public void Extract_MultipleTypesWithComments_ExtractsAll()
    {
        // Arrange
        var sql = """
            -- Статус пользователя
            CREATE TYPE user_status AS ENUM ('active', 'inactive');
            
            -- Статус заказа
            CREATE TYPE order_status AS ENUM ('pending', 'completed');
            
            COMMENT ON TYPE user_status IS 'Возможные статусы';
            COMMENT ON TYPE order_status IS 'Статусы заказа';
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.Equal(4, blocks.Count);
        Assert.Equal("Статус пользователя", blocks[0].HeaderComment);
        Assert.Equal("Статус заказа", blocks[1].HeaderComment);
    }

    [Fact]
    public void Extract_FunctionWithTrigger_ExtractsMultipleBlocks()
    {
        // Arrange
        var sql = """
            CREATE OR REPLACE FUNCTION update_timestamp()
            RETURNS TRIGGER AS $$
            BEGIN
                NEW.updated_at = NOW();
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
            
            CREATE TRIGGER update_users_timestamp
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION update_timestamp();
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        // Функция разбивается на несколько блоков + триггер
        Assert.True(blocks.Count >= 2);
        var functionBlock = blocks[0];
        Assert.Contains("CREATE OR REPLACE FUNCTION", functionBlock.Content);
        
        // Последний блок должен быть триггером
        var triggerBlock = blocks[^1];
        Assert.Contains("CREATE TRIGGER", triggerBlock.Content);
    }
}
