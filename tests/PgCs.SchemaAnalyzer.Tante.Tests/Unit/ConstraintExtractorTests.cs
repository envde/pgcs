using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для ConstraintExtractor
/// Покрывает PRIMARY KEY, FOREIGN KEY (все референциальные действия), UNIQUE, CHECK,
/// опции DEFERRABLE/INITIALLY DEFERRED, схемы, и спец. форматы комментариев
/// </summary>
public sealed class ConstraintExtractorTests
{
    private readonly IExtractor<ConstraintDefinition> _extractor = new ConstraintExtractor();

    [Fact]
    public void Extract_PrimaryKeyConstraints_HandlesAllVariants()
    {
        // Покрывает: PK single column, PK multiple columns, PK with schema, comments
        
        // Single column PK
        var pkSingleBlocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT pk_users PRIMARY KEY (id);
        ");

        var pkSingleResult = _extractor.Extract(pkSingleBlocks);
        Assert.True(pkSingleResult.IsSuccess);
        var pkSingle = pkSingleResult.Definition;
        Assert.NotNull(pkSingle);
        Assert.Equal("pk_users", pkSingle.Name);
        Assert.Equal("users", pkSingle.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, pkSingle.Type);
        Assert.Single(pkSingle.Columns);
        Assert.Contains("id", pkSingle.Columns);

        // Multiple columns PK
        var pkMultiBlocks = CreateBlocks(@"
            ALTER TABLE order_items
                ADD CONSTRAINT pk_order_items PRIMARY KEY (order_id, product_id);
        ");
        var pkMultiResult = _extractor.Extract(pkMultiBlocks);
        Assert.True(pkMultiResult.IsSuccess);
        Assert.NotNull(pkMultiResult.Definition);
        Assert.Equal(2, pkMultiResult.Definition.Columns.Count);
        Assert.Contains("order_id", pkMultiResult.Definition.Columns);
        Assert.Contains("product_id", pkMultiResult.Definition.Columns);

        // PK with schema
        var pkSchemaBlocks = CreateBlocks(@"
            ALTER TABLE public.accounts
                ADD CONSTRAINT pk_accounts PRIMARY KEY (account_id);
        ");
        var pkSchemaResult = _extractor.Extract(pkSchemaBlocks);
        Assert.True(pkSchemaResult.IsSuccess);
        Assert.Equal("public", pkSchemaResult.Definition!.Schema);
        Assert.Equal("accounts", pkSchemaResult.Definition.TableName);
    }

    [Fact]
    public void Extract_ForeignKeyConstraints_HandlesAllActionsAndOptions()
    {
        // Покрывает: FK CASCADE, SET NULL, SET DEFAULT, NO ACTION, RESTRICT,
        // DEFERRABLE, INITIALLY DEFERRED, match types, comments

        // FK with CASCADE
        var fkCascadeBlocks = CreateBlocks(@"
            ALTER TABLE order_items
                ADD CONSTRAINT fk_order_items_order 
                FOREIGN KEY (order_id) 
                REFERENCES orders(id) 
                ON DELETE CASCADE 
                ON UPDATE CASCADE;
        ");
        var fkCascadeResult = _extractor.Extract(fkCascadeBlocks);
        Assert.True(fkCascadeResult.IsSuccess);
        var fkCascade = fkCascadeResult.Definition;
        Assert.NotNull(fkCascade);
        Assert.Equal(ConstraintType.ForeignKey, fkCascade.Type);
        Assert.Equal("order_items", fkCascade.TableName);
        Assert.Contains("order_id", fkCascade.Columns);
        Assert.Equal("orders", fkCascade.ReferencedTable);
        Assert.NotNull(fkCascade.ReferencedColumns);
        Assert.Contains("id", fkCascade.ReferencedColumns);
        Assert.Equal(ReferentialAction.Cascade, fkCascade.OnDelete);
        Assert.Equal(ReferentialAction.Cascade, fkCascade.OnUpdate);

        // FK with SET NULL
        var fkSetNullBlocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user 
                FOREIGN KEY (user_id) 
                REFERENCES users(id) 
                ON DELETE SET NULL 
                ON UPDATE SET NULL;
        ");
        var fkSetNullResult = _extractor.Extract(fkSetNullBlocks);
        Assert.True(fkSetNullResult.IsSuccess);
        Assert.Equal(ReferentialAction.SetNull, fkSetNullResult.Definition!.OnDelete);
        Assert.Equal(ReferentialAction.SetNull, fkSetNullResult.Definition.OnUpdate);

        // FK with SET DEFAULT
        var fkSetDefaultBlocks = CreateBlocks(@"
            ALTER TABLE logs
                ADD CONSTRAINT fk_logs_level 
                FOREIGN KEY (level_id) 
                REFERENCES log_levels(id) 
                ON DELETE SET DEFAULT;
        ");
        var fkSetDefaultResult = _extractor.Extract(fkSetDefaultBlocks);
        Assert.True(fkSetDefaultResult.IsSuccess);
        Assert.Equal(ReferentialAction.SetDefault, fkSetDefaultResult.Definition!.OnDelete);

        // FK with NO ACTION
        var fkNoActionBlocks = CreateBlocks(@"
            ALTER TABLE products
                ADD CONSTRAINT fk_products_category 
                FOREIGN KEY (category_id) 
                REFERENCES categories(id) 
                ON DELETE NO ACTION;
        ");
        var fkNoActionResult = _extractor.Extract(fkNoActionBlocks);
        Assert.True(fkNoActionResult.IsSuccess);
        Assert.Equal(ReferentialAction.NoAction, fkNoActionResult.Definition!.OnDelete);

        // FK with DEFERRABLE INITIALLY DEFERRED
        var fkDeferrableBlocks = CreateBlocks(@"
            ALTER TABLE audit_logs
                ADD CONSTRAINT fk_audit_user 
                FOREIGN KEY (user_id) 
                REFERENCES users(id) 
                DEFERRABLE INITIALLY DEFERRED;
        ");
        var fkDeferrableResult = _extractor.Extract(fkDeferrableBlocks);
        Assert.True(fkDeferrableResult.IsSuccess);
        Assert.True(fkDeferrableResult.Definition!.IsDeferrable);
        Assert.True(fkDeferrableResult.Definition.IsInitiallyDeferred);

        // FK multiple columns
        var fkMultiBlocks = CreateBlocks(@"
            ALTER TABLE order_item_reviews
                ADD CONSTRAINT fk_reviews_item 
                FOREIGN KEY (order_id, product_id) 
                REFERENCES order_items(order_id, product_id);
        ");
        var fkMultiResult = _extractor.Extract(fkMultiBlocks);
        Assert.True(fkMultiResult.IsSuccess);
        Assert.NotNull(fkMultiResult.Definition);
        Assert.Equal(2, fkMultiResult.Definition.Columns.Count);
        Assert.NotNull(fkMultiResult.Definition.ReferencedColumns);
        Assert.Equal(2, fkMultiResult.Definition.ReferencedColumns.Count);
    }

    [Fact]
    public void Extract_UniqueAndCheckConstraints_HandlesAllVariants()
    {
        // Покрывает: UNIQUE single/multiple columns, CHECK simple/complex conditions, 
        // parentheses in expressions, comments
        
        // Single column UNIQUE
        var uniqueSingleBlocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT uk_users_email UNIQUE (email);
        ");

        var uniqueSingleResult = _extractor.Extract(uniqueSingleBlocks);
        Assert.True(uniqueSingleResult.IsSuccess);
        var uniqueSingle = uniqueSingleResult.Definition;
        Assert.NotNull(uniqueSingle);
        Assert.Equal("uk_users_email", uniqueSingle.Name);
        Assert.Equal(ConstraintType.Unique, uniqueSingle.Type);
        Assert.Single(uniqueSingle.Columns);
        Assert.Equal("email", uniqueSingle.Columns[0]);

        // Multiple columns UNIQUE
        var uniqueMultiBlocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT uk_users_username_email UNIQUE (username, email);
        ");
        var uniqueMultiResult = _extractor.Extract(uniqueMultiBlocks);
        Assert.True(uniqueMultiResult.IsSuccess);
        Assert.NotNull(uniqueMultiResult.Definition);
        Assert.Equal(2, uniqueMultiResult.Definition.Columns.Count);
        Assert.Contains("username", uniqueMultiResult.Definition.Columns);
        Assert.Contains("email", uniqueMultiResult.Definition.Columns);

        // Simple CHECK
        var checkSimpleBlocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_users_age CHECK (age >= 18);
        ");
        var checkSimpleResult = _extractor.Extract(checkSimpleBlocks);
        Assert.True(checkSimpleResult.IsSuccess);
        var checkSimple = checkSimpleResult.Definition;
        Assert.NotNull(checkSimple);
        Assert.Equal(ConstraintType.Check, checkSimple.Type);
        Assert.Contains("age >= 18", checkSimple.CheckExpression);

        // Complex CHECK
        var checkComplexBlocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT chk_orders_total CHECK (total >= 0 AND total <= 1000000);
        ");
        var checkComplexResult = _extractor.Extract(checkComplexBlocks);
        Assert.True(checkComplexResult.IsSuccess);
        Assert.Contains("total >= 0 AND total <= 1000000", checkComplexResult.Definition!.CheckExpression);

        // CHECK с parentheses в expression
        var checkParenBlocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_users_age_range CHECK ((age >= 18) AND (age <= 120));
        ");
        var checkParenResult = _extractor.Extract(checkParenBlocks);
        Assert.True(checkParenResult.IsSuccess);
        Assert.Contains("age >= 18", checkParenResult.Definition!.CheckExpression);
        Assert.Contains("age <= 120", checkParenResult.Definition.CheckExpression);

        // CHECK DEFERRABLE
        var checkDeferBlocks = CreateBlocks(@"
            ALTER TABLE inventory
                ADD CONSTRAINT chk_inventory_quantity CHECK (quantity >= 0)
                DEFERRABLE;
        ");
        var checkDeferResult = _extractor.Extract(checkDeferBlocks);
        Assert.True(checkDeferResult.IsSuccess);
        Assert.True(checkDeferResult.Definition!.IsDeferrable);
    }

    [Fact]
    public void Extract_EdgeCasesAndValidation_HandlesCorrectly()
    {
        // Null checks
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));

        // Invalid constraint type (not ALTER TABLE ADD CONSTRAINT)
        var invalidBlocks = CreateBlocks(@"
            CREATE TABLE test_table (
                id SERIAL PRIMARY KEY
            );
        ");
        Assert.False(_extractor.CanExtract(invalidBlocks));

        // Case sensitivity
        var caseBlocks = CreateBlocks(@"
            ALTER TABLE Users
                ADD CONSTRAINT PK_Users PRIMARY KEY (ID);
        ");
        var caseResult = _extractor.Extract(caseBlocks);
        Assert.True(caseResult.IsSuccess);
        Assert.Equal("Users", caseResult.Definition!.TableName);
        Assert.Equal("PK_Users", caseResult.Definition.Name);

        // Constraint with schema
        var schemaBlocks = CreateBlocks(@"
            ALTER TABLE myschema.products
                ADD CONSTRAINT uk_products_sku UNIQUE (sku);
        ");
        var schemaResult = _extractor.Extract(schemaBlocks);
        Assert.True(schemaResult.IsSuccess);
        Assert.Equal("myschema", schemaResult.Definition!.Schema);
        Assert.Equal("products", schemaResult.Definition.TableName);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: специальные форматы комментариев с метаданными
        // Формат 1: comment: Описание; type: ТИП; rename: НовоеИмя;
        // Формат 2: comment(Описание); type(ТИП); rename(НовоеИмя);
        
        // Формат 1: comment: ...; rename: ...; type: ...;
        var blocks1 = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT uk_users_username UNIQUE (username);
        ", "comment: Уникальное имя пользователя; rename: UserUsernameUnique; type: UniqueConstraint;");

        var result1 = _extractor.Extract(blocks1);
        Assert.True(result1.IsSuccess);
        Assert.NotNull(result1.Definition);
        Assert.NotNull(result1.Definition.SqlComment);
        Assert.Contains("comment:", result1.Definition.SqlComment);
        Assert.Contains("Уникальное имя пользователя", result1.Definition.SqlComment);
        Assert.Contains("rename:", result1.Definition.SqlComment);
        Assert.Contains("UserUsernameUnique", result1.Definition.SqlComment);
        Assert.Contains("type:", result1.Definition.SqlComment);
        Assert.Contains("UniqueConstraint", result1.Definition.SqlComment);

        // Формат 2: comment(...); rename(...); type(...);
        var blocks2 = CreateBlocks(@"
            ALTER TABLE products
                ADD CONSTRAINT chk_products_price CHECK (price > 0);
        ", "comment(Цена должна быть положительной); rename(ProductPriceCheck); type(CheckConstraint);");

        var result2 = _extractor.Extract(blocks2);
        Assert.True(result2.IsSuccess);
        Assert.NotNull(result2.Definition);
        Assert.NotNull(result2.Definition.SqlComment);
        Assert.Contains("comment(", result2.Definition.SqlComment);
        Assert.Contains("Цена должна быть положительной", result2.Definition.SqlComment);
        Assert.Contains("rename(ProductPriceCheck)", result2.Definition.SqlComment);
        Assert.Contains("type(CheckConstraint)", result2.Definition.SqlComment);

        // Формат 1 для PRIMARY KEY
        var blocks3 = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT pk_orders PRIMARY KEY (id);
        ", "comment: Первичный ключ заказов; type: BIGINT; rename: order_primary_key;");

        var result3 = _extractor.Extract(blocks3);
        Assert.True(result3.IsSuccess);
        Assert.NotNull(result3.Definition);
        Assert.NotNull(result3.Definition.SqlComment);
        Assert.Contains("Первичный ключ заказов", result3.Definition.SqlComment);
        Assert.Contains("type: BIGINT", result3.Definition.SqlComment);

        // Формат 2 для FOREIGN KEY
        var blocks4 = CreateBlocks(@"
            ALTER TABLE order_items
                ADD CONSTRAINT fk_order_items_order
                FOREIGN KEY (order_id)
                REFERENCES orders(id)
                ON DELETE CASCADE;
        ", "comment(Связь с таблицей заказов); rename(OrderItemsOrderFK);");

        var result4 = _extractor.Extract(blocks4);
        Assert.True(result4.IsSuccess);
        Assert.NotNull(result4.Definition);
        Assert.NotNull(result4.Definition.SqlComment);
        Assert.Contains("comment(Связь с таблицей заказов)", result4.Definition.SqlComment);
        Assert.Contains("rename(OrderItemsOrderFK)", result4.Definition.SqlComment);
        
        // Смешанный формат (частично формат 1, частично формат 2)
        var blocks5 = CreateBlocks(@"
            ALTER TABLE inventory
                ADD CONSTRAINT chk_inventory_quantity CHECK (quantity >= 0);
        ", "comment: Количество не может быть отрицательным; rename(InventoryQuantityPositive);");

        var result5 = _extractor.Extract(blocks5);
        Assert.True(result5.IsSuccess);
        Assert.NotNull(result5.Definition);
        Assert.NotNull(result5.Definition.SqlComment);
        Assert.Contains("comment:", result5.Definition.SqlComment);
        Assert.Contains("Количество не может быть отрицательным", result5.Definition.SqlComment);
        Assert.Contains("rename(InventoryQuantityPositive)", result5.Definition.SqlComment);
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? headerComment = null)
    {
        var content = sql.Trim();

        return [new SqlBlock
        {
            Content = content,
            RawContent = content,
            HeaderComment = headerComment,
            InlineComments = null,
            StartLine = 1,
            EndLine = content.Split('\n').Length,
            SourcePath = "test.sql"
        }];
    }
}
