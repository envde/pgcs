using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для ConstraintExtractor
/// </summary>
public sealed class ConstraintExtractorTests
{
    private readonly IExtractor<ConstraintDefinition> _extractor = new ConstraintExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidConstraintBlock_ReturnsTrue()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT pk_users PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TABLE test_table (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100)
            );
        ");

        // Act
        var result = _extractor.CanExtract(blocks);

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

    #region Extract PRIMARY KEY Tests

    [Fact]
    public void Extract_PrimaryKeySingleColumn_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT pk_users PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("pk_users", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Definition.Type);
        Assert.Single(result.Definition.Columns);
        Assert.Contains("id", result.Definition.Columns);
    }

    [Fact]
    public void Extract_PrimaryKeyMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE order_items
                ADD CONSTRAINT pk_order_items PRIMARY KEY (order_id, product_id);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("pk_order_items", result.Definition.Name);
        Assert.Equal("order_items", result.Definition.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Definition.Type);
        Assert.Equal(2, result.Definition.Columns.Count);
        Assert.Contains("order_id", result.Definition.Columns);
        Assert.Contains("product_id", result.Definition.Columns);
    }

    [Fact]
    public void Extract_PrimaryKeyWithSchema_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE public.users
                ADD CONSTRAINT pk_public_users PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("pk_public_users", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal("public", result.Definition.Schema);
        Assert.Equal(ConstraintType.PrimaryKey, result.Definition.Type);
    }

    #endregion

    #region Extract FOREIGN KEY Tests

    [Fact]
    public void Extract_ForeignKeySingleColumn_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("fk_orders_user", result.Definition.Name);
        Assert.Equal("orders", result.Definition.TableName);
        Assert.Equal(ConstraintType.ForeignKey, result.Definition.Type);
        Assert.Single(result.Definition.Columns);
        Assert.Contains("user_id", result.Definition.Columns);
        Assert.Equal("users", result.Definition.ReferencedTable);
        Assert.NotNull(result.Definition.ReferencedColumns);
        Assert.Single(result.Definition.ReferencedColumns);
        Assert.Contains("id", result.Definition.ReferencedColumns);
    }

    [Fact]
    public void Extract_ForeignKeyWithOnDelete_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                ON DELETE CASCADE;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("fk_orders_user", result.Definition.Name);
        Assert.Equal(ConstraintType.ForeignKey, result.Definition.Type);
        Assert.Equal(ReferentialAction.Cascade, result.Definition.OnDelete);
        Assert.Null(result.Definition.OnUpdate);
    }

    [Fact]
    public void Extract_ForeignKeyWithOnDeleteAndOnUpdate_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                ON DELETE RESTRICT
                ON UPDATE CASCADE;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("fk_orders_user", result.Definition.Name);
        Assert.Equal(ConstraintType.ForeignKey, result.Definition.Type);
        Assert.Equal(ReferentialAction.Restrict, result.Definition.OnDelete);
        Assert.Equal(ReferentialAction.Cascade, result.Definition.OnUpdate);
    }

    [Fact]
    public void Extract_ForeignKeyMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE order_details
                ADD CONSTRAINT fk_order_details_order
                FOREIGN KEY (order_id, product_id)
                REFERENCES orders (id, product_id);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("fk_order_details_order", result.Definition.Name);
        Assert.Equal(ConstraintType.ForeignKey, result.Definition.Type);
        Assert.Equal(2, result.Definition.Columns.Count);
        Assert.Contains("order_id", result.Definition.Columns);
        Assert.Contains("product_id", result.Definition.Columns);
        Assert.NotNull(result.Definition.ReferencedColumns);
        Assert.Equal(2, result.Definition.ReferencedColumns.Count);
        Assert.Contains("id", result.Definition.ReferencedColumns);
        Assert.Contains("product_id", result.Definition.ReferencedColumns);
    }

    [Fact]
    public void Extract_ForeignKeyWithSetNull_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_coupon
                FOREIGN KEY (coupon_id)
                REFERENCES coupons (id)
                ON DELETE SET NULL;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(ReferentialAction.SetNull, result.Definition.OnDelete);
    }

    [Fact]
    public void Extract_ForeignKeyWithSetDefault_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_status
                FOREIGN KEY (status_id)
                REFERENCES statuses (id)
                ON DELETE SET DEFAULT;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(ReferentialAction.SetDefault, result.Definition.OnDelete);
    }

    [Fact]
    public void Extract_ForeignKeyWithNoAction_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                ON DELETE NO ACTION;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(ReferentialAction.NoAction, result.Definition.OnDelete);
    }

    #endregion

    #region Extract UNIQUE Tests

    [Fact]
    public void Extract_UniqueSingleColumn_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT uk_users_email UNIQUE (email);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("uk_users_email", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal(ConstraintType.Unique, result.Definition.Type);
        Assert.Single(result.Definition.Columns);
        Assert.Contains("email", result.Definition.Columns);
    }

    [Fact]
    public void Extract_UniqueMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT uk_users_username_email UNIQUE (username, email);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("uk_users_username_email", result.Definition.Name);
        Assert.Equal(ConstraintType.Unique, result.Definition.Type);
        Assert.Equal(2, result.Definition.Columns.Count);
        Assert.Contains("username", result.Definition.Columns);
        Assert.Contains("email", result.Definition.Columns);
    }

    #endregion

    #region Extract CHECK Tests

    [Fact]
    public void Extract_CheckSimpleCondition_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_users_age CHECK (age >= 18);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("chk_users_age", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal(ConstraintType.Check, result.Definition.Type);
        Assert.NotNull(result.Definition.CheckExpression);
        Assert.Equal("age >= 18", result.Definition.CheckExpression);
    }

    [Fact]
    public void Extract_CheckComplexCondition_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT chk_orders_total CHECK (total >= 0 AND total <= 1000000);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("chk_orders_total", result.Definition.Name);
        Assert.Equal(ConstraintType.Check, result.Definition.Type);
        Assert.NotNull(result.Definition.CheckExpression);
        Assert.Equal("total >= 0 AND total <= 1000000", result.Definition.CheckExpression);
    }

    [Fact]
    public void Extract_CheckWithParenthesesInExpression_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_users_deleted CHECK ((is_deleted = FALSE AND deleted_at IS NULL) OR (is_deleted = TRUE AND deleted_at IS NOT NULL));
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("chk_users_deleted", result.Definition.Name);
        Assert.Equal(ConstraintType.Check, result.Definition.Type);
        Assert.NotNull(result.Definition.CheckExpression);
        Assert.Contains("is_deleted", result.Definition.CheckExpression);
        Assert.Contains("deleted_at", result.Definition.CheckExpression);
    }

    #endregion

    #region Extract DEFERRABLE Tests

    [Fact]
    public void Extract_ForeignKeyDeferrable_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                DEFERRABLE;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.True(result.Definition.IsDeferrable);
        Assert.False(result.Definition.IsInitiallyDeferred);
    }

    [Fact]
    public void Extract_ForeignKeyInitiallyDeferred_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                DEFERRABLE INITIALLY DEFERRED;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.True(result.Definition.IsDeferrable);
        Assert.True(result.Definition.IsInitiallyDeferred);
    }

    [Fact]
    public void Extract_CheckDeferrable_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_users_balance
                CHECK (balance >= 0)
                DEFERRABLE;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.True(result.Definition.IsDeferrable);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_ConstraintUpperCase_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE USERS
                ADD CONSTRAINT PK_USERS PRIMARY KEY (ID);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("PK_USERS", result.Definition.Name);
        Assert.Equal("USERS", result.Definition.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Definition.Type);
    }

    [Fact]
    public void Extract_ConstraintMixedCase_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            alter table Users
                add constraint pk_Users primary key (Id);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("pk_Users", result.Definition.Name);
        Assert.Equal("Users", result.Definition.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Definition.Type);
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_InvalidSql_ReturnsNotApplicable()
    {
        // Arrange
        var blocks = CreateBlocks("INVALID SQL STATEMENT");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    [Fact]
    public void Extract_NullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_EmptyBlock_ReturnsNotApplicable()
    {
        // Arrange
        var blocks = CreateBlocks("");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    [Fact]
    public void Extract_ConstraintWithoutName_ReturnsNotApplicable()
    {
        // Arrange
        // This is a column constraint, not an ALTER TABLE ADD CONSTRAINT
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        // Should return NotApplicable because it doesn't contain "CONSTRAINT" keyword
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_RealWorldPrimaryKey_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT users_pkey PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("users_pkey", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Definition.Type);
        Assert.Single(result.Definition.Columns);
    }

    [Fact]
    public void Extract_RealWorldForeignKey_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                ON DELETE RESTRICT
                ON UPDATE CASCADE;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("fk_orders_user", result.Definition.Name);
        Assert.Equal("orders", result.Definition.TableName);
        Assert.Equal(ConstraintType.ForeignKey, result.Definition.Type);
        Assert.Single(result.Definition.Columns);
        Assert.Contains("user_id", result.Definition.Columns);
        Assert.Equal("users", result.Definition.ReferencedTable);
        Assert.Equal(ReferentialAction.Restrict, result.Definition.OnDelete);
        Assert.Equal(ReferentialAction.Cascade, result.Definition.OnUpdate);
    }

    [Fact]
    public void Extract_RealWorldUniqueConstraint_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT users_username_key UNIQUE (username);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("users_username_key", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal(ConstraintType.Unique, result.Definition.Type);
        Assert.Single(result.Definition.Columns);
        Assert.Contains("username", result.Definition.Columns);
    }

    [Fact]
    public void Extract_RealWorldCheckConstraint_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_balance CHECK (balance >= 0);
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("chk_balance", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal(ConstraintType.Check, result.Definition.Type);
        Assert.NotNull(result.Definition.CheckExpression);
        Assert.Equal("balance >= 0", result.Definition.CheckExpression);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Создает SQL блок для тестирования
    /// </summary>
    private static IReadOnlyList<SqlBlock> CreateBlocks(string content)
    {
        return [new SqlBlock
        {
            Content = content,
            RawContent = content,
            HeaderComment = null,
            InlineComments = null,
            StartLine = 1,
            EndLine = content.Split('\n').Length,
            SourcePath = "test.sql"
        }];
    }

    #endregion
}
