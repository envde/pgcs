using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для IndexExtractor
/// Покрывает все типы индексов, методы (BTREE/HASH/GIN/GIST/BRIN), UNIQUE, WHERE, INCLUDE, и специальные форматы комментариев
/// </summary>
public sealed class IndexExtractorTests
{
    private readonly IIndexExtractor _extractor = new IndexExtractor();

    [Fact]
    public void Extract_BasicIndexes_HandlesAllVariants()
    {
        // Покрывает: simple index, UNIQUE, multiple columns, schema qualification

        // Simple single column index
        var simpleBlock = CreateBlock("CREATE INDEX idx_users_email ON users (email);");
        var simpleResult = _extractor.Extract(simpleBlock);
        Assert.NotNull(simpleResult);
        Assert.Equal("idx_users_email", simpleResult.Name);
        Assert.Equal("users", simpleResult.TableName);
        Assert.Single(simpleResult.Columns);
        Assert.Equal("email", simpleResult.Columns[0]);
        Assert.False(simpleResult.IsUnique);
        Assert.Equal(IndexMethod.BTree, simpleResult.Method);
        Assert.Null(simpleResult.Schema);

        // UNIQUE index
        var uniqueBlock = CreateBlock("CREATE UNIQUE INDEX idx_users_username ON users (username);");
        var uniqueResult = _extractor.Extract(uniqueBlock);
        Assert.NotNull(uniqueResult);
        Assert.Equal("idx_users_username", uniqueResult.Name);
        Assert.True(uniqueResult.IsUnique);

        // Multiple columns
        var multiColumnBlock = CreateBlock("CREATE INDEX idx_users_name ON users (last_name, first_name, email);");
        var multiColumnResult = _extractor.Extract(multiColumnBlock);
        Assert.NotNull(multiColumnResult);
        Assert.Equal(3, multiColumnResult.Columns.Count);
        Assert.Equal("last_name", multiColumnResult.Columns[0]);
        Assert.Equal("first_name", multiColumnResult.Columns[1]);
        Assert.Equal("email", multiColumnResult.Columns[2]);

        // Schema qualification
        var schemaBlock = CreateBlock("CREATE INDEX idx_users_email ON public.users (email);");
        var schemaResult = _extractor.Extract(schemaBlock);
        Assert.NotNull(schemaResult);
        Assert.Equal("idx_users_email", schemaResult.Name);
        Assert.Equal("users", schemaResult.TableName);
        Assert.Equal("public", schemaResult.Schema);

        // Composite UNIQUE with schema
        var compositeBlock = CreateBlock("CREATE UNIQUE INDEX idx_products_sku ON myschema.products (sku, version);");
        var compositeResult = _extractor.Extract(compositeBlock);
        Assert.NotNull(compositeResult);
        Assert.True(compositeResult.IsUnique);
        Assert.Equal(2, compositeResult.Columns.Count);
        Assert.Equal("myschema", compositeResult.Schema);
    }

    [Fact]
    public void Extract_IndexMethods_HandlesAllTypes()
    {
        // Покрывает: BTREE, HASH, GIN, GIST, BRIN, SP-GIST

        // BTREE (default)
        var btreeBlock = CreateBlock("CREATE INDEX idx_users_id ON users USING btree (id);");
        var btreeResult = _extractor.Extract(btreeBlock);
        Assert.NotNull(btreeResult);
        Assert.Equal(IndexMethod.BTree, btreeResult.Method);

        // HASH
        var hashBlock = CreateBlock("CREATE INDEX idx_users_hash ON users USING hash (email);");
        var hashResult = _extractor.Extract(hashBlock);
        Assert.NotNull(hashResult);
        Assert.Equal(IndexMethod.Hash, hashResult.Method);

        // GIN (for arrays, JSONB)
        var ginBlock = CreateBlock("CREATE INDEX idx_users_tags ON users USING gin (tags);");
        var ginResult = _extractor.Extract(ginBlock);
        Assert.NotNull(ginResult);
        Assert.Equal(IndexMethod.Gin, ginResult.Method);

        // GIST (for spatial data)
        var gistBlock = CreateBlock("CREATE INDEX idx_locations_point ON locations USING gist (coordinates);");
        var gistResult = _extractor.Extract(gistBlock);
        Assert.NotNull(gistResult);
        Assert.Equal(IndexMethod.Gist, gistResult.Method);

        // BRIN (Block Range INdexes)
        var brinBlock = CreateBlock("CREATE INDEX idx_logs_created ON logs USING brin (created_at);");
        var brinResult = _extractor.Extract(brinBlock);
        Assert.NotNull(brinResult);
        Assert.Equal(IndexMethod.Brin, brinResult.Method);

        // SP-GIST
        var spgistBlock = CreateBlock("CREATE INDEX idx_ranges ON ranges USING spgist (ip_range);");
        var spgistResult = _extractor.Extract(spgistBlock);
        Assert.NotNull(spgistResult);
        Assert.Equal(IndexMethod.SpGist, spgistResult.Method);
    }

    [Fact]
    public void Extract_AdvancedFeatures_HandlesAllOptions()
    {
        // Покрывает: WHERE условия, INCLUDE columns, expressions, CONCURRENTLY, IF NOT EXISTS, NULLS FIRST/LAST, ASC/DESC

        // Partial index with WHERE
        var whereBlock = CreateBlock("CREATE INDEX idx_users_active ON users (email) WHERE is_active = TRUE;");
        var whereResult = _extractor.Extract(whereBlock);
        Assert.NotNull(whereResult);
        Assert.Equal("idx_users_active", whereResult.Name);
        Assert.NotNull(whereResult.WhereClause);
        Assert.Equal("is_active = TRUE", whereResult.WhereClause);

        // Complex WHERE condition
        var complexWhereBlock = CreateBlock("CREATE INDEX idx_orders_pending ON orders (created_at) WHERE status = 'pending' AND total > 0;");
        var complexWhereResult = _extractor.Extract(complexWhereBlock);
        Assert.NotNull(complexWhereResult);
        Assert.NotNull(complexWhereResult.WhereClause);
        Assert.Contains("status = 'pending'", complexWhereResult.WhereClause);
        Assert.Contains("AND", complexWhereResult.WhereClause);

        // INCLUDE columns (covering index)
        var includeBlock = CreateBlock("CREATE UNIQUE INDEX idx_users_email_include ON users (email) INCLUDE (username, full_name);");
        var includeResult = _extractor.Extract(includeBlock);
        Assert.NotNull(includeResult);
        Assert.True(includeResult.IsUnique);
        Assert.NotNull(includeResult.IncludeColumns);
        Assert.Equal(2, includeResult.IncludeColumns.Count);
        Assert.Contains("username", includeResult.IncludeColumns);
        Assert.Contains("full_name", includeResult.IncludeColumns);

        // Expression index
        var expressionBlock = CreateBlock("CREATE INDEX idx_users_lower_email ON users (LOWER(email));");
        var expressionResult = _extractor.Extract(expressionBlock);
        Assert.NotNull(expressionResult);
        Assert.Single(expressionResult.Columns);
        Assert.Contains("LOWER(email)", expressionResult.Columns[0]);

        // CREATE INDEX CONCURRENTLY
        var concurrentlyBlock = CreateBlock("CREATE INDEX CONCURRENTLY idx_users_created ON users (created_at);");
        var concurrentlyResult = _extractor.Extract(concurrentlyBlock);
        Assert.NotNull(concurrentlyResult);
        Assert.Equal("idx_users_created", concurrentlyResult.Name);

        // IF NOT EXISTS
        var ifNotExistsBlock = CreateBlock("CREATE INDEX IF NOT EXISTS idx_users_email ON users (email);");
        var ifNotExistsResult = _extractor.Extract(ifNotExistsBlock);
        Assert.NotNull(ifNotExistsResult);
        Assert.Equal("idx_users_email", ifNotExistsResult.Name);

        // Column ordering: DESC NULLS LAST
        var orderingBlock = CreateBlock("CREATE INDEX idx_users_created_desc ON users (created_at DESC NULLS LAST);");
        var orderingResult = _extractor.Extract(orderingBlock);
        Assert.NotNull(orderingResult);
        Assert.Single(orderingResult.Columns);
        // Index может хранить "created_at DESC NULLS LAST" или просто "created_at" в зависимости от реализации
        Assert.Contains("created_at", orderingResult.Columns[0]);

        // Multiple columns with different ordering
        var multiOrderBlock = CreateBlock("CREATE INDEX idx_products_price ON products (category_id ASC, price DESC);");
        var multiOrderResult = _extractor.Extract(multiOrderBlock);
        Assert.NotNull(multiOrderResult);
        Assert.Equal(2, multiOrderResult.Columns.Count);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: специальные форматы комментариев с метаданными
        // Формат 1: comment: Описание; type: ТИП; rename: НовоеИмя;
        // Формат 2: comment(...); type(...); rename(...);

        // Формат 1: comment: ...; rename: ...;
        var block1 = CreateBlock(
            "CREATE INDEX idx_users_email ON users (email);",
            "comment: Индекс для быстрого поиска по email; rename: UsersEmailIndex; type: BTreeIndex;");

        var result1 = _extractor.Extract(block1);
        Assert.NotNull(result1);
        Assert.NotNull(result1.SqlComment);
        Assert.Contains("comment:", result1.SqlComment);
        Assert.Contains("Индекс для быстрого поиска по email", result1.SqlComment);
        Assert.Contains("rename:", result1.SqlComment);
        Assert.Contains("UsersEmailIndex", result1.SqlComment);
        Assert.Contains("type:", result1.SqlComment);
        Assert.Contains("BTreeIndex", result1.SqlComment);

        // Формат 2: comment(...); rename(...); type(...);
        var block2 = CreateBlock(
            "CREATE UNIQUE INDEX idx_products_sku ON products (sku);",
            "comment(Уникальный индекс для SKU); rename(ProductsSkuUniqueIndex); type(UniqueIndex);");

        var result2 = _extractor.Extract(block2);
        Assert.NotNull(result2);
        Assert.NotNull(result2.SqlComment);
        Assert.Contains("comment(", result2.SqlComment);
        Assert.Contains("Уникальный индекс для SKU", result2.SqlComment);
        Assert.Contains("rename(ProductsSkuUniqueIndex)", result2.SqlComment);
        Assert.Contains("type(UniqueIndex)", result2.SqlComment);

        // Смешанный формат с GIN
        var block3 = CreateBlock(
            "CREATE INDEX idx_posts_search USING GIN ON posts (search_vector);",
            "comment: Полнотекстовый поиск по постам; type(GinIndex);");

        var result3 = _extractor.Extract(block3);
        Assert.NotNull(result3);
        Assert.NotNull(result3.SqlComment);
        Assert.Contains("comment:", result3.SqlComment);
        Assert.Contains("Полнотекстовый поиск по постам", result3.SqlComment);
        Assert.Contains("type(GinIndex)", result3.SqlComment);

        // Формат 1 с WHERE условием
        var block4 = CreateBlock(
            "CREATE INDEX idx_orders_active ON orders (created_at) WHERE status = 'active';",
            "comment: Частичный индекс для активных заказов; rename: OrdersActiveIndex; type: PartialIndex;");

        var result4 = _extractor.Extract(block4);
        Assert.NotNull(result4);
        Assert.NotNull(result4.SqlComment);
        Assert.Contains("Частичный индекс для активных заказов", result4.SqlComment);
        Assert.Contains("rename:", result4.SqlComment);
        Assert.Contains("type:", result4.SqlComment);
        Assert.NotNull(result4.WhereClause);

        // Формат 2 с INCLUDE
        var block5 = CreateBlock(
            "CREATE UNIQUE INDEX idx_users_email_covering ON users (email) INCLUDE (username);",
            "comment(Покрывающий индекс с дополнительными колонками); rename(UsersEmailCoveringIndex);");

        var result5 = _extractor.Extract(block5);
        Assert.NotNull(result5);
        Assert.NotNull(result5.SqlComment);
        Assert.Contains("comment(Покрывающий индекс с дополнительными колонками)", result5.SqlComment);
        Assert.Contains("rename(UsersEmailCoveringIndex)", result5.SqlComment);
        Assert.NotNull(result5.IncludeColumns);
    }

    [Fact]
    public void Extract_EdgeCasesAndValidation_HandlesCorrectly()
    {
        // Покрывает: null checks, non-index blocks, empty input, case sensitivity, expression indexes

        // Null checks
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));

        // Non-index block
        var tableBlock = CreateBlock("CREATE TABLE users (id INTEGER);");
        Assert.False(_extractor.CanExtract(tableBlock));

        // View block
        var viewBlock = CreateBlock("CREATE VIEW user_view AS SELECT * FROM users;");
        Assert.False(_extractor.CanExtract(viewBlock));

        // Empty block
        var emptyBlock = CreateBlock("");
        Assert.False(_extractor.CanExtract(emptyBlock));

        // Invalid SQL
        var invalidBlock = CreateBlock("INVALID SQL STATEMENT");
        Assert.False(_extractor.CanExtract(invalidBlock));

        // Uppercase
        var upperBlock = CreateBlock("CREATE INDEX IDX_USERS_EMAIL ON USERS (EMAIL);");
        var upperResult = _extractor.Extract(upperBlock);
        Assert.NotNull(upperResult);
        Assert.Equal("IDX_USERS_EMAIL", upperResult.Name);
        Assert.Equal("USERS", upperResult.TableName);

        // Mixed case
        var mixedBlock = CreateBlock("CrEaTe InDeX idx_test On TestTable (TestColumn);");
        var mixedResult = _extractor.Extract(mixedBlock);
        Assert.NotNull(mixedResult);
        Assert.Equal("idx_test", mixedResult.Name);

        // Complex expression index
        var complexExprBlock = CreateBlock("CREATE INDEX idx_users_year ON users (EXTRACT(YEAR FROM created_at));");
        var complexExprResult = _extractor.Extract(complexExprBlock);
        Assert.NotNull(complexExprResult);
        Assert.Single(complexExprResult.Columns);

        // Index with operator class
        var opClassBlock = CreateBlock("CREATE INDEX idx_users_name_pattern ON users (name text_pattern_ops);");
        var opClassResult = _extractor.Extract(opClassBlock);
        Assert.NotNull(opClassResult);
        Assert.Equal("idx_users_name_pattern", opClassResult.Name);

        // GIN index on JSONB with jsonb_path_ops
        var jsonbBlock = CreateBlock("CREATE INDEX idx_metadata ON users USING gin (metadata jsonb_path_ops);");
        var jsonbResult = _extractor.Extract(jsonbBlock);
        Assert.NotNull(jsonbResult);
        Assert.Equal(IndexMethod.Gin, jsonbResult.Method);

        // Multiple expressions
        var multiExprBlock = CreateBlock("CREATE INDEX idx_users_full_name ON users (LOWER(first_name), LOWER(last_name));");
        var multiExprResult = _extractor.Extract(multiExprBlock);
        Assert.NotNull(multiExprResult);
        Assert.Equal(2, multiExprResult.Columns.Count);

        // CONCURRENTLY with UNIQUE
        var concurUniqueBlock = CreateBlock("CREATE UNIQUE INDEX CONCURRENTLY idx_unique ON users (email);");
        var concurUniqueResult = _extractor.Extract(concurUniqueBlock);
        Assert.NotNull(concurUniqueResult);
        Assert.True(concurUniqueResult.IsUnique);
    }

    private static SqlBlock CreateBlock(string sql, string? headerComment = null)
    {
        return new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            HeaderComment = headerComment,
            InlineComments = null,
            StartLine = 1,
            EndLine = sql.Split('\n').Length,
            SourcePath = "test.sql"
        };
    }
}
