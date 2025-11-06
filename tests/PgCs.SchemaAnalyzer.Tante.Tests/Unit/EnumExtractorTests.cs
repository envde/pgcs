using PgCs.Core.Extraction;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для EnumExtractor
/// Покрывает базовые enum, schema, multiline format, case sensitivity, специальные символы, валидацию, специальные форматы комментариев
/// </summary>
public sealed class EnumExtractorTests
{
    private readonly IExtractor<EnumTypeDefinition> _extractor = new EnumExtractor();

    [Fact]
    public void Extract_BasicEnums_HandlesAllVariants()
    {
        // Покрывает: simple enum, schema, single value, multiline format, case sensitivity

        // Simple enum with multiple values
        var simpleBlock = CreateBlocks("CREATE TYPE status AS ENUM ('active', 'inactive', 'pending');");
        var simpleResult = _extractor.Extract(simpleBlock);
        
        Assert.True(simpleResult.IsSuccess);
        Assert.NotNull(simpleResult.Definition);
        Assert.Equal("status", simpleResult.Definition.Name);
        Assert.Null(simpleResult.Definition.Schema);
        Assert.Equal(3, simpleResult.Definition.Values.Count);
        Assert.Equal("active", simpleResult.Definition.Values[0]);
        Assert.Equal("inactive", simpleResult.Definition.Values[1]);
        Assert.Equal("pending", simpleResult.Definition.Values[2]);
        
        // Enum with schema
        var schemaBlock = CreateBlocks("CREATE TYPE public.order_status AS ENUM ('new', 'processing', 'completed');");
        var schemaResult = _extractor.Extract(schemaBlock);
        
        Assert.True(schemaResult.IsSuccess);
        Assert.Equal("order_status", schemaResult.Definition!.Name);
        Assert.Equal("public", schemaResult.Definition.Schema);
        Assert.Equal(3, schemaResult.Definition.Values.Count);
        
        // Single value
        var singleBlock = CreateBlocks("CREATE TYPE status AS ENUM ('active');");
        var singleResult = _extractor.Extract(singleBlock);
        
        Assert.True(singleResult.IsSuccess);
        Assert.Single(singleResult.Definition!.Values);
        Assert.Equal("active", singleResult.Definition.Values[0]);
        
        // Multiline format
        var multilineBlock = CreateBlocks(@"CREATE TYPE user_status AS ENUM (
    'active',
    'inactive',
    'suspended',
    'deleted'
);");
        var multilineResult = _extractor.Extract(multilineBlock);
        
        Assert.True(multilineResult.IsSuccess);
        Assert.Equal("user_status", multilineResult.Definition!.Name);
        Assert.Equal(4, multilineResult.Definition.Values.Count);
        Assert.Equal("active", multilineResult.Definition.Values[0]);
        Assert.Equal("deleted", multilineResult.Definition.Values[3]);
        
        // CanExtract validation
        Assert.True(_extractor.CanExtract(simpleBlock));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);")));
    }

    [Fact]
    public void Extract_SpecialCasesAndFormatting_HandlesCorrectly()
    {
        // Покрывает: spaces in values, special characters, uppercase/mixed case, real-world examples

        // Spaces in values
        var spacesBlock = CreateBlocks("CREATE TYPE status AS ENUM (  'active'  ,  'inactive'  ,  'pending'  );");
        var spacesResult = _extractor.Extract(spacesBlock);
        
        Assert.True(spacesResult.IsSuccess);
        Assert.Equal(3, spacesResult.Definition!.Values.Count);
        Assert.Equal("active", spacesResult.Definition.Values[0]);
        
        // Special characters in values
        var specialBlock = CreateBlocks("CREATE TYPE status AS ENUM ('in-progress', 'not_started', 'done!');");
        var specialResult = _extractor.Extract(specialBlock);
        
        Assert.True(specialResult.IsSuccess);
        Assert.Equal(3, specialResult.Definition!.Values.Count);
        Assert.Equal("in-progress", specialResult.Definition.Values[0]);
        Assert.Equal("not_started", specialResult.Definition.Values[1]);
        Assert.Equal("done!", specialResult.Definition.Values[2]);
        
        // Uppercase keywords and values
        var upperBlock = CreateBlocks("CREATE TYPE STATUS AS ENUM ('ACTIVE', 'INACTIVE');");
        var upperResult = _extractor.Extract(upperBlock);
        
        Assert.True(upperResult.IsSuccess);
        Assert.Equal("STATUS", upperResult.Definition!.Name);
        Assert.Equal("ACTIVE", upperResult.Definition.Values[0]);
        Assert.Equal("INACTIVE", upperResult.Definition.Values[1]);
        
        // Mixed case keywords
        var mixedBlock = CreateBlocks("Create Type status As Enum ('active', 'inactive');");
        var mixedResult = _extractor.Extract(mixedBlock);
        
        Assert.True(mixedResult.IsSuccess);
        Assert.Equal("status", mixedResult.Definition!.Name);
        
        // Real-world: Order status
        var orderBlock = CreateBlocks("CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');");
        var orderResult = _extractor.Extract(orderBlock);
        
        Assert.True(orderResult.IsSuccess);
        Assert.Equal("order_status", orderResult.Definition!.Name);
        Assert.Equal(5, orderResult.Definition.Values.Count);
        Assert.Contains("pending", orderResult.Definition.Values);
        Assert.Contains("delivered", orderResult.Definition.Values);
        
        // Real-world: Payment method
        var paymentBlock = CreateBlocks("CREATE TYPE payment_method AS ENUM ('credit_card', 'debit_card', 'paypal', 'bank_transfer', 'crypto', 'cash');");
        var paymentResult = _extractor.Extract(paymentBlock);
        
        Assert.True(paymentResult.IsSuccess);
        Assert.Equal("payment_method", paymentResult.Definition!.Name);
        Assert.Equal(6, paymentResult.Definition.Values.Count);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: comment: формат и comment() формат для enum типов
        
        // Format 1: comment: Text; to_name: NewName; to_type: TYPE;
        var blocks1 = CreateBlocks(
            "CREATE TYPE status AS ENUM ('active', 'inactive', 'pending');",
            "comment: Статус пользователя в системе; to_name: UserStatus; to_type: STATUS;");
        var result1 = _extractor.Extract(blocks1);
        
        Assert.True(result1.IsSuccess);
        Assert.NotNull(result1.Definition);
        Assert.NotNull(result1.Definition.SqlComment);
        Assert.Contains("comment:", result1.Definition.SqlComment);
        Assert.Contains("Статус пользователя", result1.Definition.SqlComment);
        Assert.Contains("to_name:", result1.Definition.SqlComment);
        
        // Format 2: comment(Text); to_name(NewName); to_type(TYPE);
        var blocks2 = CreateBlocks(
            "CREATE TYPE order_status AS ENUM ('new', 'processing', 'completed', 'cancelled');",
            "comment(Статус заказа); to_name(OrderStatus); to_type(ORDER_STATUS);");
        var result2 = _extractor.Extract(blocks2);
        
        Assert.True(result2.IsSuccess);
        Assert.NotNull(result2.Definition);
        Assert.NotNull(result2.Definition.SqlComment);
        Assert.Contains("comment(", result2.Definition.SqlComment);
        Assert.Contains("Статус заказа", result2.Definition.SqlComment);
        Assert.Contains("to_name(", result2.Definition.SqlComment);
        
        // Regular header comment
        var blocks3 = CreateBlocks(
            "CREATE TYPE status AS ENUM ('active', 'inactive');",
            "User account status enumeration");
        var result3 = _extractor.Extract(blocks3);
        
        Assert.True(result3.IsSuccess);
        Assert.Equal("User account status enumeration", result3.Definition!.SqlComment);
        
        // Raw SQL preservation
        var rawSql = "-- Status type\nCREATE TYPE status AS ENUM ('active', 'inactive');";
        var block4 = new SqlBlock
        {
            Content = "CREATE TYPE status AS ENUM ('active', 'inactive');",
            RawContent = rawSql,
            StartLine = 1,
            EndLine = 2
        };
        var blocks4 = new[] { block4 };
        var result4 = _extractor.Extract(blocks4);
        
        Assert.True(result4.IsSuccess);
        Assert.Equal(rawSql, result4.Definition!.RawSql);
    }

    [Fact]
    public void Extract_ValidationAndEdgeCases_HandlesCorrectly()
    {
        // Покрывает: empty values, duplicate values, too many values, too long value, invalid syntax, errors

        // Empty enum values - failure
        var emptyBlock = CreateBlocks("CREATE TYPE status AS ENUM ();");
        var emptyResult = _extractor.Extract(emptyBlock);
        
        Assert.False(emptyResult.IsSuccess);
        Assert.Null(emptyResult.Definition);
        Assert.NotEmpty(emptyResult.ValidationIssues);
        
        // Duplicate values - warning
        var duplicateBlock = CreateBlocks("CREATE TYPE status AS ENUM ('active', 'inactive', 'active', 'pending');");
        var duplicateResult = _extractor.Extract(duplicateBlock);
        
        Assert.True(duplicateResult.IsSuccess);
        Assert.NotEmpty(duplicateResult.ValidationIssues);
        var duplicateWarning = duplicateResult.ValidationIssues.First(i => i.Code == "ENUM_DUPLICATE_VALUES");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, duplicateWarning.Severity);
        Assert.Contains("active", duplicateWarning.Message);
        
        // Empty string value - warning
        var emptyValueBlock = CreateBlocks("CREATE TYPE status AS ENUM ('active', '', 'inactive');");
        var emptyValueResult = _extractor.Extract(emptyValueBlock);
        
        Assert.True(emptyValueResult.IsSuccess);
        Assert.NotEmpty(emptyValueResult.ValidationIssues);
        var emptyValueWarning = emptyValueResult.ValidationIssues.First(i => i.Code == "ENUM_EMPTY_VALUE");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, emptyValueWarning.Severity);
        
        // Too many values - warning
        var values = string.Join(", ", Enumerable.Range(1, 150).Select(i => $"'value{i}'"));
        var tooManyBlock = CreateBlocks($"CREATE TYPE large_enum AS ENUM ({values});");
        var tooManyResult = _extractor.Extract(tooManyBlock);
        
        Assert.True(tooManyResult.IsSuccess);
        Assert.NotEmpty(tooManyResult.ValidationIssues);
        var tooManyWarning = tooManyResult.ValidationIssues.First(i => i.Code == "ENUM_TOO_MANY_VALUES");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, tooManyWarning.Severity);
        Assert.Contains("150", tooManyWarning.Message);
        
        // Very long value - warning
        var longValue = new string('x', 300);
        var longValueBlock = CreateBlocks($"CREATE TYPE status AS ENUM ('active', '{longValue}', 'inactive');");
        var longValueResult = _extractor.Extract(longValueBlock);
        
        Assert.True(longValueResult.IsSuccess);
        Assert.NotEmpty(longValueResult.ValidationIssues);
        var longValueWarning = longValueResult.ValidationIssues.First(i => i.Code == "ENUM_VALUE_TOO_LONG");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, longValueWarning.Severity);
        
        // Multiple issues combined
        var multipleBlock = CreateBlocks("CREATE TYPE status AS ENUM ('active', 'active', '', 'inactive');");
        var multipleResult = _extractor.Extract(multipleBlock);
        
        Assert.True(multipleResult.IsSuccess);
        Assert.NotEmpty(multipleResult.ValidationIssues);
        Assert.Equal(2, multipleResult.ValidationIssues.Count);
        Assert.Contains(multipleResult.ValidationIssues, i => i.Code == "ENUM_DUPLICATE_VALUES");
        Assert.Contains(multipleResult.ValidationIssues, i => i.Code == "ENUM_EMPTY_VALUE");
        
        // Invalid syntax (missing AS)
        var invalidBlock = CreateBlocks("CREATE TYPE status ENUM ('active', 'inactive');");
        var invalidResult = _extractor.Extract(invalidBlock);
        
        Assert.False(invalidResult.IsSuccess);
        Assert.Null(invalidResult.Definition);
        
        // Non-enum block
        var nonEnumBlock = CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);");
        var nonEnumResult = _extractor.Extract(nonEnumBlock);
        
        Assert.False(nonEnumResult.IsSuccess);
        Assert.Null(nonEnumResult.Definition);
        
        // Null blocks - exception
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? headerComment = null)
    {
        return
        [
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                StartLine = 1,
                EndLine = 1,
                HeaderComment = headerComment
            }
        ];
    }
}
