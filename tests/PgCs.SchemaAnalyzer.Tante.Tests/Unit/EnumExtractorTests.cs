using PgCs.Core.Extraction.Block;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для EnumExtractor
/// </summary>
public sealed class EnumExtractorTests
{
    private readonly IEnumExtractor _extractor = new EnumExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidEnumBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock("CREATE TYPE status AS ENUM ('active', 'inactive');");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id INT PRIMARY KEY);");

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

    #region Extract Simple Enum Tests

    [Fact]
    public void Extract_SimpleEnum_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'inactive', 'pending');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("status", result.Name);
        Assert.Null(result.Schema);
        Assert.Equal(3, result.Values.Count);
        Assert.Equal("active", result.Values[0]);
        Assert.Equal("inactive", result.Values[1]);
        Assert.Equal("pending", result.Values[2]);
    }

    [Fact]
    public void Extract_EnumWithSchema_ReturnsDefinitionWithSchema()
    {
        // Arrange
        var sql = "CREATE TYPE public.order_status AS ENUM ('new', 'processing', 'completed');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("order_status", result.Name);
        Assert.Equal("public", result.Schema);
        Assert.Equal(3, result.Values.Count);
    }

    [Fact]
    public void Extract_EnumWithMultilineFormat_ReturnsValidDefinition()
    {
        // Arrange
        var sql = @"CREATE TYPE user_status AS ENUM (
    'active',
    'inactive',
    'suspended',
    'deleted'
);";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user_status", result.Name);
        Assert.Equal(4, result.Values.Count);
        Assert.Equal("active", result.Values[0]);
        Assert.Equal("deleted", result.Values[3]);
    }

    #endregion

    #region Extract with Comments Tests

    [Fact]
    public void Extract_EnumWithHeaderComment_PreservesComment()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'inactive');";
        var block = new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            HeaderComment = "User account status enumeration",
            StartLine = 1,
            EndLine = 1
        };

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("User account status enumeration", result.SqlComment);
    }

    [Fact]
    public void Extract_EnumBlock_PreservesRawSql()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active', 'inactive');";
        var rawSql = "-- Status type\nCREATE TYPE status AS ENUM ('active', 'inactive');";
        var block = new SqlBlock
        {
            Content = sql,
            RawContent = rawSql,
            StartLine = 1,
            EndLine = 2
        };

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rawSql, result.RawSql);
    }

    #endregion

    #region Extract Special Cases Tests

    [Fact]
    public void Extract_EnumWithSpacesInValues_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM (  'active'  ,  'inactive'  ,  'pending'  );";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Values.Count);
        Assert.Equal("active", result.Values[0]);
    }

    [Fact]
    public void Extract_EnumWithEmptyValues_ReturnsNull()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ();";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_EnumWithSingleValue_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('active');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Values);
        Assert.Equal("active", result.Values[0]);
    }

    [Fact]
    public void Extract_EnumWithSpecialCharactersInValues_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE status AS ENUM ('in-progress', 'not_started', 'done!');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Values.Count);
        Assert.Equal("in-progress", result.Values[0]);
        Assert.Equal("not_started", result.Values[1]);
        Assert.Equal("done!", result.Values[2]);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_EnumWithUpperCaseKeywords_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE STATUS AS ENUM ('ACTIVE', 'INACTIVE');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("STATUS", result.Name);
        Assert.Equal(2, result.Values.Count);
    }

    [Fact]
    public void Extract_EnumWithMixedCaseKeywords_ExtractsCorrectly()
    {
        // Arrange
        var sql = "Create Type status As Enum ('active', 'inactive');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("status", result.Name);
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
    public void Extract_NonEnumBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE TABLE users (id INT PRIMARY KEY);");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_InvalidEnumSyntax_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("CREATE TYPE status ENUM ('active', 'inactive');"); // Missing AS

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_PostgreSQL18EnumExample_ExtractsCorrectly()
    {
        // Arrange - Пример из реального Schema.sql
        var sql = "CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended', 'deleted');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user_status", result.Name);
        Assert.Equal(4, result.Values.Count);
        Assert.Equal("active", result.Values[0]);
        Assert.Equal("inactive", result.Values[1]);
        Assert.Equal("suspended", result.Values[2]);
        Assert.Equal("deleted", result.Values[3]);
    }

    [Fact]
    public void Extract_OrderStatusEnum_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("order_status", result.Name);
        Assert.Equal(5, result.Values.Count);
        Assert.Contains("pending", result.Values);
        Assert.Contains("delivered", result.Values);
    }

    [Fact]
    public void Extract_PaymentMethodEnum_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE payment_method AS ENUM ('credit_card', 'debit_card', 'paypal', 'bank_transfer', 'crypto', 'cash');";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("payment_method", result.Name);
        Assert.Equal(6, result.Values.Count);
    }

    #endregion

    #region Helper Methods

    private static SqlBlock CreateBlock(string sql)
    {
        return new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            StartLine = 1,
            EndLine = 1
        };
    }

    #endregion
}
