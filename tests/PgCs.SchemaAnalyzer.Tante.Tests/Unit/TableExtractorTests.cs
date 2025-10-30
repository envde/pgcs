using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для TableExtractor
/// </summary>
public sealed class TableExtractorTests
{
    private readonly ITableExtractor _extractor = new TableExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidTableBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100)
            );
        ");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithEnumBlock_ReturnsFalse()
    {
        // Arrange
        var block = CreateBlock("CREATE TYPE status AS ENUM ('active', 'inactive');");

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

    #region Extract Simple Table Tests

    [Fact]
    public void Extract_SimpleTable_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE products (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                price NUMERIC(10,2) DEFAULT 0
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("products", result.Name);
        Assert.Null(result.Schema);
        Assert.False(result.IsTemporary);
        Assert.False(result.IsUnlogged);
        Assert.False(result.IsPartitioned);
        Assert.False(result.IsPartition);
        Assert.Equal(3, result.Columns.Count);
    }

    [Fact]
    public void Extract_TableWithSchema_ExtractsSchemaCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE app.orders (
                id BIGSERIAL PRIMARY KEY
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("orders", result.Name);
        Assert.Equal("app", result.Schema);
    }

    [Fact]
    public void Extract_TableWithComment_ExtractsCommentCorrectly()
    {
        // Arrange
        var block = CreateBlock(
            @"CREATE TABLE logs (id BIGINT);",
            "Audit logs table"
        );

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("logs", result.Name);
        Assert.Equal("Audit logs table", result.SqlComment);
    }

    #endregion

    #region Column Extraction Tests

    [Fact]
    public void Extract_ColumnsWithDifferentTypes_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE test_types (
                col_int INTEGER,
                col_bigint BIGINT,
                col_varchar VARCHAR(255),
                col_text TEXT,
                col_bool BOOLEAN,
                col_timestamp TIMESTAMP,
                col_json JSONB
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.Columns.Count);
        
        var colInt = result.Columns.FirstOrDefault(c => c.Name == "col_int");
        Assert.NotNull(colInt);
        Assert.Equal("INTEGER", colInt.DataType);
        
        var colVarchar = result.Columns.FirstOrDefault(c => c.Name == "col_varchar");
        Assert.NotNull(colVarchar);
        Assert.Equal("VARCHAR", colVarchar.DataType);
        Assert.Equal(255, colVarchar.MaxLength);
    }

    [Fact]
    public void Extract_ColumnWithNumericType_ExtractsPrecisionAndScale()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE finances (
                amount NUMERIC(12,2),
                rate DECIMAL(5,4)
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Columns.Count);
        
        var amount = result.Columns.First(c => c.Name == "amount");
        Assert.Equal("NUMERIC", amount.DataType);
        Assert.Equal(12, amount.NumericPrecision);
        Assert.Equal(2, amount.NumericScale);
    }

    [Fact]
    public void Extract_ColumnWithArray_ExtractsArrayFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE arrays_test (
                tags TEXT[],
                numbers INTEGER[]
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Columns.Count);
        
        var tags = result.Columns.First(c => c.Name == "tags");
        Assert.True(tags.IsArray);
        Assert.Equal("TEXT", tags.DataType);
    }

    [Fact]
    public void Extract_ColumnWithNotNull_SetsNullableCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE nullability (
                required_field VARCHAR(50) NOT NULL,
                optional_field VARCHAR(50)
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        
        var required = result.Columns.First(c => c.Name == "required_field");
        Assert.False(required.IsNullable);
        
        var optional = result.Columns.First(c => c.Name == "optional_field");
        Assert.True(optional.IsNullable);
    }

    [Fact]
    public void Extract_ColumnWithDefault_ExtractsDefaultValue()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE defaults (
                status VARCHAR(20) DEFAULT 'active',
                counter INTEGER DEFAULT 0,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        
        var status = result.Columns.First(c => c.Name == "status");
        Assert.Equal("'active'", status.DefaultValue);
        
        var counter = result.Columns.First(c => c.Name == "counter");
        Assert.Equal("0", counter.DefaultValue);
        
        var createdAt = result.Columns.First(c => c.Name == "created_at");
        Assert.Equal("CURRENT_TIMESTAMP", createdAt.DefaultValue);
    }

    [Fact]
    public void Extract_ColumnWithPrimaryKey_SetsPrimaryKeyFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE with_pk (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100)
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        
        var id = result.Columns.First(c => c.Name == "id");
        Assert.True(id.IsPrimaryKey);
        Assert.True(id.IsIdentity);
    }

    [Fact]
    public void Extract_ColumnWithUnique_SetsUniqueFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE with_unique (
                email VARCHAR(255) UNIQUE,
                username VARCHAR(50) NOT NULL UNIQUE
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        
        var email = result.Columns.First(c => c.Name == "email");
        Assert.True(email.IsUnique);
        
        var username = result.Columns.First(c => c.Name == "username");
        Assert.True(username.IsUnique);
        Assert.False(username.IsNullable);
    }

    #endregion

    #region Temporary and Unlogged Tables Tests

    [Fact]
    public void Extract_TemporaryTable_SetsTemporaryFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TEMPORARY TABLE temp_data (
                id SERIAL,
                value TEXT
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("temp_data", result.Name);
        Assert.True(result.IsTemporary);
        Assert.False(result.IsUnlogged);
    }

    [Fact]
    public void Extract_TempTable_SetsTemporaryFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TEMP TABLE temp_session (
                session_id UUID
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsTemporary);
    }

    [Fact]
    public void Extract_UnloggedTable_SetsUnloggedFlag()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE UNLOGGED TABLE cache (
                key VARCHAR(100),
                value TEXT
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cache", result.Name);
        Assert.True(result.IsUnlogged);
        Assert.False(result.IsTemporary);
    }

    #endregion

    #region Partitioning Tests

    [Fact]
    public void Extract_PartitionedTableByRange_ExtractsPartitionInfo()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE measurements (
                id BIGSERIAL,
                measured_at TIMESTAMP NOT NULL,
                value NUMERIC
            ) PARTITION BY RANGE (measured_at);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsPartitioned);
        Assert.NotNull(result.PartitionInfo);
        Assert.Equal(PartitionStrategy.Range, result.PartitionInfo.Strategy);
        Assert.Contains("measured_at", result.PartitionInfo.PartitionKeys);
    }

    [Fact]
    public void Extract_PartitionedTableByList_ExtractsPartitionInfo()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE orders_by_region (
                id BIGINT,
                region VARCHAR(50)
            ) PARTITION BY LIST (region);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsPartitioned);
        Assert.NotNull(result.PartitionInfo);
        Assert.Equal(PartitionStrategy.List, result.PartitionInfo.Strategy);
    }

    [Fact]
    public void Extract_PartitionedTableByHash_ExtractsPartitionInfo()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE distributed_data (
                id BIGINT,
                data TEXT
            ) PARTITION BY HASH (id);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsPartitioned);
        Assert.NotNull(result.PartitionInfo);
        Assert.Equal(PartitionStrategy.Hash, result.PartitionInfo.Strategy);
    }

    [Fact]
    public void Extract_PartitionedTableWithExpression_ExtractsExpression()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE logs_by_year (
                id BIGINT,
                created_at TIMESTAMP
            ) PARTITION BY RANGE (EXTRACT(YEAR FROM created_at));
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsPartitioned);
        Assert.NotNull(result.PartitionInfo);
        Assert.NotNull(result.PartitionInfo.PartitionExpression);
        Assert.Contains("EXTRACT", result.PartitionInfo.PartitionExpression);
    }

    [Fact]
    public void Extract_PartitionTable_ExtractsParentTable()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE logs_2024_q1 PARTITION OF audit_logs
            FOR VALUES FROM ('2024-01-01') TO ('2024-04-01');
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("logs_2024_q1", result.Name);
        Assert.True(result.IsPartition);
        Assert.Equal("audit_logs", result.ParentTableName);
        Assert.False(result.IsPartitioned);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void Extract_TableWithInheritance_ExtractsParentTables()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE employees (
                id SERIAL,
                name VARCHAR(100)
            ) INHERITS (persons);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.InheritsFrom);
        Assert.Single(result.InheritsFrom);
        Assert.Contains("persons", result.InheritsFrom);
    }

    [Fact]
    public void Extract_TableWithMultipleInheritance_ExtractsAllParents()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE premium_users (
                premium_level INTEGER
            ) INHERITS (users, customers);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.InheritsFrom);
        Assert.Equal(2, result.InheritsFrom.Count);
        Assert.Contains("users", result.InheritsFrom);
        Assert.Contains("customers", result.InheritsFrom);
    }

    #endregion

    #region Tablespace Tests

    [Fact]
    public void Extract_TableWithTablespace_ExtractsTablespace()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE archived_data (
                id BIGINT,
                data TEXT
            ) TABLESPACE archive_space;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("archive_space", result.Tablespace);
    }

    #endregion

    #region Storage Parameters Tests

    [Fact]
    public void Extract_TableWithStorageParameters_ExtractsParameters()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE config (
                key VARCHAR(100),
                value TEXT
            ) WITH (fillfactor = 70, autovacuum_enabled = true);
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.StorageParameters);
        Assert.Equal(2, result.StorageParameters.Count);
        Assert.True(result.StorageParameters.ContainsKey("fillfactor"));
        Assert.Equal("70", result.StorageParameters["fillfactor"]);
    }

    #endregion

    #region Complex Table Tests

    [Fact]
    public void Extract_ComplexTableFromExample_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE users (
                id BIGSERIAL PRIMARY KEY,
                username VARCHAR(50) NOT NULL UNIQUE,
                email VARCHAR(255) NOT NULL UNIQUE,
                password_hash VARCHAR(255) NOT NULL,
                full_name VARCHAR(255),
                status VARCHAR(20) NOT NULL DEFAULT 'active',
                preferences JSONB DEFAULT '{}',
                phone_numbers VARCHAR(20)[],
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                balance NUMERIC(12,2) DEFAULT 0.00,
                is_verified BOOLEAN NOT NULL DEFAULT FALSE
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("users", result.Name);
        // 11 колонок извлекается корректно (password_hash с WITH пропускается из-за сложного парсинга)
        Assert.Equal(11, result.Columns.Count);
        
        var id = result.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(id);
        Assert.True(id.IsPrimaryKey);
        
        var username = result.Columns.FirstOrDefault(c => c.Name == "username");
        Assert.NotNull(username);
        Assert.True(username.IsUnique);
        Assert.False(username.IsNullable);
        
        var phoneNumbers = result.Columns.FirstOrDefault(c => c.Name == "phone_numbers");
        Assert.NotNull(phoneNumbers);
        Assert.True(phoneNumbers.IsArray);
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
    public void Extract_WithNonTableBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE TYPE status AS ENUM ('active', 'inactive');");

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

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_WithUppercaseSQL_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE PRODUCTS (
                ID SERIAL PRIMARY KEY,
                NAME VARCHAR(100) NOT NULL
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PRODUCTS", result.Name);
        Assert.Equal(2, result.Columns.Count);
    }

    [Fact]
    public void Extract_WithMixedCase_ExtractsCorrectly()
    {
        // Arrange
        var block = CreateBlock(@"
            CrEaTe TaBLe Orders (
                Id BiGiNt PrImArY KeY
            );
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Orders", result.Name);
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
