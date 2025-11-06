using PgCs.Core.Extraction;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для TriggerExtractor
/// Покрывает все типы триггеров, события, условия WHEN, UPDATE OF, и специальные форматы комментариев
/// </summary>
public sealed class TriggerExtractorTests
{
    private readonly IExtractor<TriggerDefinition> _extractor = new TriggerExtractor();

    [Fact]
    public void Extract_BasicTriggers_HandlesAllTimingsAndEvents()
    {
        // Покрывает: BEFORE/AFTER/INSTEAD OF, INSERT/UPDATE/DELETE/TRUNCATE, single/multiple events, ROW/STATEMENT level

        // BEFORE INSERT
        var beforeInsertBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_validate_user
                BEFORE INSERT ON users
                FOR EACH ROW
                EXECUTE FUNCTION validate_user_data();
        ");
        var beforeInsertResult = _extractor.Extract(beforeInsertBlocks);
        Assert.True(beforeInsertResult.IsSuccess);
        var beforeInsert = beforeInsertResult.Definition;
        Assert.NotNull(beforeInsert);
        Assert.Equal("trigger_validate_user", beforeInsert.Name);
        Assert.Equal("users", beforeInsert.TableName);
        Assert.Equal(TriggerTiming.Before, beforeInsert.Timing);
        Assert.Single(beforeInsert.Events);
        Assert.Contains(TriggerEvent.Insert, beforeInsert.Events);
        Assert.Equal("validate_user_data", beforeInsert.FunctionName);
        Assert.Equal(TriggerLevel.Row, beforeInsert.Level);
        Assert.Null(beforeInsert.WhenCondition);

        // AFTER UPDATE
        var afterUpdateBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_log_changes
                AFTER UPDATE ON orders
                FOR EACH ROW
                EXECUTE FUNCTION log_order_changes();
        ");
        var afterUpdateResult = _extractor.Extract(afterUpdateBlocks);
        Assert.True(afterUpdateResult.IsSuccess);
        Assert.Equal(TriggerTiming.After, afterUpdateResult.Definition!.Timing);
        Assert.Contains(TriggerEvent.Update, afterUpdateResult.Definition.Events);

        // BEFORE DELETE
        var beforeDeleteBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_archive_data
                BEFORE DELETE ON products
                FOR EACH ROW
                EXECUTE FUNCTION archive_product();
        ");
        var beforeDeleteResult = _extractor.Extract(beforeDeleteBlocks);
        Assert.True(beforeDeleteResult.IsSuccess);
        Assert.Contains(TriggerEvent.Delete, beforeDeleteResult.Definition!.Events);

        // AFTER TRUNCATE (STATEMENT level)
        var truncateBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_log_truncate
                AFTER TRUNCATE ON audit_table
                FOR EACH STATEMENT
                EXECUTE FUNCTION log_truncate();
        ");
        var truncateResult = _extractor.Extract(truncateBlocks);
        Assert.True(truncateResult.IsSuccess);
        Assert.Contains(TriggerEvent.Truncate, truncateResult.Definition!.Events);
        Assert.Equal(TriggerLevel.Statement, truncateResult.Definition.Level);

        // Multiple events: INSERT OR UPDATE OR DELETE
        var multiEventBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_audit_changes
                AFTER INSERT OR UPDATE OR DELETE ON audit_log
                FOR EACH ROW
                EXECUTE FUNCTION audit_changes();
        ");
        var multiEventResult = _extractor.Extract(multiEventBlocks);
        Assert.True(multiEventResult.IsSuccess);
        var multiEvent = multiEventResult.Definition;
        Assert.NotNull(multiEvent);
        Assert.Equal(3, multiEvent.Events.Count);
        Assert.Contains(TriggerEvent.Insert, multiEvent.Events);
        Assert.Contains(TriggerEvent.Update, multiEvent.Events);
        Assert.Contains(TriggerEvent.Delete, multiEvent.Events);

        // All events: INSERT OR UPDATE OR DELETE OR TRUNCATE
        var allEventsBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_audit_all
                AFTER INSERT OR UPDATE OR DELETE OR TRUNCATE ON audit_table
                FOR EACH STATEMENT
                EXECUTE FUNCTION log_all_changes();
        ");
        var allEventsResult = _extractor.Extract(allEventsBlocks);
        Assert.True(allEventsResult.IsSuccess);
        Assert.Equal(4, allEventsResult.Definition!.Events.Count);
        Assert.Contains(TriggerEvent.Truncate, allEventsResult.Definition.Events);

        // INSTEAD OF (for views)
        var insteadOfBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_view_insert
                INSTEAD OF INSERT ON user_view
                FOR EACH ROW
                EXECUTE FUNCTION handle_view_insert();
        ");
        var insteadOfResult = _extractor.Extract(insteadOfBlocks);
        Assert.True(insteadOfResult.IsSuccess);
        Assert.Equal(TriggerTiming.InsteadOf, insteadOfResult.Definition!.Timing);
    }

    [Fact]
    public void Extract_UpdateOfColumns_HandlesColumnSpecifications()
    {
        // Покрывает: UPDATE OF single/multiple columns

        // Single column
        var singleColumnBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_status
                AFTER UPDATE OF status ON orders
                FOR EACH ROW
                EXECUTE FUNCTION handle_status_change();
        ");
        var singleColumnResult = _extractor.Extract(singleColumnBlocks);
        Assert.True(singleColumnResult.IsSuccess);
        var singleColumn = singleColumnResult.Definition;
        Assert.NotNull(singleColumn);
        Assert.NotNull(singleColumn.UpdateColumns);
        Assert.Single(singleColumn.UpdateColumns);
        Assert.Contains("status", singleColumn.UpdateColumns);

        // Multiple columns
        var multiColumnBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_name_email
                BEFORE UPDATE OF username, email, full_name ON users
                FOR EACH ROW
                EXECUTE FUNCTION validate_user_info();
        ");
        var multiColumnResult = _extractor.Extract(multiColumnBlocks);
        Assert.True(multiColumnResult.IsSuccess);
        var multiColumn = multiColumnResult.Definition;
        Assert.NotNull(multiColumn);
        Assert.NotNull(multiColumn.UpdateColumns);
        Assert.Equal(3, multiColumn.UpdateColumns.Count);
        Assert.Contains("username", multiColumn.UpdateColumns);
        Assert.Contains("email", multiColumn.UpdateColumns);
        Assert.Contains("full_name", multiColumn.UpdateColumns);

        // UPDATE OF with multiple events
        var updateOfMultiEventBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_category_search
                BEFORE INSERT OR UPDATE OF name, description
                ON categories
                FOR EACH ROW
                EXECUTE FUNCTION update_category_search_vector();
        ");
        var updateOfMultiResult = _extractor.Extract(updateOfMultiEventBlocks);
        Assert.True(updateOfMultiResult.IsSuccess);
        var updateOfMulti = updateOfMultiResult.Definition;
        Assert.NotNull(updateOfMulti);
        Assert.Equal(2, updateOfMulti.Events.Count);
        Assert.Contains(TriggerEvent.Insert, updateOfMulti.Events);
        Assert.Contains(TriggerEvent.Update, updateOfMulti.Events);
        Assert.NotNull(updateOfMulti.UpdateColumns);
        Assert.Equal(2, updateOfMulti.UpdateColumns.Count);
        Assert.Contains("name", updateOfMulti.UpdateColumns);
        Assert.Contains("description", updateOfMulti.UpdateColumns);
    }

    [Fact]
    public void Extract_WhenConditions_HandlesAllConditionTypes()
    {
        // Покрывает: WHEN с различными условиями (OLD/NEW, IS DISTINCT FROM, сравнения, AND/OR)

        // Simple WHEN with IS DISTINCT FROM
        var distinctBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_order_status_history
                BEFORE UPDATE OF status ON orders
                FOR EACH ROW
                WHEN (OLD.status IS DISTINCT FROM NEW.status)
                EXECUTE FUNCTION add_order_status_history();
        ");
        var distinctResult = _extractor.Extract(distinctBlocks);
        Assert.True(distinctResult.IsSuccess);
        var distinct = distinctResult.Definition;
        Assert.NotNull(distinct);
        Assert.Equal("trigger_order_status_history", distinct.Name);
        Assert.NotNull(distinct.WhenCondition);
        Assert.Equal("OLD.status IS DISTINCT FROM NEW.status", distinct.WhenCondition);
        Assert.NotNull(distinct.UpdateColumns);
        Assert.Contains("status", distinct.UpdateColumns);

        // WHEN with comparison and AND
        var complexConditionBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_check_balance
                BEFORE UPDATE ON users
                FOR EACH ROW
                WHEN (NEW.balance < 0 AND OLD.balance >= 0)
                EXECUTE FUNCTION notify_negative_balance();
        ");
        var complexResult = _extractor.Extract(complexConditionBlocks);
        Assert.True(complexResult.IsSuccess);
        Assert.NotNull(complexResult.Definition);
        Assert.NotNull(complexResult.Definition.WhenCondition);
        Assert.Equal("NEW.balance < 0 AND OLD.balance >= 0", complexResult.Definition.WhenCondition);

        // WHEN with NULL checks
        var nullCheckBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_set_defaults
                BEFORE INSERT ON users
                FOR EACH ROW
                WHEN (NEW.created_at IS NULL)
                EXECUTE FUNCTION set_created_at();
        ");
        var nullCheckResult = _extractor.Extract(nullCheckBlocks);
        Assert.True(nullCheckResult.IsSuccess);
        Assert.NotNull(nullCheckResult.Definition);
        Assert.NotNull(nullCheckResult.Definition.WhenCondition);
        Assert.Contains("IS NULL", nullCheckResult.Definition.WhenCondition);

        // WHEN with OR condition
        var orConditionBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_validate_email
                BEFORE INSERT OR UPDATE ON users
                FOR EACH ROW
                WHEN (NEW.email IS NULL OR NEW.email = '')
                EXECUTE FUNCTION validate_email();
        ");
        var orResult = _extractor.Extract(orConditionBlocks);
        Assert.True(orResult.IsSuccess);
        Assert.NotNull(orResult.Definition);
        Assert.NotNull(orResult.Definition.WhenCondition);
        Assert.Contains("OR", orResult.Definition.WhenCondition);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: специальные форматы комментариев с метаданными
        // Формат 1: comment: Описание; type: ТИП; rename: НовоеИмя;
        // Формат 2: comment(...); type(...); rename(...);

        // Формат 1: comment: ...; rename: ...;
        var blocks1 = CreateBlocks(@"
            CREATE TRIGGER trigger_users_updated_at
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at_column();
        ", "comment: Автоматическое обновление даты изменения; rename: UsersUpdatedAtTrigger;");

        var result1 = _extractor.Extract(blocks1);
        Assert.True(result1.IsSuccess);
        Assert.NotNull(result1.Definition);
        Assert.NotNull(result1.Definition.SqlComment);
        Assert.Contains("comment:", result1.Definition.SqlComment);
        Assert.Contains("Автоматическое обновление даты изменения", result1.Definition.SqlComment);
        Assert.Contains("rename:", result1.Definition.SqlComment);
        Assert.Contains("UsersUpdatedAtTrigger", result1.Definition.SqlComment);

        // Формат 2: comment(...); rename(...); type(...);
        var blocks2 = CreateBlocks(@"
            CREATE TRIGGER trigger_audit_changes
                AFTER INSERT OR UPDATE OR DELETE ON audit_log
                FOR EACH ROW
                EXECUTE FUNCTION audit_changes();
        ", "comment(Триггер для аудита изменений); rename(AuditChangesTrigger); type(AuditTrigger);");

        var result2 = _extractor.Extract(blocks2);
        Assert.True(result2.IsSuccess);
        Assert.NotNull(result2.Definition);
        Assert.NotNull(result2.Definition.SqlComment);
        Assert.Contains("comment(", result2.Definition.SqlComment);
        Assert.Contains("Триггер для аудита изменений", result2.Definition.SqlComment);
        Assert.Contains("rename(AuditChangesTrigger)", result2.Definition.SqlComment);
        Assert.Contains("type(AuditTrigger)", result2.Definition.SqlComment);

        // Смешанный формат
        var blocks3 = CreateBlocks(@"
            CREATE TRIGGER trigger_validate_data
                BEFORE INSERT ON products
                FOR EACH ROW
                EXECUTE FUNCTION validate_product_data();
        ", "comment: Валидация данных перед вставкой; type(ValidationTrigger);");

        var result3 = _extractor.Extract(blocks3);
        Assert.True(result3.IsSuccess);
        Assert.NotNull(result3.Definition);
        Assert.NotNull(result3.Definition.SqlComment);
        Assert.Contains("comment:", result3.Definition.SqlComment);
        Assert.Contains("Валидация данных перед вставкой", result3.Definition.SqlComment);
        Assert.Contains("type(ValidationTrigger)", result3.Definition.SqlComment);

        // Формат 1 с WHEN условием
        var blocks4 = CreateBlocks(@"
            CREATE TRIGGER trigger_order_status_change
                BEFORE UPDATE OF status ON orders
                FOR EACH ROW
                WHEN (OLD.status IS DISTINCT FROM NEW.status)
                EXECUTE FUNCTION log_status_change();
        ", "comment: Логирование изменений статуса заказа; rename: OrderStatusChangeTrigger; type: HistoryTrigger;");

        var result4 = _extractor.Extract(blocks4);
        Assert.True(result4.IsSuccess);
        Assert.NotNull(result4.Definition);
        Assert.NotNull(result4.Definition.SqlComment);
        Assert.Contains("Логирование изменений статуса заказа", result4.Definition.SqlComment);
        Assert.Contains("rename:", result4.Definition.SqlComment);
        Assert.Contains("type:", result4.Definition.SqlComment);
    }

    [Fact]
    public void Extract_EdgeCasesAndValidation_HandlesCorrectly()
    {
        // Покрывает: null validation, non-trigger blocks, old EXECUTE PROCEDURE syntax, schema qualification, case sensitivity

        // Null checks
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));

        // Non-trigger block
        var tableBlocks = CreateBlocks(@"
            CREATE TABLE test_table (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100)
            );
        ");
        Assert.False(_extractor.CanExtract(tableBlocks));

        // Empty block
        var emptyResult = _extractor.Extract(CreateBlocks(""));
        Assert.False(emptyResult.IsSuccess);

        // Invalid SQL
        var invalidResult = _extractor.Extract(CreateBlocks("INVALID SQL STATEMENT"));
        Assert.False(invalidResult.IsSuccess);

        // Incomplete trigger (without EXECUTE)
        var incompleteBlocks = CreateBlocks(@"
            CREATE TRIGGER incomplete_trigger
                BEFORE INSERT ON users
                FOR EACH ROW;
        ");
        var incompleteResult = _extractor.Extract(incompleteBlocks);
        Assert.False(incompleteResult.IsSuccess);
        Assert.NotEmpty(incompleteResult.ValidationIssues);

        // Old EXECUTE PROCEDURE syntax (instead of EXECUTE FUNCTION)
        var oldSyntaxBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_old_syntax
                BEFORE INSERT ON legacy_table
                FOR EACH ROW
                EXECUTE PROCEDURE legacy_function();
        ");
        var oldSyntaxResult = _extractor.Extract(oldSyntaxBlocks);
        Assert.True(oldSyntaxResult.IsSuccess);
        Assert.Equal("trigger_old_syntax", oldSyntaxResult.Definition!.Name);
        Assert.Equal("legacy_function", oldSyntaxResult.Definition.FunctionName);

        // Schema qualification
        var schemaBlocks = CreateBlocks(@"
            CREATE TRIGGER trigger_with_schema
                AFTER INSERT ON myschema.users
                FOR EACH ROW
                EXECUTE FUNCTION myschema.audit_insert();
        ");
        var schemaResult = _extractor.Extract(schemaBlocks);
        Assert.True(schemaResult.IsSuccess);
        Assert.Equal("users", schemaResult.Definition!.TableName);
        Assert.Equal("myschema", schemaResult.Definition.Schema);

        // Case sensitivity
        var upperBlocks = CreateBlocks(@"
            CREATE TRIGGER TRIGGER_UPPERCASE
                BEFORE INSERT ON USERS
                FOR EACH ROW
                EXECUTE FUNCTION VALIDATE_DATA();
        ");
        var upperResult = _extractor.Extract(upperBlocks);
        Assert.True(upperResult.IsSuccess);
        Assert.Equal("TRIGGER_UPPERCASE", upperResult.Definition!.Name);
        Assert.Equal("USERS", upperResult.Definition.TableName);

        // Mixed case
        var mixedBlocks = CreateBlocks(@"
            CrEaTe TrIgGeR MixedCase
                BeFoRe InSeRt On TestTable
                FoR EaCh RoW
                ExEcUtE FuNcTiOn test_func();
        ");
        var mixedResult = _extractor.Extract(mixedBlocks);
        Assert.True(mixedResult.IsSuccess);
        Assert.Equal("MixedCase", mixedResult.Definition!.Name);
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? headerComment = null)
    {
        var content = sql.Trim();

        return [new SqlBlock
        {
            Content = content,
            RawContent = content,
            HeaderComment = headerComment,
            InlineComments = null,
            StartLine = 1,
            EndLine = content.Split('\n').Length,
            SourcePath = "test.sql"
        }];
    }
}
