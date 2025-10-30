using PgCs.SchemaAnalyzer.Tante;
using PgCs.Core.Schema.Common;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Интеграционные тесты для SchemaAnalyzer с извлечением таблиц
/// </summary>
public sealed class SchemaAnalyzerTableTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    #region ExtractTables Tests

    [Fact]
    public void ExtractTables_WithSimpleTable_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
            CREATE TABLE products (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                price NUMERIC(10,2)
            );
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        var table = tables[0];
        Assert.Equal("products", table.Name);
        Assert.Equal(3, table.Columns.Count);
    }

    [Fact]
    public void ExtractTables_WithMultipleTables_ExtractsAll()
    {
        // Arrange
        var sql = @"
            CREATE TABLE users (
                id BIGSERIAL PRIMARY KEY,
                username VARCHAR(50)
            );

            CREATE TABLE orders (
                id BIGSERIAL PRIMARY KEY,
                user_id BIGINT
            );

            CREATE TABLE products (
                id SERIAL PRIMARY KEY
            );
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Equal(3, tables.Count);
        Assert.Contains(tables, t => t.Name == "users");
        Assert.Contains(tables, t => t.Name == "orders");
        Assert.Contains(tables, t => t.Name == "products");
    }

    [Fact]
    public void ExtractTables_WithTableAndEnum_ExtractsOnlyTable()
    {
        // Arrange
        var sql = @"
            CREATE TYPE status AS ENUM ('active', 'inactive');

            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                status status NOT NULL
            );
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        Assert.Equal("users", tables[0].Name);
    }

    [Fact]
    public void ExtractTables_WithTemporaryTable_SetsTemporaryFlag()
    {
        // Arrange
        var sql = @"
            CREATE TEMPORARY TABLE temp_calculations (
                id SERIAL,
                result NUMERIC
            );
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        Assert.True(tables[0].IsTemporary);
    }

    [Fact]
    public void ExtractTables_WithPartitionedTable_ExtractsPartitionInfo()
    {
        // Arrange
        var sql = @"
            CREATE TABLE measurements (
                id BIGSERIAL,
                measured_at TIMESTAMP NOT NULL,
                value NUMERIC
            ) PARTITION BY RANGE (measured_at);
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        var table = tables[0];
        Assert.True(table.IsPartitioned);
        Assert.NotNull(table.PartitionInfo);
        Assert.Equal(PartitionStrategy.Range, table.PartitionInfo.Strategy);
    }

    [Fact]
    public void ExtractTables_WithPartitionTable_ExtractsParentTable()
    {
        // Arrange
        var sql = @"
            CREATE TABLE logs_2024_q1 PARTITION OF audit_logs
            FOR VALUES FROM ('2024-01-01') TO ('2024-04-01');
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        var table = tables[0];
        Assert.True(table.IsPartition);
        Assert.Equal("audit_logs", table.ParentTableName);
    }

    [Fact]
    public void ExtractTables_WithInheritance_ExtractsParentTables()
    {
        // Arrange
        var sql = @"
            CREATE TABLE employees (
                id SERIAL,
                employee_number VARCHAR(20)
            ) INHERITS (persons);
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        var table = tables[0];
        Assert.NotNull(table.InheritsFrom);
        Assert.Contains("persons", table.InheritsFrom);
    }

    [Fact]
    public void ExtractTables_WithComplexColumns_ExtractsColumnDetails()
    {
        // Arrange
        var sql = @"
            CREATE TABLE complex_table (
                id BIGSERIAL PRIMARY KEY,
                email VARCHAR(255) NOT NULL UNIQUE,
                balance NUMERIC(12,2) DEFAULT 0.00,
                tags TEXT[],
                metadata JSONB,
                is_active BOOLEAN NOT NULL DEFAULT TRUE
            );
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        var table = tables[0];
        Assert.Equal(6, table.Columns.Count);
        
        var email = table.Columns.FirstOrDefault(c => c.Name == "email");
        Assert.NotNull(email);
        Assert.True(email.IsUnique);
        Assert.False(email.IsNullable);
        
        var balance = table.Columns.FirstOrDefault(c => c.Name == "balance");
        Assert.NotNull(balance);
        Assert.Equal(12, balance.NumericPrecision);
        Assert.Equal(2, balance.NumericScale);
        
        var tags = table.Columns.FirstOrDefault(c => c.Name == "tags");
        Assert.NotNull(tags);
        Assert.True(tags.IsArray);
    }

    [Fact]
    public void ExtractTables_WithSchemaQualified_ExtractsSchema()
    {
        // Arrange
        var sql = @"
            CREATE TABLE app.users (
                id SERIAL PRIMARY KEY
            );

            CREATE TABLE public.logs (
                id BIGINT
            );
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Equal(2, tables.Count);
        
        var appUsers = tables.FirstOrDefault(t => t.Name == "users");
        Assert.NotNull(appUsers);
        Assert.Equal("app", appUsers.Schema);
        
        var publicLogs = tables.FirstOrDefault(t => t.Name == "logs");
        Assert.NotNull(publicLogs);
        Assert.Equal("public", publicLogs.Schema);
    }

    [Fact]
    public void ExtractTables_WithEmptyScript_ThrowsException()
    {
        // Arrange
        var sql = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractTables(sql));
    }

    [Fact]
    public void ExtractTables_WithOnlyComments_ReturnsEmptyList()
    {
        // Arrange
        var sql = @"
            -- This is a comment
            -- Another comment
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Empty(tables);
    }

    [Fact]
    public void ExtractTables_WithNullScript_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractTables(null!));
    }

    [Fact]
    public void ExtractTables_WithWhitespaceScript_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractTables("   "));
    }

    #endregion

    #region Real World Example Tests

    [Fact]
    public void ExtractTables_WithRealUsersTable_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
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
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        var table = tables[0];
        Assert.Equal("users", table.Name);
        Assert.Equal(11, table.Columns.Count);
        
        // Проверка id колонки
        var id = table.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(id);
        Assert.True(id.IsPrimaryKey);
        Assert.True(id.IsIdentity);
        
        // Проверка username колонки
        var username = table.Columns.FirstOrDefault(c => c.Name == "username");
        Assert.NotNull(username);
        Assert.True(username.IsUnique);
        Assert.False(username.IsNullable);
        Assert.Equal(50, username.MaxLength);
        
        // Проверка массива
        var phoneNumbers = table.Columns.FirstOrDefault(c => c.Name == "phone_numbers");
        Assert.NotNull(phoneNumbers);
        Assert.True(phoneNumbers.IsArray);
    }

    [Fact]
    public void ExtractTables_WithRealPartitionedTable_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"
            CREATE TABLE audit_logs (
                id BIGSERIAL,
                user_id BIGINT,
                action VARCHAR(50) NOT NULL,
                entity_type VARCHAR(50) NOT NULL,
                entity_id BIGINT,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (id, created_at)
            ) PARTITION BY RANGE (created_at);
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Single(tables);
        var table = tables[0];
        Assert.Equal("audit_logs", table.Name);
        Assert.True(table.IsPartitioned);
        Assert.NotNull(table.PartitionInfo);
        Assert.Equal(PartitionStrategy.Range, table.PartitionInfo.Strategy);
        Assert.Contains("created_at", table.PartitionInfo.PartitionKeys);
    }

    #endregion

    #region Mixed Schema Tests

    [Fact]
    public void ExtractTables_WithMixedObjects_ExtractsOnlyTables()
    {
        // Arrange
        var sql = @"
            CREATE TYPE user_status AS ENUM ('active', 'inactive');

            CREATE TYPE address AS (
                street VARCHAR(255),
                city VARCHAR(100)
            );

            CREATE TABLE users (
                id BIGSERIAL PRIMARY KEY,
                status user_status,
                address address
            );

            CREATE INDEX idx_users_status ON users (status);

            CREATE TABLE orders (
                id BIGSERIAL PRIMARY KEY,
                user_id BIGINT
            );
        ";

        // Act
        var tables = _analyzer.ExtractTables(sql);

        // Assert
        Assert.Equal(2, tables.Count);
        Assert.Contains(tables, t => t.Name == "users");
        Assert.Contains(tables, t => t.Name == "orders");
    }

    #endregion
}
