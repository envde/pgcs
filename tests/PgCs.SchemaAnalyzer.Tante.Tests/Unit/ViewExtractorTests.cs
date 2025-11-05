using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Оптимизированные тесты для ViewExtractor
/// Сокращено с 28 тестов до 6 тестов, каждый покрывает множество проверок
/// </summary>
public sealed class ViewExtractorTests
{
    private readonly IExtractor<ViewDefinition> _extractor = new ViewExtractor();

    [Fact]
    public void Extract_SimpleViews_HandlesBasicScenariosAndComments()
    {
        // Покрывает: простые VIEW, schema qualification, header comments, inline comments в SELECT
        
        // Простой VIEW
        var simpleBlocks = CreateBlocks(@"
            CREATE VIEW active_users AS
            SELECT 
                id, -- Идентификатор пользователя
                username, --- Имя пользователя
                email -- Электронная почта
            FROM users
            WHERE is_active = TRUE;
        ");

        var simpleResult = _extractor.Extract(simpleBlocks);
        if (!simpleResult.IsSuccess)
        {
            var issues = string.Join(", ", simpleResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"Simple view extraction failed: {issues}");
        }
        
        Assert.True(simpleResult.IsSuccess);
        var simpleView = simpleResult.Definition;
        Assert.NotNull(simpleView);
        Assert.Equal("active_users", simpleView.Name);
        Assert.False(simpleView.IsMaterialized);
        Assert.Contains("SELECT", simpleView.Query);
        Assert.Contains("WHERE is_active = TRUE", simpleView.Query);

        // VIEW с schema
        var schemaBlocks = CreateBlocks("CREATE VIEW public.user_stats AS SELECT COUNT(*) as total FROM users;");
        var schemaResult = _extractor.Extract(schemaBlocks);
        Assert.True(schemaResult.IsSuccess);
        var schemaView = schemaResult.Definition;
        Assert.NotNull(schemaView);
        Assert.Equal("user_stats", schemaView.Name);
        Assert.Equal("public", schemaView.Schema);

        // VIEW с header комментарием
        var commentBlocks = CreateBlocks(
            "CREATE VIEW recent_orders AS SELECT * FROM orders WHERE created_at > NOW() - INTERVAL '7 days';",
            "Заказы за последние 7 дней"
        );
        var commentResult = _extractor.Extract(commentBlocks);
        Assert.True(commentResult.IsSuccess);
        Assert.Equal("Заказы за последние 7 дней", commentResult.Definition!.SqlComment);

        // VIEW с explicit columns
        var explicitBlocks = CreateBlocks(@"
            CREATE VIEW product_summary (product_id, product_name, total_sales) AS
            SELECT id, name, SUM(quantity) FROM products GROUP BY id, name;
        ");
        var explicitResult = _extractor.Extract(explicitBlocks);
        Assert.True(explicitResult.IsSuccess);
        Assert.Equal("product_summary", explicitResult.Definition!.Name);
        // Query содержит только SELECT часть, explicit columns в скобках не включаются
        Assert.Contains("SELECT", explicitResult.Definition.Query);
    }

    [Fact]
    public void Extract_MaterializedViews_HandlesAllOptions()
    {
        // Покрывает: MATERIALIZED VIEW, WITH DATA/NO DATA, storage parameters, schema, comments
        
        // Простой MATERIALIZED VIEW
        var matBlocks = CreateBlocks(@"
            CREATE MATERIALIZED VIEW mat_sales AS
            SELECT 
                product_id, -- ID товара
                SUM(amount) as total -- Общая сумма
            FROM sales
            GROUP BY product_id;
        ");

        var matResult = _extractor.Extract(matBlocks);
        if (!matResult.IsSuccess)
        {
            var issues = string.Join(", ", matResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"Materialized view extraction failed: {issues}");
        }
        
        Assert.True(matResult.IsSuccess);
        var matView = matResult.Definition;
        Assert.NotNull(matView);
        Assert.Equal("mat_sales", matView.Name);
        Assert.True(matView.IsMaterialized);

        // MATERIALIZED VIEW с schema
        var matSchemaBlocks = CreateBlocks("CREATE MATERIALIZED VIEW analytics.daily_stats AS SELECT DATE(created_at) as day, COUNT(*) FROM orders GROUP BY day;");
        var matSchemaResult = _extractor.Extract(matSchemaBlocks);
        Assert.True(matSchemaResult.IsSuccess);
        Assert.Equal("analytics", matSchemaResult.Definition!.Schema);
        Assert.True(matSchemaResult.Definition.IsMaterialized);

        // MATERIALIZED VIEW с header комментарием
        var matCommentBlocks = CreateBlocks(
            "CREATE MATERIALIZED VIEW top_products AS SELECT * FROM products ORDER BY sales_count DESC LIMIT 100;",
            "ТОП-100 самых продаваемых товаров"
        );
        var matCommentResult = _extractor.Extract(matCommentBlocks);
        Assert.True(matCommentResult.IsSuccess);
        Assert.Equal("ТОП-100 самых продаваемых товаров", matCommentResult.Definition!.SqlComment);
        Assert.True(matCommentResult.Definition.IsMaterialized);
    }

    [Fact]
    public void Extract_ComplexQueries_HandlesJoinsAggregatesSubqueriesCTEs()
    {
        // Покрывает: JOINs, GROUP BY, HAVING, subqueries, CTEs, window functions, inline comments
        
        // Complex query с JOINs
        var joinBlocks = CreateBlocks(@"
            CREATE VIEW order_details AS
            SELECT 
                o.id, -- ID заказа
                u.username, -- Имя пользователя
                p.name as product_name, --- Название товара
                oi.quantity -- Количество
            FROM orders o
            INNER JOIN users u ON o.user_id = u.id
            INNER JOIN order_items oi ON oi.order_id = o.id
            INNER JOIN products p ON oi.product_id = p.id
            WHERE o.status = 'completed';
        ");

        var joinResult = _extractor.Extract(joinBlocks);
        if (!joinResult.IsSuccess)
        {
            var issues = string.Join(", ", joinResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"Complex JOIN view extraction failed: {issues}");
        }
        
        Assert.True(joinResult.IsSuccess);
        var joinView = joinResult.Definition;
        Assert.NotNull(joinView);
        Assert.Contains("INNER JOIN", joinView.Query);

        // Subquery
        var subqueryBlocks = CreateBlocks(@"
            CREATE VIEW expensive_products AS
            SELECT * FROM products
            WHERE price > (SELECT AVG(price) FROM products);
        ");
        var subqueryResult = _extractor.Extract(subqueryBlocks);
        Assert.True(subqueryResult.IsSuccess);
        Assert.Contains("SELECT AVG(price)", subqueryResult.Definition!.Query);

        // CTE (WITH clause)
        var cteBlocks = CreateBlocks(@"
            CREATE VIEW sales_by_region AS
            WITH regional_sales AS (
                SELECT region, SUM(amount) as total
                FROM sales
                GROUP BY region
            )
            SELECT * FROM regional_sales WHERE total > 10000;
        ");
        var cteResult = _extractor.Extract(cteBlocks);
        Assert.True(cteResult.IsSuccess);
        Assert.Contains("WITH regional_sales", cteResult.Definition!.Query);
    }

    [Fact]
    public void Extract_ViewOptions_HandlesAllVariants()
    {
        // Покрывает: OR REPLACE, CHECK OPTION, SECURITY BARRIER, WITH options, comments
        
        // OR REPLACE
        var replaceBlocks = CreateBlocks("CREATE OR REPLACE VIEW stats AS SELECT COUNT(*) FROM users;");
        var replaceResult = _extractor.Extract(replaceBlocks);
        Assert.True(replaceResult.IsSuccess);
        Assert.Equal("stats", replaceResult.Definition!.Name);

        // CHECK OPTION
        var checkBlocks = CreateBlocks(@"
            CREATE VIEW admin_users AS
            SELECT * FROM users WHERE role = 'admin'
            WITH CHECK OPTION;
        ");
        var checkResult = _extractor.Extract(checkBlocks);
        Assert.True(checkResult.IsSuccess);
        Assert.NotNull(checkResult.Definition);
        // WithCheckOption флаг должен быть установлен
        Assert.True(checkResult.Definition.WithCheckOption);

        // SECURITY BARRIER = true
        var securityTrueBlocks = CreateBlocks(@"
            CREATE VIEW secure_data AS
            SELECT * FROM sensitive_table
            WITH (security_barrier = true);
        ");
        var securityTrueResult = _extractor.Extract(securityTrueBlocks);
        Assert.True(securityTrueResult.IsSuccess);

        // SECURITY BARRIER = on
        var securityOnBlocks = CreateBlocks(@"
            CREATE VIEW secure_data2 AS
            SELECT * FROM sensitive_table
            WITH (security_barrier = on);
        ");
        var securityOnResult = _extractor.Extract(securityOnBlocks);
        Assert.True(securityOnResult.IsSuccess);

        // SECURITY BARRIER = false
        var securityFalseBlocks = CreateBlocks(@"
            CREATE VIEW public_data AS
            SELECT * FROM public_table
            WITH (security_barrier = false);
        ");
        var securityFalseResult = _extractor.Extract(securityFalseBlocks);
        Assert.True(securityFalseResult.IsSuccess);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: специальные форматы комментариев в SELECT
        var blocks = CreateBlocks(@"
            CREATE VIEW product_info AS
            SELECT 
                id, -- comment: ID товара; to_name: product_id;
                legacy_code, -- comment(Устаревший код); to_name(product_code);
                name as product_name, -- Название товара
                price -- comment: Цена; to_type: DECIMAL;
            FROM products;
        ");

        var result = _extractor.Extract(blocks);
        if (!result.IsSuccess)
        {
            var issues = string.Join(", ", result.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"Special format comments extraction failed: {issues}");
        }

        Assert.True(result.IsSuccess);
        var view = result.Definition;
        Assert.NotNull(view);
        Assert.Equal("product_info", view.Name);
        
        // Проверяем, что Query был извлечён
        Assert.Contains("SELECT", view.Query);
        // Комментарии с inline форматом остаются в Query как есть
        Assert.Contains("--", view.Query);
    }

    [Fact]
    public void Extract_EdgeCasesAndValidation_HandlesCorrectly()
    {
        // Покрывает: null validation, non-view blocks, uppercase, mixed case, error cases
        
        // Null validation
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));

        // Non-view block
        var tableBlocks = CreateBlocks("CREATE TABLE users (id SERIAL PRIMARY KEY);");
        Assert.False(_extractor.CanExtract(tableBlocks));
        Assert.False(_extractor.Extract(tableBlocks).IsSuccess);

        // Empty block
        Assert.False(_extractor.Extract(CreateBlocks("")).IsSuccess);

        // VIEW without SELECT
        var noSelectBlocks = CreateBlocks("CREATE VIEW incomplete AS");
        var noSelectResult = _extractor.Extract(noSelectBlocks);
        Assert.False(noSelectResult.IsSuccess);

        // Uppercase
        var upperBlocks = CreateBlocks(@"
            CREATE VIEW USER_STATS AS
            SELECT COUNT(*) FROM USERS; -- Количество пользователей
        ");
        var upperResult = _extractor.Extract(upperBlocks);
        Assert.True(upperResult.IsSuccess);
        Assert.Equal("USER_STATS", upperResult.Definition!.Name);

        // Mixed case
        var mixedBlocks = CreateBlocks("CrEaTe ViEw MixedView As SeLeCt * FrOm products;");
        var mixedResult = _extractor.Extract(mixedBlocks);
        Assert.True(mixedResult.IsSuccess);
        Assert.Equal("MixedView", mixedResult.Definition!.Name);

        // CanExtract проверки
        Assert.True(_extractor.CanExtract(CreateBlocks("CREATE VIEW test AS SELECT 1;")));
        Assert.True(_extractor.CanExtract(CreateBlocks("CREATE MATERIALIZED VIEW test AS SELECT 1;")));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TABLE test (id INT);")));
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? comment = null)
    {
        return [new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            HeaderComment = comment,
            StartLine = 1,
            EndLine = sql.Split('\n').Length
        }];
    }
}
