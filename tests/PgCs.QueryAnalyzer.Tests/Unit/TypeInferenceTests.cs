using PgCs.QueryAnalyzer.Parsing;

namespace PgCs.QueryAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для TypeInference - вывод типов параметров и колонок
/// </summary>
public sealed class TypeInferenceTests
{
    [Fact]
    public void InferParameterType_WithIntCast_ReturnsIntegerType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE id = $id::int";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "id");

        // Assert
        Assert.Equal("integer", pgType);
        Assert.Equal("int", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithBigIntCast_ReturnsBigIntType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE id = $id::bigint";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "id");

        // Assert
        Assert.Equal("bigint", pgType);
        Assert.Equal("long", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithTimestampCast_ReturnsDateTimeType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE created_at > $date::timestamp";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "date");

        // Assert
        Assert.Equal("timestamp", pgType);
        Assert.Equal("DateTime", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithBooleanCast_ReturnsBoolType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE active = $active::boolean";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "active");

        // Assert
        Assert.Equal("boolean", pgType);
        Assert.Equal("bool", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithBoolCast_ReturnsBoolType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE active = $active::bool";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "active");

        // Assert
        Assert.Equal("boolean", pgType);
        Assert.Equal("bool", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithUuidCast_ReturnsGuidType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE uuid = $id::uuid";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "id");

        // Assert
        Assert.Equal("uuid", pgType);
        Assert.Equal("Guid", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithDecimalCast_ReturnsDecimalType()
    {
        // Arrange
        const string sql = "SELECT * FROM products WHERE price = $price::decimal";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "price");

        // Assert
        Assert.Equal("numeric", pgType);
        Assert.Equal("decimal", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithNumericCast_ReturnsDecimalType()
    {
        // Arrange
        const string sql = "SELECT * FROM products WHERE price = $price::numeric";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "price");

        // Assert
        Assert.Equal("numeric", pgType);
        Assert.Equal("decimal", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithNoCast_ReturnsTextType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE username = $username";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "username");

        // Assert
        Assert.Equal("text", pgType);
        Assert.Equal("string", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_CaseInsensitive_ReturnsCorrectType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE id = $ID::INT";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "id");

        // Assert
        Assert.Equal("integer", pgType);
        Assert.Equal("int", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferParameterType_WithAtSign_ReturnsCorrectType()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE id = @id::int";

        // Act
        var (pgType, csType, isNullable) = TypeInference.InferParameterType(sql, "id");

        // Assert
        Assert.Equal("integer", pgType);
        Assert.Equal("int", csType);
        Assert.False(isNullable);
    }

    [Fact]
    public void InferColumnType_WithCountAggregate_ReturnsLongType()
    {
        // Arrange
        const string expression = "COUNT(*)";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("bigint", pgType);
        Assert.Equal("long", csType);
    }

    [Fact]
    public void InferColumnType_WithSumAggregate_ReturnsLongType()
    {
        // Arrange
        const string expression = "SUM(total)";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("bigint", pgType);
        Assert.Equal("long", csType);
    }

    [Fact]
    public void InferColumnType_WithAvgAggregate_ReturnsDecimalType()
    {
        // Arrange
        const string expression = "AVG(price)";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("numeric", pgType);
        Assert.Equal("decimal", csType);
    }

    [Fact]
    public void InferColumnType_WithNowFunction_ReturnsDateTimeType()
    {
        // Arrange
        const string expression = "NOW()";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("timestamp", pgType);
        Assert.Equal("DateTime", csType);
    }

    [Fact]
    public void InferColumnType_WithCurrentTimestamp_ReturnsDateTimeType()
    {
        // Arrange
        const string expression = "CURRENT_TIMESTAMP";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("timestamp", pgType);
        Assert.Equal("DateTime", csType);
    }

    [Fact]
    public void InferColumnType_WithIntCast_ReturnsIntType()
    {
        // Arrange
        const string expression = "id::int";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("integer", pgType);
        Assert.Equal("int", csType);
    }

    [Fact]
    public void InferColumnType_WithBigIntCast_ReturnsLongType()
    {
        // Arrange
        const string expression = "id::bigint";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("bigint", pgType);
        Assert.Equal("long", csType);
    }

    [Fact]
    public void InferColumnType_WithBooleanCast_ReturnsBoolType()
    {
        // Arrange
        const string expression = "active::boolean";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("boolean", pgType);
        Assert.Equal("bool", csType);
    }

    [Fact]
    public void InferColumnType_WithBoolCast_ReturnsBoolType()
    {
        // Arrange
        const string expression = "active::bool";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("boolean", pgType);
        Assert.Equal("bool", csType);
    }

    [Fact]
    public void InferColumnType_WithUuidCast_ReturnsGuidType()
    {
        // Arrange
        const string expression = "uuid::uuid";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("uuid", pgType);
        Assert.Equal("Guid", csType);
    }

    [Fact]
    public void InferColumnType_WithNoTypeInfo_ReturnsTextType()
    {
        // Arrange
        const string expression = "username";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("text", pgType);
        Assert.Equal("string", csType);
    }

    [Fact]
    public void InferColumnType_CaseInsensitive_ReturnsCorrectType()
    {
        // Arrange
        const string expression = "count(*)";

        // Act
        var (pgType, csType) = TypeInference.InferColumnType(expression);

        // Assert
        Assert.Equal("bigint", pgType);
        Assert.Equal("long", csType);
    }
}
