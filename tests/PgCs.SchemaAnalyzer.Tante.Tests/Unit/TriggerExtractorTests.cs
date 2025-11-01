using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для TriggerExtractor
/// </summary>
public sealed class TriggerExtractorTests
{
    private readonly IExtractor<TriggerDefinition> _extractor = new TriggerExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidTriggerBlock_ReturnsTrue()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_user
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at_column();
        ");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TABLE test_table (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100)
            );
        ");

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

    #region Extract Simple Trigger Tests

    [Fact]
    public void Extract_SimpleTriggerBeforeInsert_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_validate_user
                BEFORE INSERT ON users
                FOR EACH ROW
                EXECUTE FUNCTION validate_user_data();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_validate_user", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal(TriggerTiming.Before, result.Definition.Timing);
        Assert.Single(result.Definition.Events);
        Assert.Contains(TriggerEvent.Insert, result.Definition.Events);
        Assert.Equal("validate_user_data", result.Definition.FunctionName);
        Assert.Equal(TriggerLevel.Row, result.Definition.Level);
        Assert.Null(result.Definition.WhenCondition);
        Assert.Null(result.Definition.UpdateColumns);
    }

    [Fact]
    public void Extract_TriggerAfterUpdate_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_log_changes
                AFTER UPDATE ON orders
                FOR EACH ROW
                EXECUTE FUNCTION log_order_changes();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_log_changes", result.Definition.Name);
        Assert.Equal("orders", result.Definition.TableName);
        Assert.Equal(TriggerTiming.After, result.Definition.Timing);
        Assert.Single(result.Definition.Events);
        Assert.Contains(TriggerEvent.Update, result.Definition.Events);
        Assert.Equal("log_order_changes", result.Definition.FunctionName);
        Assert.Equal(TriggerLevel.Row, result.Definition.Level);
    }

    [Fact]
    public void Extract_TriggerInsteadOf_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_view_insert
                INSTEAD OF INSERT ON user_view
                FOR EACH ROW
                EXECUTE FUNCTION insert_into_users();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_view_insert", result.Definition.Name);
        Assert.Equal("user_view", result.Definition.TableName);
        Assert.Equal(TriggerTiming.InsteadOf, result.Definition.Timing);
        Assert.Single(result.Definition.Events);
        Assert.Contains(TriggerEvent.Insert, result.Definition.Events);
        Assert.Equal("insert_into_users", result.Definition.FunctionName);
    }

    [Fact]
    public void Extract_TriggerWithSchema_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER public.trigger_update_audit
                AFTER UPDATE ON public.users
                FOR EACH ROW
                EXECUTE FUNCTION audit_log();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_update_audit", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal("public", result.Definition.Schema);
    }

    #endregion

    #region Extract Multiple Events Tests

    [Fact]
    public void Extract_TriggerWithMultipleEvents_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_category_search
                BEFORE INSERT OR UPDATE OF name, description ON categories
                FOR EACH ROW
                EXECUTE FUNCTION update_category_search_vector();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_update_category_search", result.Definition.Name);
        Assert.Equal("categories", result.Definition.TableName);
        Assert.Equal(TriggerTiming.Before, result.Definition.Timing);
        Assert.Equal(2, result.Definition.Events.Count);
        Assert.Contains(TriggerEvent.Insert, result.Definition.Events);
        Assert.Contains(TriggerEvent.Update, result.Definition.Events);
        Assert.Equal("update_category_search_vector", result.Definition.FunctionName);
        Assert.NotNull(result.Definition.UpdateColumns);
        Assert.Equal(2, result.Definition.UpdateColumns.Count);
        Assert.Contains("name", result.Definition.UpdateColumns);
        Assert.Contains("description", result.Definition.UpdateColumns);
    }

    [Fact]
    public void Extract_TriggerWithAllEvents_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_audit_all
                AFTER INSERT OR UPDATE OR DELETE OR TRUNCATE ON audit_table
                FOR EACH STATEMENT
                EXECUTE FUNCTION log_all_changes();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_audit_all", result.Definition.Name);
        Assert.Equal("audit_table", result.Definition.TableName);
        Assert.Equal(4, result.Definition.Events.Count);
        Assert.Contains(TriggerEvent.Insert, result.Definition.Events);
        Assert.Contains(TriggerEvent.Update, result.Definition.Events);
        Assert.Contains(TriggerEvent.Delete, result.Definition.Events);
        Assert.Contains(TriggerEvent.Truncate, result.Definition.Events);
        Assert.Equal(TriggerLevel.Statement, result.Definition.Level);
    }

    #endregion

    #region Extract Trigger Level Tests

    [Fact]
    public void Extract_TriggerForEachRow_ReturnsRowLevel()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_row_level
                AFTER UPDATE ON test_table
                FOR EACH ROW
                EXECUTE FUNCTION test_function();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(TriggerLevel.Row, result.Definition.Level);
    }

    [Fact]
    public void Extract_TriggerForEachStatement_ReturnsStatementLevel()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_statement_level
                AFTER TRUNCATE ON test_table
                FOR EACH STATEMENT
                EXECUTE FUNCTION test_function();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(TriggerLevel.Statement, result.Definition.Level);
    }

    #endregion

    #region Extract WHEN Condition Tests

    [Fact]
    public void Extract_TriggerWithWhenCondition_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_order_status_history
                BEFORE UPDATE OF status ON orders
                FOR EACH ROW
                WHEN (OLD.status IS DISTINCT FROM NEW.status)
                EXECUTE FUNCTION add_order_status_history();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_order_status_history", result.Definition.Name);
        Assert.Equal("orders", result.Definition.TableName);
        Assert.NotNull(result.Definition.WhenCondition);
        Assert.Equal("OLD.status IS DISTINCT FROM NEW.status", result.Definition.WhenCondition);
        Assert.NotNull(result.Definition.UpdateColumns);
        Assert.Single(result.Definition.UpdateColumns);
        Assert.Contains("status", result.Definition.UpdateColumns);
    }

    [Fact]
    public void Extract_TriggerWithComplexWhenCondition_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_check_balance
                BEFORE UPDATE ON users
                FOR EACH ROW
                WHEN (NEW.balance < 0 AND OLD.balance >= 0)
                EXECUTE FUNCTION notify_negative_balance();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.NotNull(result.Definition.WhenCondition);
        Assert.Equal("NEW.balance < 0 AND OLD.balance >= 0", result.Definition.WhenCondition);
    }

    #endregion

    #region Extract UPDATE OF Columns Tests

    [Fact]
    public void Extract_TriggerUpdateOfSingleColumn_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_status
                AFTER UPDATE OF status ON orders
                FOR EACH ROW
                EXECUTE FUNCTION handle_status_change();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.NotNull(result.Definition.UpdateColumns);
        Assert.Single(result.Definition.UpdateColumns);
        Assert.Contains("status", result.Definition.UpdateColumns);
    }

    [Fact]
    public void Extract_TriggerUpdateOfMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_name_email
                BEFORE UPDATE OF username, email, full_name ON users
                FOR EACH ROW
                EXECUTE FUNCTION validate_user_info();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.NotNull(result.Definition.UpdateColumns);
        Assert.Equal(3, result.Definition.UpdateColumns.Count);
        Assert.Contains("username", result.Definition.UpdateColumns);
        Assert.Contains("email", result.Definition.UpdateColumns);
        Assert.Contains("full_name", result.Definition.UpdateColumns);
    }

    #endregion

    #region Old Syntax Tests

    [Fact]
    public void Extract_TriggerWithExecuteProcedure_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_old_syntax
                BEFORE INSERT ON legacy_table
                FOR EACH ROW
                EXECUTE PROCEDURE legacy_function();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_old_syntax", result.Definition.Name);
        Assert.Equal("legacy_table", result.Definition.TableName);
        Assert.Equal("legacy_function", result.Definition.FunctionName);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_TriggerUpperCase_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER TRIGGER_UPPERCASE
                BEFORE INSERT ON USERS
                FOR EACH ROW
                EXECUTE FUNCTION VALIDATE_DATA();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("TRIGGER_UPPERCASE", result.Definition.Name);
        Assert.Equal("USERS", result.Definition.TableName);
        Assert.Equal("VALIDATE_DATA", result.Definition.FunctionName);
    }

    [Fact]
    public void Extract_TriggerMixedCase_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            create TrIgGeR MixedCaseTrigger
                BeFoRe UpDaTe on TestTable
                fOr EaCh RoW
                ExEcUtE fUnCtIoN testFunc();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("MixedCaseTrigger", result.Definition.Name);
        Assert.Equal("TestTable", result.Definition.TableName);
        Assert.Equal("testFunc", result.Definition.FunctionName);
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_InvalidSql_ReturnsNotApplicable()
    {
        // Arrange
        var blocks = CreateBlocks("INVALID SQL STATEMENT");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    [Fact]
    public void Extract_NullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_EmptyBlock_ReturnsNotApplicable()
    {
        // Arrange
        var blocks = CreateBlocks("");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    [Fact]
    public void Extract_TriggerWithoutFunction_ReturnsFailure()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER incomplete_trigger
                BEFORE INSERT ON users
                FOR EACH ROW;
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
        Assert.NotEmpty(result.ValidationIssues);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_RealWorldUpdatedAtTrigger_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_users_updated_at
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at_column();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_users_updated_at", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal(TriggerTiming.Before, result.Definition.Timing);
        Assert.Single(result.Definition.Events);
        Assert.Contains(TriggerEvent.Update, result.Definition.Events);
        Assert.Equal("update_updated_at_column", result.Definition.FunctionName);
        Assert.Equal(TriggerLevel.Row, result.Definition.Level);
    }

    [Fact]
    public void Extract_RealWorldSearchVectorTrigger_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_category_search
                BEFORE INSERT OR UPDATE OF name, description
                ON categories
                FOR EACH ROW
                EXECUTE FUNCTION update_category_search_vector();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_update_category_search", result.Definition.Name);
        Assert.Equal("categories", result.Definition.TableName);
        Assert.Equal(TriggerTiming.Before, result.Definition.Timing);
        Assert.Equal(2, result.Definition.Events.Count);
        Assert.Contains(TriggerEvent.Insert, result.Definition.Events);
        Assert.Contains(TriggerEvent.Update, result.Definition.Events);
        Assert.Equal("update_category_search_vector", result.Definition.FunctionName);
        Assert.NotNull(result.Definition.UpdateColumns);
        Assert.Equal(2, result.Definition.UpdateColumns.Count);
    }

    [Fact]
    public void Extract_RealWorldAuditTrigger_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_order_status_history
                BEFORE UPDATE OF status ON orders
                FOR EACH ROW
                WHEN (OLD.status IS DISTINCT FROM NEW.status)
                EXECUTE FUNCTION add_order_status_history();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_order_status_history", result.Definition.Name);
        Assert.Equal("orders", result.Definition.TableName);
        Assert.Equal(TriggerTiming.Before, result.Definition.Timing);
        Assert.Single(result.Definition.Events);
        Assert.Contains(TriggerEvent.Update, result.Definition.Events);
        Assert.Equal("add_order_status_history", result.Definition.FunctionName);
        Assert.Equal(TriggerLevel.Row, result.Definition.Level);
        Assert.NotNull(result.Definition.WhenCondition);
        Assert.Equal("OLD.status IS DISTINCT FROM NEW.status", result.Definition.WhenCondition);
        Assert.NotNull(result.Definition.UpdateColumns);
        Assert.Single(result.Definition.UpdateColumns);
        Assert.Contains("status", result.Definition.UpdateColumns);
    }

    [Fact]
    public void Extract_RealWorldCategoryPathTrigger_ReturnsValidDefinition()
    {
        // Arrange
        var blocks = CreateBlocks(@"
            CREATE TRIGGER trigger_update_category_path
                BEFORE INSERT OR UPDATE OF parent_id
                ON categories
                FOR EACH ROW
                EXECUTE FUNCTION update_category_path();
        ");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("trigger_update_category_path", result.Definition.Name);
        Assert.Equal("categories", result.Definition.TableName);
        Assert.Equal(TriggerTiming.Before, result.Definition.Timing);
        Assert.Equal(2, result.Definition.Events.Count);
        Assert.Contains(TriggerEvent.Insert, result.Definition.Events);
        Assert.Contains(TriggerEvent.Update, result.Definition.Events);
        Assert.Equal("update_category_path", result.Definition.FunctionName);
        Assert.NotNull(result.Definition.UpdateColumns);
        Assert.Single(result.Definition.UpdateColumns);
        Assert.Contains("parent_id", result.Definition.UpdateColumns);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Создает SQL блок для тестирования
    /// </summary>
    private static IReadOnlyList<SqlBlock> CreateBlocks(string content)
    {
        return [new SqlBlock
        {
            Content = content,
            RawContent = content,
            HeaderComment = null,
            InlineComments = null,
            StartLine = 1,
            EndLine = content.Split('\n').Length,
            SourcePath = "test.sql"
        }];
    }

    #endregion
}
