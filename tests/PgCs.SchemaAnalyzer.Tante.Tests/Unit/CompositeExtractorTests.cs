using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для CompositeExtractor
/// </summary>
public sealed class CompositeExtractorTests
{
    private readonly IExtractor<CompositeTypeDefinition> _extractor = new CompositeExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidCompositeBlock_ReturnsTrue()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100));");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithEnumBlock_ReturnsFalse()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TYPE status AS ENUM ('active', 'inactive');");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);");

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

    #region Extract Simple Composite Tests

    [Fact]
    public void Extract_SimpleComposite_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100), zip_code VARCHAR(20));";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("address", result.Definition.Name);
        Assert.Null(result.Definition.Schema);
        Assert.Equal(3, result.Definition.Attributes.Count);
        Assert.Equal("street", result.Definition.Attributes[0].Name);
        Assert.Equal("VARCHAR", result.Definition.Attributes[0].DataType);
        Assert.Equal(255, result.Definition.Attributes[0].MaxLength);
    }

    [Fact]
    public void Extract_CompositeWithSchema_ReturnsDefinitionWithSchema()
    {
        // Arrange
        var sql = "CREATE TYPE public.contact_info AS (phone VARCHAR(20), email VARCHAR(255));";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("contact_info", result.Definition.Name);
        Assert.Equal("public", result.Definition.Schema);
        Assert.Equal(2, result.Definition.Attributes.Count);
    }

    [Fact]
    public void Extract_CompositeWithMultilineFormat_ReturnsValidDefinition()
    {
        // Arrange
        var sql = @"CREATE TYPE address AS (
    street VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(50),
    zip_code VARCHAR(20),
    country VARCHAR(50)
);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("address", result.Definition.Name);
        Assert.Equal(5, result.Definition.Attributes.Count);
        Assert.Equal("street", result.Definition.Attributes[0].Name);
        Assert.Equal("country", result.Definition.Attributes[4].Name);
    }

    #endregion

    #region Extract with Different Data Types Tests

    [Fact]
    public void Extract_CompositeWithNumericTypes_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE product_price AS (amount NUMERIC(12, 2), currency VARCHAR(3));";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(2, result.Definition.Attributes.Count);
        
        var amountAttr = result.Definition.Attributes[0];
        Assert.Equal("amount", amountAttr.Name);
        Assert.Equal("NUMERIC", amountAttr.DataType);
        Assert.Equal(12, amountAttr.NumericPrecision);
        Assert.Equal(2, amountAttr.NumericScale);
    }

    [Fact]
    public void Extract_CompositeWithArrayTypes_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE tags_info AS (tag_names VARCHAR(50)[], tag_counts INTEGER[]);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(2, result.Definition.Attributes.Count);
        
        var tagNamesAttr = result.Definition.Attributes[0];
        Assert.Equal("tag_names", tagNamesAttr.Name);
        Assert.Equal("VARCHAR", tagNamesAttr.DataType);
        Assert.True(tagNamesAttr.IsArray);
        
        var tagCountsAttr = result.Definition.Attributes[1];
        Assert.Equal("tag_counts", tagCountsAttr.Name);
        Assert.Equal("INTEGER", tagCountsAttr.DataType);
        Assert.True(tagCountsAttr.IsArray);
    }

    [Fact]
    public void Extract_CompositeWithMixedTypes_ExtractsCorrectly()
    {
        // Arrange
        var sql = "CREATE TYPE user_data AS (id INTEGER, name TEXT, created_at TIMESTAMP, is_active BOOLEAN);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(4, result.Definition.Attributes.Count);
        Assert.Equal("INTEGER", result.Definition.Attributes[0].DataType);
        Assert.Equal("TEXT", result.Definition.Attributes[1].DataType);
        Assert.Equal("TIMESTAMP", result.Definition.Attributes[2].DataType);
        Assert.Equal("BOOLEAN", result.Definition.Attributes[3].DataType);
    }

    #endregion

    #region Extract with Comments Tests

    [Fact]
    public void Extract_CompositeWithHeaderComment_PreservesComment()
    {
        // Arrange
        var sql = "CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100));";
        var blocks = (IReadOnlyList<SqlBlock>)[new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            HeaderComment = "Address type for user locations",
            StartLine = 1,
            EndLine = 1
        }];

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("Address type for user locations", result.Definition.SqlComment);
    }

    [Fact]
    public void Extract_CompositeBlock_PreservesRawSql()
    {
        // Arrange
        var sql = "CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100));";
        var rawSql = "-- Address type\nCREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100));";
        var blocks = (IReadOnlyList<SqlBlock>)[new SqlBlock
        {
            Content = sql,
            RawContent = rawSql,
            StartLine = 1,
            EndLine = 2
        }];

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(rawSql, result.Definition.RawSql);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Extract_CompositeWithEmptyAttributes_ReturnsFailure()
    {
        // Arrange
        var sql = "CREATE TYPE empty_type AS ();";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
        Assert.Contains(result.ValidationIssues, i => i.Code == "COMPOSITE_EMPTY_ATTRIBUTES");
    }

    [Fact]
    public void Extract_CompositeWithSingleAttribute_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "CREATE TYPE simple_type AS (value INTEGER);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Single(result.Definition.Attributes);
        Assert.Equal("value", result.Definition.Attributes[0].Name);
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
    public void Extract_NonCompositeBlock_ReturnsNotApplicable()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_AddressTypeFromSchema_ExtractsCorrectly()
    {
        // Arrange - Пример из реального Schema.sql
        var sql = @"CREATE TYPE address AS (
    street VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(50),
    zip_code VARCHAR(20),
    country VARCHAR(50)
);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("address", result.Definition.Name);
        Assert.Equal(5, result.Definition.Attributes.Count);
        
        Assert.Equal("street", result.Definition.Attributes[0].Name);
        Assert.Equal("VARCHAR", result.Definition.Attributes[0].DataType);
        Assert.Equal(255, result.Definition.Attributes[0].MaxLength);
        
        Assert.Equal("zip_code", result.Definition.Attributes[3].Name);
    }

    [Fact]
    public void Extract_ContactInfoTypeFromSchema_ExtractsCorrectly()
    {
        // Arrange
        var sql = @"CREATE TYPE contact_info AS (
    phone VARCHAR(20),
    email VARCHAR(255),
    telegram VARCHAR(50),
    preferred_method VARCHAR(20)
);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("contact_info", result.Definition.Name);
        Assert.Equal(4, result.Definition.Attributes.Count);
        Assert.Contains(result.Definition.Attributes, a => a.Name == "phone");
        Assert.Contains(result.Definition.Attributes, a => a.Name == "telegram");
    }

    #endregion

    #region Helper Methods

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql)
    {
        return [new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            StartLine = 1,
            EndLine = 1
        }];
    }

    #endregion
}
