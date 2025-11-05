using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для CompositeExtractor
/// Покрывает composite типы с различными атрибутами, типы данных (VARCHAR, NUMERIC, TIMESTAMP, массивы), специальные форматы комментариев
/// </summary>
public sealed class CompositeExtractorTests
{
    private readonly IExtractor<CompositeTypeDefinition> _extractor = new CompositeExtractor();

    [Fact]
    public void Extract_BasicComposites_HandlesAllVariants()
    {
        // Покрывает: simple composite, schema, multiline format, single attribute, mixed data types

        // Simple composite with multiple attributes
        var simpleBlock = CreateBlocks("CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100), zip_code VARCHAR(20));");
        var simpleResult = _extractor.Extract(simpleBlock);
        
        Assert.True(simpleResult.IsSuccess);
        Assert.NotNull(simpleResult.Definition);
        Assert.Equal("address", simpleResult.Definition.Name);
        Assert.Null(simpleResult.Definition.Schema);
        Assert.Equal(3, simpleResult.Definition.Attributes.Count);
        Assert.Equal("street", simpleResult.Definition.Attributes[0].Name);
        Assert.Equal("VARCHAR", simpleResult.Definition.Attributes[0].DataType);
        Assert.Equal(255, simpleResult.Definition.Attributes[0].MaxLength);
        Assert.Equal("city", simpleResult.Definition.Attributes[1].Name);
        Assert.Equal(100, simpleResult.Definition.Attributes[1].MaxLength);
        
        // Composite with schema
        var schemaBlock = CreateBlocks("CREATE TYPE public.contact_info AS (phone VARCHAR(20), email VARCHAR(255));");
        var schemaResult = _extractor.Extract(schemaBlock);
        
        Assert.True(schemaResult.IsSuccess);
        Assert.Equal("contact_info", schemaResult.Definition!.Name);
        Assert.Equal("public", schemaResult.Definition.Schema);
        Assert.Equal(2, schemaResult.Definition.Attributes.Count);
        
        // Multiline format
        var multilineBlock = CreateBlocks(@"CREATE TYPE address AS (
    street VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(50),
    zip_code VARCHAR(20),
    country VARCHAR(50)
);");
        var multilineResult = _extractor.Extract(multilineBlock);
        
        Assert.True(multilineResult.IsSuccess);
        Assert.Equal("address", multilineResult.Definition!.Name);
        Assert.Equal(5, multilineResult.Definition.Attributes.Count);
        Assert.Equal("street", multilineResult.Definition.Attributes[0].Name);
        Assert.Equal("country", multilineResult.Definition.Attributes[4].Name);
        
        // Single attribute
        var singleBlock = CreateBlocks("CREATE TYPE simple_type AS (value INTEGER);");
        var singleResult = _extractor.Extract(singleBlock);
        
        Assert.True(singleResult.IsSuccess);
        Assert.Single(singleResult.Definition!.Attributes);
        Assert.Equal("value", singleResult.Definition.Attributes[0].Name);
        Assert.Equal("INTEGER", singleResult.Definition.Attributes[0].DataType);
        
        // Mixed data types
        var mixedBlock = CreateBlocks("CREATE TYPE user_data AS (id INTEGER, name TEXT, created_at TIMESTAMP, is_active BOOLEAN);");
        var mixedResult = _extractor.Extract(mixedBlock);
        
        Assert.True(mixedResult.IsSuccess);
        Assert.Equal(4, mixedResult.Definition!.Attributes.Count);
        Assert.Equal("INTEGER", mixedResult.Definition.Attributes[0].DataType);
        Assert.Equal("TEXT", mixedResult.Definition.Attributes[1].DataType);
        Assert.Equal("TIMESTAMP", mixedResult.Definition.Attributes[2].DataType);
        Assert.Equal("BOOLEAN", mixedResult.Definition.Attributes[3].DataType);
        
        // CanExtract validation
        Assert.True(_extractor.CanExtract(simpleBlock));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TYPE status AS ENUM ('active', 'inactive');")));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);")));
    }

    [Fact]
    public void Extract_ComplexDataTypes_HandlesCorrectly()
    {
        // Покрывает: NUMERIC with precision/scale, array types, real-world examples

        // Numeric with precision and scale
        var numericBlock = CreateBlocks("CREATE TYPE product_price AS (amount NUMERIC(12, 2), currency VARCHAR(3));");
        var numericResult = _extractor.Extract(numericBlock);
        
        Assert.True(numericResult.IsSuccess);
        Assert.Equal(2, numericResult.Definition!.Attributes.Count);
        
        var amountAttr = numericResult.Definition.Attributes[0];
        Assert.Equal("amount", amountAttr.Name);
        Assert.Equal("NUMERIC", amountAttr.DataType);
        Assert.Equal(12, amountAttr.NumericPrecision);
        Assert.Equal(2, amountAttr.NumericScale);
        
        var currencyAttr = numericResult.Definition.Attributes[1];
        Assert.Equal("currency", currencyAttr.Name);
        Assert.Equal("VARCHAR", currencyAttr.DataType);
        Assert.Equal(3, currencyAttr.MaxLength);
        
        // Array types
        var arrayBlock = CreateBlocks("CREATE TYPE tags_info AS (tag_names VARCHAR(50)[], tag_counts INTEGER[]);");
        var arrayResult = _extractor.Extract(arrayBlock);
        
        Assert.True(arrayResult.IsSuccess);
        Assert.Equal(2, arrayResult.Definition!.Attributes.Count);
        
        var tagNamesAttr = arrayResult.Definition.Attributes[0];
        Assert.Equal("tag_names", tagNamesAttr.Name);
        Assert.Equal("VARCHAR", tagNamesAttr.DataType);
        Assert.Equal(50, tagNamesAttr.MaxLength);
        Assert.True(tagNamesAttr.IsArray);
        
        var tagCountsAttr = arrayResult.Definition.Attributes[1];
        Assert.Equal("tag_counts", tagCountsAttr.Name);
        Assert.Equal("INTEGER", tagCountsAttr.DataType);
        Assert.True(tagCountsAttr.IsArray);
        
        // Real-world: Address type
        var addressBlock = CreateBlocks(@"CREATE TYPE address AS (
    street VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(50),
    zip_code VARCHAR(20),
    country VARCHAR(50)
);");
        var addressResult = _extractor.Extract(addressBlock);
        
        Assert.True(addressResult.IsSuccess);
        Assert.Equal("address", addressResult.Definition!.Name);
        Assert.Equal(5, addressResult.Definition.Attributes.Count);
        Assert.Equal("street", addressResult.Definition.Attributes[0].Name);
        Assert.Equal(255, addressResult.Definition.Attributes[0].MaxLength);
        Assert.Equal("zip_code", addressResult.Definition.Attributes[3].Name);
        
        // Real-world: Contact info
        var contactBlock = CreateBlocks(@"CREATE TYPE contact_info AS (
    phone VARCHAR(20),
    email VARCHAR(255),
    telegram VARCHAR(50),
    preferred_method VARCHAR(20)
);");
        var contactResult = _extractor.Extract(contactBlock);
        
        Assert.True(contactResult.IsSuccess);
        Assert.Equal("contact_info", contactResult.Definition!.Name);
        Assert.Equal(4, contactResult.Definition.Attributes.Count);
        Assert.Contains(contactResult.Definition.Attributes, a => a.Name == "phone");
        Assert.Contains(contactResult.Definition.Attributes, a => a.Name == "telegram");
    }

    [Fact]
    public void Extract_SpecialFormatCommentsAndEdgeCases_HandlesCorrectly()
    {
        // Покрывает: comment: формат, comment() формат, regular comments, empty attributes, invalid syntax
        
        // Format 1: comment: Text; rename: NewName; type: TYPE;
        var blocks1 = CreateBlocks(
            "CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100), zip_code VARCHAR(20));",
            "comment: Адрес пользователя; rename: UserAddress; type: ADDRESS;");
        var result1 = _extractor.Extract(blocks1);
        
        Assert.True(result1.IsSuccess);
        Assert.NotNull(result1.Definition);
        Assert.NotNull(result1.Definition.SqlComment);
        Assert.Contains("comment:", result1.Definition.SqlComment);
        Assert.Contains("Адрес пользователя", result1.Definition.SqlComment);
        Assert.Contains("rename:", result1.Definition.SqlComment);
        
        // Format 2: comment(Text); rename(NewName); type(TYPE);
        var blocks2 = CreateBlocks(
            "CREATE TYPE product_price AS (amount NUMERIC(12, 2), currency VARCHAR(3));",
            "comment(Цена продукта с валютой); rename(ProductPrice); type(PRICE);");
        var result2 = _extractor.Extract(blocks2);
        
        Assert.True(result2.IsSuccess);
        Assert.NotNull(result2.Definition);
        Assert.NotNull(result2.Definition.SqlComment);
        Assert.Contains("comment(", result2.Definition.SqlComment);
        Assert.Contains("Цена продукта", result2.Definition.SqlComment);
        Assert.Contains("rename(", result2.Definition.SqlComment);
        
        // Regular header comment
        var blocks3 = CreateBlocks(
            "CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100));",
            "Address type for user locations");
        var result3 = _extractor.Extract(blocks3);
        
        Assert.True(result3.IsSuccess);
        Assert.Equal("Address type for user locations", result3.Definition!.SqlComment);
        
        // Raw SQL preservation
        var rawSql = "-- Address type\nCREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100));";
        var block4 = new SqlBlock
        {
            Content = "CREATE TYPE address AS (street VARCHAR(255), city VARCHAR(100));",
            RawContent = rawSql,
            StartLine = 1,
            EndLine = 2
        };
        var blocks4 = new[] { block4 };
        var result4 = _extractor.Extract(blocks4);
        
        Assert.True(result4.IsSuccess);
        Assert.Equal(rawSql, result4.Definition!.RawSql);
        
        // Empty attributes - failure
        var emptyBlock = CreateBlocks("CREATE TYPE empty_type AS ();");
        var emptyResult = _extractor.Extract(emptyBlock);
        
        Assert.False(emptyResult.IsSuccess);
        Assert.Null(emptyResult.Definition);
        Assert.NotEmpty(emptyResult.ValidationIssues);
        Assert.Contains(emptyResult.ValidationIssues, i => i.Code == "COMPOSITE_EMPTY_ATTRIBUTES");
        
        // Non-composite block
        var nonCompositeBlock = CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);");
        var nonCompositeResult = _extractor.Extract(nonCompositeBlock);
        
        Assert.False(nonCompositeResult.IsSuccess);
        Assert.Null(nonCompositeResult.Definition);
        
        // Null blocks - exception
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_CompositeWithInlineComments_ShouldIgnoreComments()
    {
        // Arrange - SQL с inline комментариями как в Schema.sql
        var sql = @"CREATE TYPE address AS
(
    street   VARCHAR(255), -- comment: Улица; type: VARCHAR(255); rename: StreetAddress
    city     VARCHAR(100), -- comment: Город; type: VARCHAR(100); rename: CityName
    state    VARCHAR(50),  -- comment: Штат/область; type: VARCHAR(50); rename: StateName
    zip_code VARCHAR(20),  -- comment: Почтовый индекс; type: VARCHAR(20); rename: PostalCode
    country  VARCHAR(50)   -- comment: Страна; type: VARCHAR(50); rename: CountryName
);";
        
        // Важно: Нужно симулировать поведение BlockExtractor - удалить inline комментарии из Content
        var sqlWithoutComments = @"CREATE TYPE address AS
(
    street   VARCHAR(255),
    city     VARCHAR(100),
    state    VARCHAR(50),
    zip_code VARCHAR(20),
    country  VARCHAR(50)
);";

        var blocks = new List<SqlBlock>
        {
            new SqlBlock
            {
                Content = sqlWithoutComments,
                RawContent = sql,
                StartLine = 1,
                EndLine = 9,
                HeaderComment = null
            }
        };
        
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
        
        Assert.Equal("city", result.Definition.Attributes[1].Name);
        Assert.Equal(100, result.Definition.Attributes[1].MaxLength);
        
        Assert.Equal("zip_code", result.Definition.Attributes[3].Name);
        Assert.Equal(20, result.Definition.Attributes[3].MaxLength);
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? headerComment = null)
    {
        return [new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            StartLine = 1,
            EndLine = 1,
            HeaderComment = headerComment
        }];
    }
}
