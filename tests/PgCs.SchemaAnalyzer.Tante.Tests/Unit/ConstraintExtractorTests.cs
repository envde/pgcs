using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для ConstraintExtractor
/// </summary>
public sealed class ConstraintExtractorTests
{
    private readonly IConstraintExtractor _extractor = new ConstraintExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidConstraintBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT pk_users PRIMARY KEY (id);
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
        var block = CreateBlock(@"
            CREATE TABLE test_table (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100)
            );
        ");

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

    #region Extract PRIMARY KEY Tests

    [Fact]
    public void Extract_PrimaryKeySingleColumn_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT pk_users PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pk_users", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Type);
        Assert.Single(result.Columns);
        Assert.Contains("id", result.Columns);
    }

    [Fact]
    public void Extract_PrimaryKeyMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE order_items
                ADD CONSTRAINT pk_order_items PRIMARY KEY (order_id, product_id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pk_order_items", result.Name);
        Assert.Equal("order_items", result.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Type);
        Assert.Equal(2, result.Columns.Count);
        Assert.Contains("order_id", result.Columns);
        Assert.Contains("product_id", result.Columns);
    }

    [Fact]
    public void Extract_PrimaryKeyWithSchema_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE public.users
                ADD CONSTRAINT pk_public_users PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pk_public_users", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal("public", result.Schema);
        Assert.Equal(ConstraintType.PrimaryKey, result.Type);
    }

    #endregion

    #region Extract FOREIGN KEY Tests

    [Fact]
    public void Extract_ForeignKeySingleColumn_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fk_orders_user", result.Name);
        Assert.Equal("orders", result.TableName);
        Assert.Equal(ConstraintType.ForeignKey, result.Type);
        Assert.Single(result.Columns);
        Assert.Contains("user_id", result.Columns);
        Assert.Equal("users", result.ReferencedTable);
        Assert.NotNull(result.ReferencedColumns);
        Assert.Single(result.ReferencedColumns);
        Assert.Contains("id", result.ReferencedColumns);
    }

    [Fact]
    public void Extract_ForeignKeyWithOnDelete_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                ON DELETE CASCADE;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fk_orders_user", result.Name);
        Assert.Equal(ConstraintType.ForeignKey, result.Type);
        Assert.Equal(ReferentialAction.Cascade, result.OnDelete);
        Assert.Null(result.OnUpdate);
    }

    [Fact]
    public void Extract_ForeignKeyWithOnDeleteAndOnUpdate_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                ON DELETE RESTRICT
                ON UPDATE CASCADE;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fk_orders_user", result.Name);
        Assert.Equal(ConstraintType.ForeignKey, result.Type);
        Assert.Equal(ReferentialAction.Restrict, result.OnDelete);
        Assert.Equal(ReferentialAction.Cascade, result.OnUpdate);
    }

    [Fact]
    public void Extract_ForeignKeyMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE order_details
                ADD CONSTRAINT fk_order_details_order
                FOREIGN KEY (order_id, product_id)
                REFERENCES orders (id, product_id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fk_order_details_order", result.Name);
        Assert.Equal(ConstraintType.ForeignKey, result.Type);
        Assert.Equal(2, result.Columns.Count);
        Assert.Contains("order_id", result.Columns);
        Assert.Contains("product_id", result.Columns);
        Assert.NotNull(result.ReferencedColumns);
        Assert.Equal(2, result.ReferencedColumns.Count);
        Assert.Contains("id", result.ReferencedColumns);
        Assert.Contains("product_id", result.ReferencedColumns);
    }

    [Fact]
    public void Extract_ForeignKeyWithSetNull_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_coupon
                FOREIGN KEY (coupon_id)
                REFERENCES coupons (id)
                ON DELETE SET NULL;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ReferentialAction.SetNull, result.OnDelete);
    }

    [Fact]
    public void Extract_ForeignKeyWithSetDefault_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_status
                FOREIGN KEY (status_id)
                REFERENCES statuses (id)
                ON DELETE SET DEFAULT;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ReferentialAction.SetDefault, result.OnDelete);
    }

    [Fact]
    public void Extract_ForeignKeyWithNoAction_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                ON DELETE NO ACTION;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ReferentialAction.NoAction, result.OnDelete);
    }

    #endregion

    #region Extract UNIQUE Tests

    [Fact]
    public void Extract_UniqueSingleColumn_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT uk_users_email UNIQUE (email);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("uk_users_email", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(ConstraintType.Unique, result.Type);
        Assert.Single(result.Columns);
        Assert.Contains("email", result.Columns);
    }

    [Fact]
    public void Extract_UniqueMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT uk_users_username_email UNIQUE (username, email);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("uk_users_username_email", result.Name);
        Assert.Equal(ConstraintType.Unique, result.Type);
        Assert.Equal(2, result.Columns.Count);
        Assert.Contains("username", result.Columns);
        Assert.Contains("email", result.Columns);
    }

    #endregion

    #region Extract CHECK Tests

    [Fact]
    public void Extract_CheckSimpleCondition_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_users_age CHECK (age >= 18);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("chk_users_age", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(ConstraintType.Check, result.Type);
        Assert.NotNull(result.CheckExpression);
        Assert.Equal("age >= 18", result.CheckExpression);
    }

    [Fact]
    public void Extract_CheckComplexCondition_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT chk_orders_total CHECK (total >= 0 AND total <= 1000000);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("chk_orders_total", result.Name);
        Assert.Equal(ConstraintType.Check, result.Type);
        Assert.NotNull(result.CheckExpression);
        Assert.Equal("total >= 0 AND total <= 1000000", result.CheckExpression);
    }

    [Fact]
    public void Extract_CheckWithParenthesesInExpression_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_users_deleted CHECK ((is_deleted = FALSE AND deleted_at IS NULL) OR (is_deleted = TRUE AND deleted_at IS NOT NULL));
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("chk_users_deleted", result.Name);
        Assert.Equal(ConstraintType.Check, result.Type);
        Assert.NotNull(result.CheckExpression);
        Assert.Contains("is_deleted", result.CheckExpression);
        Assert.Contains("deleted_at", result.CheckExpression);
    }

    #endregion

    #region Extract DEFERRABLE Tests

    [Fact]
    public void Extract_ForeignKeyDeferrable_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                DEFERRABLE;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsDeferrable);
        Assert.False(result.IsInitiallyDeferred);
    }

    [Fact]
    public void Extract_ForeignKeyInitiallyDeferred_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                DEFERRABLE INITIALLY DEFERRED;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsDeferrable);
        Assert.True(result.IsInitiallyDeferred);
    }

    [Fact]
    public void Extract_CheckDeferrable_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_users_balance
                CHECK (balance >= 0)
                DEFERRABLE;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsDeferrable);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_ConstraintUpperCase_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE USERS
                ADD CONSTRAINT PK_USERS PRIMARY KEY (ID);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PK_USERS", result.Name);
        Assert.Equal("USERS", result.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Type);
    }

    [Fact]
    public void Extract_ConstraintMixedCase_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            alter table Users
                add constraint pk_Users primary key (Id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pk_Users", result.Name);
        Assert.Equal("Users", result.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Type);
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_InvalidSql_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("INVALID SQL STATEMENT");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_NullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_EmptyBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_ConstraintWithoutName_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_RealWorldPrimaryKey_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT users_pkey PRIMARY KEY (id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("users_pkey", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(ConstraintType.PrimaryKey, result.Type);
        Assert.Single(result.Columns);
    }

    [Fact]
    public void Extract_RealWorldForeignKey_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE orders
                ADD CONSTRAINT fk_orders_user
                FOREIGN KEY (user_id)
                REFERENCES users (id)
                ON DELETE RESTRICT
                ON UPDATE CASCADE;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fk_orders_user", result.Name);
        Assert.Equal("orders", result.TableName);
        Assert.Equal(ConstraintType.ForeignKey, result.Type);
        Assert.Single(result.Columns);
        Assert.Contains("user_id", result.Columns);
        Assert.Equal("users", result.ReferencedTable);
        Assert.Equal(ReferentialAction.Restrict, result.OnDelete);
        Assert.Equal(ReferentialAction.Cascade, result.OnUpdate);
    }

    [Fact]
    public void Extract_RealWorldUniqueConstraint_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT users_username_key UNIQUE (username);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("users_username_key", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(ConstraintType.Unique, result.Type);
        Assert.Single(result.Columns);
        Assert.Contains("username", result.Columns);
    }

    [Fact]
    public void Extract_RealWorldCheckConstraint_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            ALTER TABLE users
                ADD CONSTRAINT chk_balance CHECK (balance >= 0);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("chk_balance", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(ConstraintType.Check, result.Type);
        Assert.NotNull(result.CheckExpression);
        Assert.Equal("balance >= 0", result.CheckExpression);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Создает SQL блок для тестирования
    /// </summary>
    private static SqlBlock CreateBlock(string content)
    {
        return new SqlBlock
        {
            Content = content,
            RawContent = content,
            HeaderComment = null,
            InlineComments = null,
            StartLine = 1,
            EndLine = content.Split('\n').Length,
            SourcePath = "test.sql"
        };
    }

    #endregion
}
