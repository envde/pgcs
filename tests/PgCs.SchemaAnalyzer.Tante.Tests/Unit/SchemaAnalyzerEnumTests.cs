namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для SchemaAnalyzer - метод ExtractEnums
/// </summary>
public sealed class SchemaAnalyzerEnumTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    #region ExtractEnums Basic Tests

    [Fact]
    public void ExtractEnums_WithSingleEnum_ReturnsOneDefinition()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'inactive');";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Single(result);
        Assert.Equal("status", result[0].Name);
        Assert.Equal(2, result[0].Values.Count);
    }

    [Fact]
    public void ExtractEnums_WithMultipleEnums_ReturnsAllDefinitions()
    {
        // Arrange
        var sql = @"
CREATE TYPE status AS ENUM ('active', 'inactive');
CREATE TYPE priority AS ENUM ('low', 'medium', 'high');
CREATE TYPE color AS ENUM ('red', 'green', 'blue');
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("status", result[0].Name);
        Assert.Equal("priority", result[1].Name);
        Assert.Equal("color", result[2].Name);
    }

    [Fact]
    public void ExtractEnums_WithEmptySql_ReturnsEmptyList()
    {
        // Arrange
        var sql = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractEnums(sql));
    }

    [Fact]
    public void ExtractEnums_WithNullSql_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _analyzer.ExtractEnums(null!));
    }

    [Fact]
    public void ExtractEnums_WithWhitespaceSql_ThrowsArgumentException()
    {
        // Arrange
        var sql = "   \n\t  ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _analyzer.ExtractEnums(sql));
    }

    #endregion

    #region ExtractEnums Mixed Content Tests

    [Fact]
    public void ExtractEnums_WithMixedSqlObjects_ReturnsOnlyEnums()
    {
        // Arrange
        var sql = @"
CREATE TABLE users (id INT PRIMARY KEY);

CREATE TYPE status AS ENUM ('active', 'inactive');

CREATE VIEW active_users AS SELECT * FROM users WHERE status = 'active';

CREATE TYPE priority AS ENUM ('low', 'high');
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.NotNull(e.Name));
        Assert.Contains(result, e => e.Name == "status");
        Assert.Contains(result, e => e.Name == "priority");
    }

    [Fact]
    public void ExtractEnums_WithComments_PreservesComments()
    {
        // Arrange
        var sql = @"
-- User status enumeration
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended');

-- Order priority levels
CREATE TYPE priority AS ENUM ('low', 'medium', 'high');
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("User status enumeration", result[0].SqlComment);
        Assert.Equal("Order priority levels", result[1].SqlComment);
    }

    #endregion

    #region ExtractEnums Schema Qualified Tests

    [Fact]
    public void ExtractEnums_WithSchemaQualifiedNames_ExtractsSchemas()
    {
        // Arrange
        var sql = @"
CREATE TYPE public.status AS ENUM ('active', 'inactive');
CREATE TYPE app.priority AS ENUM ('low', 'high');
CREATE TYPE admin.role AS ENUM ('user', 'admin');
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("public", result[0].Schema);
        Assert.Equal("app", result[1].Schema);
        Assert.Equal("admin", result[2].Schema);
    }

    #endregion

    #region ExtractEnums Formatting Tests

    [Fact]
    public void ExtractEnums_WithDifferentFormats_HandlesAllCorrectly()
    {
        // Arrange
        var sql = @"
-- Single line
CREATE TYPE status1 AS ENUM ('a', 'b');

-- Multi-line
CREATE TYPE status2 AS ENUM (
    'x',
    'y',
    'z'
);

-- Compact
CREATE TYPE status3 AS ENUM('one','two','three');

-- With extra spaces
CREATE TYPE status4 AS ENUM (  'up'  ,  'down'  );
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.All(result, e => Assert.NotEmpty(e.Values));
    }

    #endregion

    #region ExtractEnums Real World Examples

    [Fact]
    public void ExtractEnums_PostgreSQL18Example_ExtractsAllEnums()
    {
        // Arrange - Реальный пример из Schema.sql
        var sql = @"
-- ENUM для статуса пользователя
CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');

-- ENUM для статуса заказа
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');

-- ENUM для способов оплаты
CREATE TYPE payment_method AS ENUM ('credit_card', 'debit_card', 'paypal', 'bank_transfer', 'crypto', 'cash');

-- ENUM для уровня приоритета
CREATE TYPE priority_level AS ENUM ('low', 'medium', 'high', 'urgent');
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Equal(4, result.Count);
        
        var userStatus = result.First(e => e.Name == "user_status");
        Assert.Equal(4, userStatus.Values.Count);
        Assert.Contains("suspended", userStatus.Values);
        
        var orderStatus = result.First(e => e.Name == "order_status");
        Assert.Equal(5, orderStatus.Values.Count);
        
        var paymentMethod = result.First(e => e.Name == "payment_method");
        Assert.Equal(6, paymentMethod.Values.Count);
        
        var priorityLevel = result.First(e => e.Name == "priority_level");
        Assert.Equal(4, priorityLevel.Values.Count);
    }

    #endregion

    #region ExtractEnums No Enums Tests

    [Fact]
    public void ExtractEnums_SqlWithoutEnums_ReturnsEmptyList()
    {
        // Arrange
        var sql = @"
CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(100));
CREATE VIEW active_users AS SELECT * FROM users;
CREATE FUNCTION get_user(id INT) RETURNS TABLE(name VARCHAR) AS $$ SELECT name FROM users WHERE id = $1 $$ LANGUAGE SQL;
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractEnums_OnlyComments_ReturnsEmptyList()
    {
        // Arrange
        var sql = @"
-- This is a comment
-- Another comment
-- More comments
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region ExtractEnums Order Preservation Tests

    [Fact]
    public void ExtractEnums_PreservesDefinitionOrder()
    {
        // Arrange
        var sql = @"
CREATE TYPE first AS ENUM ('a');
CREATE TYPE second AS ENUM ('b');
CREATE TYPE third AS ENUM ('c');
CREATE TYPE fourth AS ENUM ('d');
";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal("first", result[0].Name);
        Assert.Equal("second", result[1].Name);
        Assert.Equal("third", result[2].Name);
        Assert.Equal("fourth", result[3].Name);
    }

    [Fact]
    public void ExtractEnums_PreservesValueOrder()
    {
        // Arrange
        var sql = "CREATE TYPE ordered AS ENUM ('first', 'second', 'third', 'fourth', 'fifth');";

        // Act
        var result = _analyzer.ExtractEnums(sql);

        // Assert
        Assert.Single(result);
        var values = result[0].Values;
        Assert.Equal(5, values.Count);
        Assert.Equal("first", values[0]);
        Assert.Equal("second", values[1]);
        Assert.Equal("third", values[2]);
        Assert.Equal("fourth", values[3]);
        Assert.Equal("fifth", values[4]);
    }

    #endregion
}
