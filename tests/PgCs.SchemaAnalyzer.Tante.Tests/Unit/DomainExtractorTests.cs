using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для DomainExtractor
/// Покрывает базовые типы, constraints (NOT NULL, DEFAULT, CHECK), параметры (VARCHAR, NUMERIC), валидацию, специальные форматы комментариев
/// </summary>
public sealed class DomainExtractorTests
{
    private readonly IExtractor<DomainTypeDefinition> _extractor = new DomainExtractor();

    [Fact]
    public void Extract_BasicDomains_HandlesAllVariants()
    {
        // Покрывает: simple domain, schema, VARCHAR with length, NUMERIC with precision/scale, different base types

        // Simple domain
        var simpleBlock = CreateBlocks("CREATE DOMAIN email AS VARCHAR(255);");
        var simpleResult = _extractor.Extract(simpleBlock);
        
        Assert.True(simpleResult.IsSuccess);
        Assert.NotNull(simpleResult.Definition);
        Assert.Equal("email", simpleResult.Definition.Name);
        Assert.Null(simpleResult.Definition.Schema);
        Assert.Equal("VARCHAR(255)", simpleResult.Definition.BaseType);
        Assert.Equal(255, simpleResult.Definition.MaxLength);
        
        // Domain with schema
        var schemaBlock = CreateBlocks("CREATE DOMAIN public.email AS VARCHAR(255);");
        var schemaResult = _extractor.Extract(schemaBlock);
        
        Assert.True(schemaResult.IsSuccess);
        Assert.Equal("email", schemaResult.Definition!.Name);
        Assert.Equal("public", schemaResult.Definition.Schema);
        
        // Numeric with precision and scale
        var numericBlock = CreateBlocks("CREATE DOMAIN positive_numeric AS NUMERIC(12, 2);");
        var numericResult = _extractor.Extract(numericBlock);
        
        Assert.True(numericResult.IsSuccess);
        Assert.Equal("positive_numeric", numericResult.Definition!.Name);
        Assert.Equal("NUMERIC(12, 2)", numericResult.Definition.BaseType);
        Assert.Equal(12, numericResult.Definition.NumericPrecision);
        Assert.Equal(2, numericResult.Definition.NumericScale);
        
        // TEXT type
        var textBlock = CreateBlocks("CREATE DOMAIN description AS TEXT NOT NULL;");
        var textResult = _extractor.Extract(textBlock);
        
        Assert.True(textResult.IsSuccess);
        Assert.Equal("TEXT", textResult.Definition!.BaseType);
        Assert.True(textResult.Definition.IsNotNull);
        
        // INTEGER type
        var intBlock = CreateBlocks("CREATE DOMAIN age AS INTEGER CHECK (VALUE >= 0 AND VALUE <= 150);");
        var intResult = _extractor.Extract(intBlock);
        
        Assert.True(intResult.IsSuccess);
        Assert.Equal("INTEGER", intResult.Definition!.BaseType);
        Assert.Single(intResult.Definition.CheckConstraints);
        
        // TIMESTAMP type with DEFAULT
        var timestampBlock = CreateBlocks("CREATE DOMAIN created_timestamp AS TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL;");
        var timestampResult = _extractor.Extract(timestampBlock);
        
        Assert.True(timestampResult.IsSuccess);
        Assert.Equal("TIMESTAMP", timestampResult.Definition!.BaseType);
        Assert.Equal("CURRENT_TIMESTAMP", timestampResult.Definition.DefaultValue);
        Assert.True(timestampResult.Definition.IsNotNull);
        
        // CanExtract validation
        Assert.True(_extractor.CanExtract(simpleBlock));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);")));
    }

    [Fact]
    public void Extract_ConstraintsAndOptions_HandlesAllTypes()
    {
        // Покрывает: NOT NULL, DEFAULT, CHECK (single and multiple), COLLATION

        // NOT NULL
        var notNullBlock = CreateBlocks("CREATE DOMAIN email AS VARCHAR(255) NOT NULL;");
        var notNullResult = _extractor.Extract(notNullBlock);
        
        Assert.True(notNullResult.IsSuccess);
        Assert.True(notNullResult.Definition!.IsNotNull);
        
        // DEFAULT value
        var defaultBlock = CreateBlocks("CREATE DOMAIN status AS VARCHAR(20) DEFAULT 'active';");
        var defaultResult = _extractor.Extract(defaultBlock);
        
        Assert.True(defaultResult.IsSuccess);
        Assert.Equal("'active'", defaultResult.Definition!.DefaultValue);
        
        // Single CHECK constraint
        var checkBlock = CreateBlocks("CREATE DOMAIN positive_int AS INTEGER CHECK (VALUE > 0);");
        var checkResult = _extractor.Extract(checkBlock);
        
        Assert.True(checkResult.IsSuccess);
        Assert.Single(checkResult.Definition!.CheckConstraints);
        Assert.Equal("VALUE > 0", checkResult.Definition.CheckConstraints[0]);
        
        // Multiple CHECK constraints
        var multiCheckBlock = CreateBlocks("CREATE DOMAIN percentage AS NUMERIC(5, 2) CHECK (VALUE >= 0) CHECK (VALUE <= 100);");
        var multiCheckResult = _extractor.Extract(multiCheckBlock);
        
        Assert.True(multiCheckResult.IsSuccess);
        Assert.Equal(2, multiCheckResult.Definition!.CheckConstraints.Count);
        Assert.Contains("VALUE >= 0", multiCheckResult.Definition.CheckConstraints);
        Assert.Contains("VALUE <= 100", multiCheckResult.Definition.CheckConstraints);
        
        // CHECK with AND
        var andCheckBlock = CreateBlocks("CREATE DOMAIN percentage AS NUMERIC(5, 2) CHECK (VALUE >= 0 AND VALUE <= 100);");
        var andCheckResult = _extractor.Extract(andCheckBlock);
        
        Assert.True(andCheckResult.IsSuccess);
        Assert.Single(andCheckResult.Definition!.CheckConstraints);
        Assert.Contains("VALUE >= 0 AND VALUE <= 100", andCheckResult.Definition.CheckConstraints[0]);
        
        // COLLATION
        var collationBlock = CreateBlocks("CREATE DOMAIN case_insensitive_text AS TEXT COLLATE \"en_US\";");
        var collationResult = _extractor.Extract(collationBlock);
        
        Assert.True(collationResult.IsSuccess);
        Assert.Equal("en_US", collationResult.Definition!.Collation);
        
        // All features combined
        var allFeaturesBlock = CreateBlocks(@"CREATE DOMAIN positive_numeric AS NUMERIC(12, 2)
    DEFAULT 0
    NOT NULL
    CHECK (VALUE >= 0);");
        var allFeaturesResult = _extractor.Extract(allFeaturesBlock);
        
        Assert.True(allFeaturesResult.IsSuccess);
        Assert.Equal("positive_numeric", allFeaturesResult.Definition!.Name);
        Assert.Equal("NUMERIC(12, 2)", allFeaturesResult.Definition.BaseType);
        Assert.Equal("0", allFeaturesResult.Definition.DefaultValue);
        Assert.True(allFeaturesResult.Definition.IsNotNull);
        Assert.Single(allFeaturesResult.Definition.CheckConstraints);
        Assert.Equal("VALUE >= 0", allFeaturesResult.Definition.CheckConstraints[0]);
    }

    [Fact]
    public void Extract_ComplexCheckConstraints_HandlesPatterns()
    {
        // Покрывает: regex patterns, complex expressions, real-world examples

        // Email validation with regex
        var emailBlock = CreateBlocks(
            "CREATE DOMAIN email AS VARCHAR(255) CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Z|a-z]{2,}$');");
        var emailResult = _extractor.Extract(emailBlock);
        
        Assert.True(emailResult.IsSuccess);
        Assert.Equal("email", emailResult.Definition!.Name);
        Assert.Equal("VARCHAR(255)", emailResult.Definition.BaseType);
        Assert.Single(emailResult.Definition.CheckConstraints);
        Assert.Contains("~*", emailResult.Definition.CheckConstraints[0]);
        Assert.Contains("@", emailResult.Definition.CheckConstraints[0]);
        
        // Username slug with regex
        var slugBlock = CreateBlocks("CREATE DOMAIN username_slug AS VARCHAR(50) CHECK (VALUE ~* '^[a-z0-9_-]+$');");
        var slugResult = _extractor.Extract(slugBlock);
        
        Assert.True(slugResult.IsSuccess);
        Assert.Equal("username_slug", slugResult.Definition!.Name);
        Assert.Equal(50, slugResult.Definition.MaxLength);
        Assert.Single(slugResult.Definition.CheckConstraints);
        Assert.Contains("~*", slugResult.Definition.CheckConstraints[0]);
        
        // Positive numeric with range
        var positiveBlock = CreateBlocks("CREATE DOMAIN positive_numeric AS NUMERIC(12, 2) CHECK (VALUE >= 0);");
        var positiveResult = _extractor.Extract(positiveBlock);
        
        Assert.True(positiveResult.IsSuccess);
        Assert.Equal("positive_numeric", positiveResult.Definition!.Name);
        Assert.Equal(12, positiveResult.Definition.NumericPrecision);
        Assert.Equal(2, positiveResult.Definition.NumericScale);
        Assert.Single(positiveResult.Definition.CheckConstraints);
        Assert.Equal("VALUE >= 0", positiveResult.Definition.CheckConstraints[0]);
        
        // Age domain with bounds
        var ageBlock = CreateBlocks("CREATE DOMAIN age AS INTEGER CHECK (VALUE >= 0 AND VALUE <= 150);");
        var ageResult = _extractor.Extract(ageBlock);
        
        Assert.True(ageResult.IsSuccess);
        Assert.Equal("INTEGER", ageResult.Definition!.BaseType);
        Assert.Single(ageResult.Definition.CheckConstraints);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: comment: формат и comment() формат для domain типов
        
        // Format 1: comment: Text; rename: NewName; type: TYPE;
        var blocks1 = CreateBlocks(
            "CREATE DOMAIN email AS VARCHAR(255) CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@');",
            "comment: Email адрес пользователя; rename: UserEmail; type: EMAIL;");
        var result1 = _extractor.Extract(blocks1);
        
        Assert.True(result1.IsSuccess);
        Assert.NotNull(result1.Definition);
        Assert.NotNull(result1.Definition.SqlComment);
        Assert.Contains("comment:", result1.Definition.SqlComment);
        Assert.Contains("Email адрес пользователя", result1.Definition.SqlComment);
        Assert.Contains("rename:", result1.Definition.SqlComment);
        
        // Format 2: comment(Text); rename(NewName); type(TYPE);
        var blocks2 = CreateBlocks(
            "CREATE DOMAIN positive_numeric AS NUMERIC(12, 2) CHECK (VALUE >= 0);",
            "comment(Положительное число с 2 знаками после запятой); rename(PositiveAmount); type(AMOUNT);");
        var result2 = _extractor.Extract(blocks2);
        
        Assert.True(result2.IsSuccess);
        Assert.NotNull(result2.Definition);
        Assert.NotNull(result2.Definition.SqlComment);
        Assert.Contains("comment(", result2.Definition.SqlComment);
        Assert.Contains("Положительное число", result2.Definition.SqlComment);
        Assert.Contains("rename(", result2.Definition.SqlComment);
        
        // Regular header comment
        var blocks3 = CreateBlocks(
            "CREATE DOMAIN email AS VARCHAR(255);",
            "Email address with validation");
        var result3 = _extractor.Extract(blocks3);
        
        Assert.True(result3.IsSuccess);
        Assert.Equal("Email address with validation", result3.Definition!.SqlComment);
        
        // Raw SQL preservation
        var rawSql = "-- Email domain\nCREATE DOMAIN email AS VARCHAR(255);";
        var block4 = new SqlBlock
        {
            Content = "CREATE DOMAIN email AS VARCHAR(255);",
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
        // Покрывает: validation issues (excessive length, invalid numeric params, too many checks), errors, edge cases

        // Excessive string length - warning
        var excessiveLengthBlock = CreateBlocks("CREATE DOMAIN huge_text AS VARCHAR(50000);");
        var excessiveLengthResult = _extractor.Extract(excessiveLengthBlock);
        
        Assert.True(excessiveLengthResult.IsSuccess);
        Assert.NotEmpty(excessiveLengthResult.ValidationIssues);
        var lengthWarning = excessiveLengthResult.ValidationIssues.First(i => i.Code == "DOMAIN_EXCESSIVE_LENGTH");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, lengthWarning.Severity);
        Assert.Contains("50000", lengthWarning.Message);
        
        // Invalid numeric scale (scale > precision) - warning
        var invalidScaleBlock = CreateBlocks("CREATE DOMAIN bad_numeric AS NUMERIC(5, 10);");
        var invalidScaleResult = _extractor.Extract(invalidScaleBlock);
        
        Assert.True(invalidScaleResult.IsSuccess);
        Assert.NotEmpty(invalidScaleResult.ValidationIssues);
        var scaleWarning = invalidScaleResult.ValidationIssues.First(i => i.Code == "DOMAIN_INVALID_NUMERIC_PARAMS");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, scaleWarning.Severity);
        Assert.Contains("scale", scaleWarning.Message);
        
        // Excessive precision - warning
        var excessivePrecisionBlock = CreateBlocks("CREATE DOMAIN huge_numeric AS NUMERIC(2000, 2);");
        var excessivePrecisionResult = _extractor.Extract(excessivePrecisionBlock);
        
        Assert.True(excessivePrecisionResult.IsSuccess);
        Assert.NotEmpty(excessivePrecisionResult.ValidationIssues);
        var precisionWarning = excessivePrecisionResult.ValidationIssues.First(i => i.Code == "DOMAIN_EXCESSIVE_PRECISION");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, precisionWarning.Severity);
        Assert.Contains("2000", precisionWarning.Message);
        
        // Too many CHECK constraints - warning
        var tooManyChecksBlock = CreateBlocks(
            "CREATE DOMAIN complex AS INTEGER CHECK (VALUE > 0) CHECK (VALUE < 100) CHECK (VALUE != 50) " +
            "CHECK (VALUE != 75) CHECK (VALUE != 25) CHECK (VALUE != 10);");
        var tooManyChecksResult = _extractor.Extract(tooManyChecksBlock);
        
        Assert.True(tooManyChecksResult.IsSuccess);
        Assert.NotEmpty(tooManyChecksResult.ValidationIssues);
        var checksWarning = tooManyChecksResult.ValidationIssues.First(i => i.Code == "DOMAIN_TOO_MANY_CHECKS");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, checksWarning.Severity);
        Assert.Contains("6", checksWarning.Message);
        
        // Multiple issues combined
        var multipleIssuesBlock = CreateBlocks(
            "CREATE DOMAIN complex AS NUMERIC(2000, 2) CHECK (VALUE > 0) CHECK (VALUE < 100) CHECK (VALUE != 50) " +
            "CHECK (VALUE != 75) CHECK (VALUE != 25) CHECK (VALUE != 10);");
        var multipleIssuesResult = _extractor.Extract(multipleIssuesBlock);
        
        Assert.True(multipleIssuesResult.IsSuccess);
        Assert.NotEmpty(multipleIssuesResult.ValidationIssues);
        Assert.Equal(2, multipleIssuesResult.ValidationIssues.Count);
        Assert.Contains(multipleIssuesResult.ValidationIssues, i => i.Code == "DOMAIN_EXCESSIVE_PRECISION");
        Assert.Contains(multipleIssuesResult.ValidationIssues, i => i.Code == "DOMAIN_TOO_MANY_CHECKS");
        
        // Non-domain block
        var nonDomainBlock = CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);");
        var nonDomainResult = _extractor.Extract(nonDomainBlock);
        Assert.False(nonDomainResult.IsSuccess);
        Assert.Null(nonDomainResult.Definition);
        
        // Invalid syntax - error
        var invalidBlock = CreateBlocks("CREATE DOMAIN INVALID SYNTAX");
        var invalidResult = _extractor.Extract(invalidBlock);
        
        Assert.False(invalidResult.IsSuccess);
        Assert.Null(invalidResult.Definition);
        Assert.NotEmpty(invalidResult.ValidationIssues);
        var error = invalidResult.ValidationIssues.First(i => i.Code == "DOMAIN_PARSE_ERROR");
        Assert.Equal(ValidationIssue.ValidationSeverity.Error, error.Severity);
        
        // Null blocks - exception
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? headerComment = null)
    {
        return new[]
        {
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                StartLine = 1,
                EndLine = 1,
                HeaderComment = headerComment
            }
        };
    }
}
