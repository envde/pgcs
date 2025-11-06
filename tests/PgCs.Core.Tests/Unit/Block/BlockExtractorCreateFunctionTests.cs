using PgCs.Core.Parsing.Blocks;


namespace PgCs.Core.Tests.Unit.Block;

/// <summary>
/// Тесты для BlockExtractor с CREATE FUNCTION
/// </summary>
public sealed class BlockExtractorCreateFunctionTests
{

    [Fact]
    public void Extract_CreateFunction_ExtractsMultipleBlocks()
    {
        // Arrange
        // Примечание: функции с $$ и внутренними ; разбиваются на несколько блоков
        var sql = """
            -- Функция для обновления timestamp
            CREATE OR REPLACE FUNCTION update_updated_at()
            RETURNS TRIGGER AS $$
            BEGIN
                NEW.updated_at = NOW();
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        // Функция разбивается на несколько блоков из-за точек с запятой внутри
        Assert.True(blocks.Count >= 1);
        Assert.Equal("Функция для обновления timestamp", blocks[0].HeaderComment);
        Assert.Contains("CREATE OR REPLACE FUNCTION", blocks[0].Content);
        Assert.Contains("RETURNS TRIGGER", blocks[0].Content);
    }

    [Fact]
    public void Extract_FunctionWithParameters_ExtractsMultipleBlocks()
    {
        // Arrange
        var sql = """
            CREATE OR REPLACE FUNCTION get_user_orders(user_id_param INTEGER)
            RETURNS TABLE(order_id INTEGER, total NUMERIC) AS $$
            BEGIN
                RETURN QUERY
                SELECT id, total FROM orders WHERE user_id = user_id_param;
            END;
            $$ LANGUAGE plpgsql;
            """;

        // Act
        var blocks = BlockParser.Parse(sql);

        // Assert
        Assert.True(blocks.Count >= 1);
        // ContentBuilder добавляет пробел между токенами для читаемости
        Assert.Contains("get_user_orders", blocks[0].Content);
        Assert.Contains("user_id_param INTEGER", blocks[0].Content);
        Assert.Contains("RETURNS TABLE", blocks[0].Content);
    }
}
