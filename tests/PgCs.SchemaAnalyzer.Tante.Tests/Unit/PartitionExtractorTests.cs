using PgCs.Core.Extraction;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для PartitionExtractor
/// Покрывает RANGE, LIST, HASH, DEFAULT партиции, schema, tablespace, валидацию, специальные форматы комментариев
/// </summary>
public sealed class PartitionExtractorTests
{
    private readonly IExtractor<PartitionDefinition> _extractor = new PartitionExtractor();

    [Fact]
    public void Extract_BasicPartitions_HandlesAllStrategies()
    {
        // Покрывает: RANGE partition, LIST partition, HASH partition, DEFAULT partition, schema qualification, parent table

        // RANGE partition - простая партиция по диапазону дат
        var rangeBlock = CreateBlocks("CREATE TABLE sales_2023 PARTITION OF sales_data FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');");
        var rangeResult = _extractor.Extract(rangeBlock);
        
        Assert.True(rangeResult.IsSuccess);
        Assert.NotNull(rangeResult.Definition);
        Assert.Equal("sales_2023", rangeResult.Definition.Name);
        Assert.Equal("sales_data", rangeResult.Definition.ParentTableName);
        Assert.Equal(PartitionStrategy.Range, rangeResult.Definition.Strategy);
        Assert.Equal("'2023-01-01'", rangeResult.Definition.FromValue);
        Assert.Equal("'2024-01-01'", rangeResult.Definition.ToValue);
        Assert.Null(rangeResult.Definition.Schema);
        Assert.False(rangeResult.Definition.IsDefault);
        
        // RANGE partition со схемой
        var rangeSchemaBlock = CreateBlocks("CREATE TABLE public.audit_logs_2024_q1 PARTITION OF public.audit_logs FOR VALUES FROM ('2024-01-01') TO ('2024-04-01');");
        var rangeSchemaResult = _extractor.Extract(rangeSchemaBlock);
        
        Assert.True(rangeSchemaResult.IsSuccess);
        Assert.Equal("audit_logs_2024_q1", rangeSchemaResult.Definition!.Name);
        Assert.Equal("public", rangeSchemaResult.Definition.Schema);
        Assert.Equal("audit_logs", rangeSchemaResult.Definition.ParentTableName);
        Assert.Equal(PartitionStrategy.Range, rangeSchemaResult.Definition.Strategy);
        
        // LIST partition - партиция по списку значений
        var listBlock = CreateBlocks("CREATE TABLE orders_active PARTITION OF orders FOR VALUES IN ('active', 'pending', 'processing');");
        var listResult = _extractor.Extract(listBlock);
        
        Assert.True(listResult.IsSuccess);
        Assert.Equal("orders_active", listResult.Definition!.Name);
        Assert.Equal("orders", listResult.Definition.ParentTableName);
        Assert.Equal(PartitionStrategy.List, listResult.Definition.Strategy);
        Assert.NotNull(listResult.Definition.InValues);
        Assert.Equal(3, listResult.Definition.InValues.Count);
        Assert.Equal("active", listResult.Definition.InValues[0]);
        Assert.Equal("pending", listResult.Definition.InValues[1]);
        Assert.Equal("processing", listResult.Definition.InValues[2]);
        
        // HASH partition - партиция по хэшу
        var hashBlock = CreateBlocks("CREATE TABLE users_p0 PARTITION OF users FOR VALUES WITH (MODULUS 4, REMAINDER 0);");
        var hashResult = _extractor.Extract(hashBlock);
        
        Assert.True(hashResult.IsSuccess);
        Assert.Equal("users_p0", hashResult.Definition!.Name);
        Assert.Equal("users", hashResult.Definition.ParentTableName);
        Assert.Equal(PartitionStrategy.Hash, hashResult.Definition.Strategy);
        Assert.Equal(4, hashResult.Definition.Modulus);
        Assert.Equal(0, hashResult.Definition.Remainder);
        
        // HASH partition с разными remainder
        var hashBlock2 = CreateBlocks("CREATE TABLE users_p3 PARTITION OF users FOR VALUES WITH (MODULUS 4, REMAINDER 3);");
        var hashResult2 = _extractor.Extract(hashBlock2);
        
        Assert.True(hashResult2.IsSuccess);
        Assert.Equal(3, hashResult2.Definition!.Remainder);
        
        // DEFAULT partition - партиция по умолчанию
        var defaultBlock = CreateBlocks("CREATE TABLE sales_default PARTITION OF sales_data DEFAULT;");
        var defaultResult = _extractor.Extract(defaultBlock);
        
        Assert.True(defaultResult.IsSuccess);
        Assert.Equal("sales_default", defaultResult.Definition!.Name);
        Assert.Equal("sales_data", defaultResult.Definition.ParentTableName);
        Assert.True(defaultResult.Definition.IsDefault);
        Assert.NotEmpty(defaultResult.ValidationIssues); // Должно быть информационное сообщение
        var infoIssue = defaultResult.ValidationIssues.First(i => i.Code == "PARTITION_DEFAULT");
        Assert.Equal(ValidationIssue.ValidationSeverity.Info, infoIssue.Severity);
        
        // CanExtract validation
        Assert.True(_extractor.CanExtract(rangeBlock));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);")));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TYPE status AS ENUM ('active');")));
    }

    [Fact]
    public void Extract_SpecialCasesAndFormatting_HandlesCorrectly()
    {
        // Покрывает: tablespace, multiline format, uppercase/mixed case, real-world examples, spaces in values

        // RANGE partition с tablespace
        var tablespaceBlock = CreateBlocks("CREATE TABLE sales_2023 PARTITION OF sales_data FOR VALUES FROM ('2023-01-01') TO ('2024-01-01') TABLESPACE fast_storage;");
        var tablespaceResult = _extractor.Extract(tablespaceBlock);
        
        Assert.True(tablespaceResult.IsSuccess);
        Assert.Equal("fast_storage", tablespaceResult.Definition!.Tablespace);
        
        // Multiline format
        var multilineBlock = CreateBlocks(@"CREATE TABLE audit_logs_2024_q2 PARTITION OF audit_logs
    FOR VALUES FROM ('2024-04-01') TO ('2024-07-01');");
        var multilineResult = _extractor.Extract(multilineBlock);
        
        Assert.True(multilineResult.IsSuccess);
        Assert.Equal("audit_logs_2024_q2", multilineResult.Definition!.Name);
        Assert.Equal("audit_logs", multilineResult.Definition.ParentTableName);
        
        // Uppercase keywords
        var upperBlock = CreateBlocks("CREATE TABLE SALES_2023 PARTITION OF SALES_DATA FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');");
        var upperResult = _extractor.Extract(upperBlock);
        
        Assert.True(upperResult.IsSuccess);
        Assert.Equal("SALES_2023", upperResult.Definition!.Name);
        Assert.Equal("SALES_DATA", upperResult.Definition.ParentTableName);
        
        // Mixed case keywords
        var mixedBlock = CreateBlocks("Create Table sales_2023 Partition Of sales_data For Values From ('2023-01-01') To ('2024-01-01');");
        var mixedResult = _extractor.Extract(mixedBlock);
        
        Assert.True(mixedResult.IsSuccess);
        Assert.Equal("sales_2023", mixedResult.Definition!.Name);
        
        // LIST partition с пробелами
        var spacesBlock = CreateBlocks("CREATE TABLE orders_active PARTITION OF orders FOR VALUES IN (  'active'  ,  'pending'  ,  'processing'  );");
        var spacesResult = _extractor.Extract(spacesBlock);
        
        Assert.True(spacesResult.IsSuccess);
        Assert.Equal(3, spacesResult.Definition!.InValues!.Count);
        
        // Real-world: quarterly RANGE partition
        var quarterlyBlock = CreateBlocks("CREATE TABLE logs_q1_2024 PARTITION OF audit_logs FOR VALUES FROM ('2024-01-01 00:00:00') TO ('2024-04-01 00:00:00');");
        var quarterlyResult = _extractor.Extract(quarterlyBlock);
        
        Assert.True(quarterlyResult.IsSuccess);
        Assert.Equal("logs_q1_2024", quarterlyResult.Definition!.Name);
        Assert.Contains("2024-01-01", quarterlyResult.Definition.FromValue!);
        Assert.Contains("2024-04-01", quarterlyResult.Definition.ToValue!);
        
        // Real-world: LIST partition для статусов
        var statusBlock = CreateBlocks("CREATE TABLE orders_completed PARTITION OF orders FOR VALUES IN ('shipped', 'delivered', 'completed');");
        var statusResult = _extractor.Extract(statusBlock);
        
        Assert.True(statusResult.IsSuccess);
        Assert.Equal(3, statusResult.Definition!.InValues!.Count);
        Assert.Contains("completed", statusResult.Definition.InValues);
        
        // Real-world: HASH partition для распределенной нагрузки
        var hashDistBlock = CreateBlocks("CREATE TABLE sessions_shard_0 PARTITION OF user_sessions FOR VALUES WITH (MODULUS 8, REMAINDER 0);");
        var hashDistResult = _extractor.Extract(hashDistBlock);
        
        Assert.True(hashDistResult.IsSuccess);
        Assert.Equal(8, hashDistResult.Definition!.Modulus);
        Assert.Equal(0, hashDistResult.Definition.Remainder);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: comment: формат и comment() формат для партиций
        
        // Format 1: comment: Text; rename: NewName;
        var blocks1 = CreateBlocks(
            "CREATE TABLE sales_2023 PARTITION OF sales_data FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');",
            "comment: Партиция продаж за 2023 год; rename: Sales2023Partition;");
        var result1 = _extractor.Extract(blocks1);
        
        Assert.True(result1.IsSuccess);
        Assert.NotNull(result1.Definition);
        Assert.NotNull(result1.Definition.SqlComment);
        Assert.Contains("comment:", result1.Definition.SqlComment);
        Assert.Contains("Партиция продаж", result1.Definition.SqlComment);
        Assert.Contains("rename:", result1.Definition.SqlComment);
        
        // Format 2: comment(Text); rename(NewName);
        var blocks2 = CreateBlocks(
            "CREATE TABLE audit_logs_2024_q1 PARTITION OF audit_logs FOR VALUES FROM ('2024-01-01') TO ('2024-04-01');",
            "comment(Партиция логов за Q1 2024); rename(AuditLogsQ1_2024);");
        var result2 = _extractor.Extract(blocks2);
        
        Assert.True(result2.IsSuccess);
        Assert.NotNull(result2.Definition);
        Assert.NotNull(result2.Definition.SqlComment);
        Assert.Contains("comment(", result2.Definition.SqlComment);
        Assert.Contains("Партиция логов за Q1", result2.Definition.SqlComment);
        
        // Regular header comment
        var blocks3 = CreateBlocks(
            "CREATE TABLE orders_active PARTITION OF orders FOR VALUES IN ('active', 'pending');",
            "Active orders partition for quick access");
        var result3 = _extractor.Extract(blocks3);
        
        Assert.True(result3.IsSuccess);
        Assert.Equal("Active orders partition for quick access", result3.Definition!.SqlComment);
        
        // HASH partition with comment
        var blocks4 = CreateBlocks(
            "CREATE TABLE users_p0 PARTITION OF users FOR VALUES WITH (MODULUS 4, REMAINDER 0);",
            "Users partition shard 0 of 4");
        var result4 = _extractor.Extract(blocks4);
        
        Assert.True(result4.IsSuccess);
        Assert.Equal("Users partition shard 0 of 4", result4.Definition!.SqlComment);
        
        // DEFAULT partition with comment
        var blocks5 = CreateBlocks(
            "CREATE TABLE sales_default PARTITION OF sales_data DEFAULT;",
            "Default partition for unmatched data");
        var result5 = _extractor.Extract(blocks5);
        
        Assert.True(result5.IsSuccess);
        Assert.Equal("Default partition for unmatched data", result5.Definition!.SqlComment);
        
        // Raw SQL preservation
        var rawSql = "-- Partition for 2023\nCREATE TABLE sales_2023 PARTITION OF sales_data FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');";
        var block6 = new SqlBlock
        {
            Content = "CREATE TABLE sales_2023 PARTITION OF sales_data FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');",
            RawContent = rawSql,
            StartLine = 1,
            EndLine = 2
        };
        var blocks6 = new[] { block6 };
        var result6 = _extractor.Extract(blocks6);
        
        Assert.True(result6.IsSuccess);
        Assert.Equal(rawSql, result6.Definition!.RawSql);
    }

    [Fact]
    public void Extract_ValidationAndEdgeCases_HandlesCorrectly()
    {
        // Покрывает: empty LIST values, duplicate LIST values, too many LIST values, equal RANGE values, 
        // invalid HASH modulus/remainder, invalid syntax, errors

        // LIST partition без значений - failure
        var emptyListBlock = CreateBlocks("CREATE TABLE orders_empty PARTITION OF orders FOR VALUES IN ();");
        var emptyListResult = _extractor.Extract(emptyListBlock);
        
        Assert.False(emptyListResult.IsSuccess);
        Assert.Null(emptyListResult.Definition);
        Assert.NotEmpty(emptyListResult.ValidationIssues);
        var emptyError = emptyListResult.ValidationIssues.First(i => i.Code == "PARTITION_LIST_NO_VALUES");
        Assert.Equal(ValidationIssue.ValidationSeverity.Error, emptyError.Severity);
        
        // LIST partition с дубликатами - warning
        var duplicateListBlock = CreateBlocks("CREATE TABLE orders_active PARTITION OF orders FOR VALUES IN ('active', 'pending', 'active', 'processing');");
        var duplicateListResult = _extractor.Extract(duplicateListBlock);
        
        Assert.True(duplicateListResult.IsSuccess);
        Assert.NotEmpty(duplicateListResult.ValidationIssues);
        var duplicateWarning = duplicateListResult.ValidationIssues.First(i => i.Code == "PARTITION_LIST_DUPLICATE_VALUES");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, duplicateWarning.Severity);
        Assert.Contains("active", duplicateWarning.Message);
        
        // LIST partition со слишком большим количеством значений - warning
        var values = string.Join(", ", Enumerable.Range(1, 150).Select(i => $"'value{i}'"));
        var tooManyListBlock = CreateBlocks($"CREATE TABLE orders_many PARTITION OF orders FOR VALUES IN ({values});");
        var tooManyListResult = _extractor.Extract(tooManyListBlock);
        
        Assert.True(tooManyListResult.IsSuccess);
        Assert.NotEmpty(tooManyListResult.ValidationIssues);
        var tooManyWarning = tooManyListResult.ValidationIssues.First(i => i.Code == "PARTITION_LIST_TOO_MANY_VALUES");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, tooManyWarning.Severity);
        Assert.Contains("150", tooManyWarning.Message);
        
        // RANGE partition с равными FROM и TO - warning
        var equalRangeBlock = CreateBlocks("CREATE TABLE sales_equal PARTITION OF sales_data FOR VALUES FROM ('2023-01-01') TO ('2023-01-01');");
        var equalRangeResult = _extractor.Extract(equalRangeBlock);
        
        Assert.True(equalRangeResult.IsSuccess);
        Assert.NotEmpty(equalRangeResult.ValidationIssues);
        var equalWarning = equalRangeResult.ValidationIssues.First(i => i.Code == "PARTITION_RANGE_EQUAL");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, equalWarning.Severity);
        Assert.Contains("equal FROM and TO", equalWarning.Message);
        
        // HASH partition с невалидным MODULUS (0) - error
        var invalidModulusBlock = CreateBlocks("CREATE TABLE users_p0 PARTITION OF users FOR VALUES WITH (MODULUS 0, REMAINDER 0);");
        var invalidModulusResult = _extractor.Extract(invalidModulusBlock);
        
        Assert.False(invalidModulusResult.IsSuccess);
        Assert.Null(invalidModulusResult.Definition);
        Assert.NotEmpty(invalidModulusResult.ValidationIssues);
        var modulusError = invalidModulusResult.ValidationIssues.First(i => i.Code == "PARTITION_HASH_INVALID_MODULUS");
        Assert.Equal(ValidationIssue.ValidationSeverity.Error, modulusError.Severity);
        
        // HASH partition с невалидным MODULUS (отрицательное) - error
        var negativeModulusBlock = CreateBlocks("CREATE TABLE users_p0 PARTITION OF users FOR VALUES WITH (MODULUS -4, REMAINDER 0);");
        var negativeModulusResult = _extractor.Extract(negativeModulusBlock);
        
        Assert.False(negativeModulusResult.IsSuccess);
        
        // HASH partition с невалидным REMAINDER (>= MODULUS) - error
        var invalidRemainderBlock = CreateBlocks("CREATE TABLE users_p4 PARTITION OF users FOR VALUES WITH (MODULUS 4, REMAINDER 4);");
        var invalidRemainderResult = _extractor.Extract(invalidRemainderBlock);
        
        Assert.False(invalidRemainderResult.IsSuccess);
        Assert.NotEmpty(invalidRemainderResult.ValidationIssues);
        var remainderError = invalidRemainderResult.ValidationIssues.First(i => i.Code == "PARTITION_HASH_INVALID_REMAINDER");
        Assert.Equal(ValidationIssue.ValidationSeverity.Error, remainderError.Severity);
        Assert.Contains("must be between 0 and 3", remainderError.Message);
        
        // HASH partition с отрицательным REMAINDER - error
        var negativeRemainderBlock = CreateBlocks("CREATE TABLE users_p0 PARTITION OF users FOR VALUES WITH (MODULUS 4, REMAINDER -1);");
        var negativeRemainderResult = _extractor.Extract(negativeRemainderBlock);
        
        Assert.False(negativeRemainderResult.IsSuccess);
        
        // Invalid syntax - missing FOR VALUES
        var invalidSyntaxBlock = CreateBlocks("CREATE TABLE sales_2023 PARTITION OF sales_data FROM ('2023-01-01') TO ('2024-01-01');");
        var invalidSyntaxResult = _extractor.Extract(invalidSyntaxBlock);
        
        Assert.False(invalidSyntaxResult.IsSuccess);
        Assert.Null(invalidSyntaxResult.Definition);
        
        // Invalid syntax - unsupported format
        var unsupportedBlock = CreateBlocks("CREATE TABLE sales_2023 PARTITION OF sales_data FOR VALUES SOMETHING ELSE;");
        var unsupportedResult = _extractor.Extract(unsupportedBlock);
        
        Assert.False(unsupportedResult.IsSuccess);
        var parseError = unsupportedResult.ValidationIssues.First(i => i.Code == "PARTITION_PARSE_ERROR");
        Assert.Equal(ValidationIssue.ValidationSeverity.Error, parseError.Severity);
        Assert.Contains("Unsupported partition format", parseError.Message);
        
        // Non-partition block
        var nonPartitionBlock = CreateBlocks("CREATE TABLE users (id INT PRIMARY KEY);");
        var nonPartitionResult = _extractor.Extract(nonPartitionBlock);
        
        Assert.False(nonPartitionResult.IsSuccess);
        Assert.Null(nonPartitionResult.Definition);
        
        // Null blocks - exception
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
        
        // Empty blocks collection
        var emptyBlocks = Array.Empty<SqlBlock>();
        Assert.False(_extractor.CanExtract(emptyBlocks));
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
