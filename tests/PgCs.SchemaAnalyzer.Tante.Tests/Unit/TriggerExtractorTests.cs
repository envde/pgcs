using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для TriggerExtractor
/// </summary>
public sealed class TriggerExtractorTests
{
    private readonly ITriggerExtractor _extractor = new TriggerExtractor();

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidTriggerBlock_ReturnsTrue()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_update_user
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at_column();
        ");

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TABLE test_table (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100)
            );
        ");

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

    #region Extract Simple Trigger Tests

    [Fact]
    public void Extract_SimpleTriggerBeforeInsert_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_validate_user
                BEFORE INSERT ON users
                FOR EACH ROW
                EXECUTE FUNCTION validate_user_data();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_validate_user", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(TriggerTiming.Before, result.Timing);
        Assert.Single(result.Events);
        Assert.Contains(TriggerEvent.Insert, result.Events);
        Assert.Equal("validate_user_data", result.FunctionName);
        Assert.Equal(TriggerLevel.Row, result.Level);
        Assert.Null(result.WhenCondition);
        Assert.Null(result.UpdateColumns);
    }

    [Fact]
    public void Extract_TriggerAfterUpdate_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_log_changes
                AFTER UPDATE ON orders
                FOR EACH ROW
                EXECUTE FUNCTION log_order_changes();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_log_changes", result.Name);
        Assert.Equal("orders", result.TableName);
        Assert.Equal(TriggerTiming.After, result.Timing);
        Assert.Single(result.Events);
        Assert.Contains(TriggerEvent.Update, result.Events);
        Assert.Equal("log_order_changes", result.FunctionName);
        Assert.Equal(TriggerLevel.Row, result.Level);
    }

    [Fact]
    public void Extract_TriggerInsteadOf_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_view_insert
                INSTEAD OF INSERT ON user_view
                FOR EACH ROW
                EXECUTE FUNCTION insert_into_users();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_view_insert", result.Name);
        Assert.Equal("user_view", result.TableName);
        Assert.Equal(TriggerTiming.InsteadOf, result.Timing);
        Assert.Single(result.Events);
        Assert.Contains(TriggerEvent.Insert, result.Events);
        Assert.Equal("insert_into_users", result.FunctionName);
    }

    [Fact]
    public void Extract_TriggerWithSchema_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER public.trigger_update_audit
                AFTER UPDATE ON public.users
                FOR EACH ROW
                EXECUTE FUNCTION audit_log();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_update_audit", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal("public", result.Schema);
    }

    #endregion

    #region Extract Multiple Events Tests

    [Fact]
    public void Extract_TriggerWithMultipleEvents_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_update_category_search
                BEFORE INSERT OR UPDATE OF name, description ON categories
                FOR EACH ROW
                EXECUTE FUNCTION update_category_search_vector();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_update_category_search", result.Name);
        Assert.Equal("categories", result.TableName);
        Assert.Equal(TriggerTiming.Before, result.Timing);
        Assert.Equal(2, result.Events.Count);
        Assert.Contains(TriggerEvent.Insert, result.Events);
        Assert.Contains(TriggerEvent.Update, result.Events);
        Assert.Equal("update_category_search_vector", result.FunctionName);
        Assert.NotNull(result.UpdateColumns);
        Assert.Equal(2, result.UpdateColumns.Count);
        Assert.Contains("name", result.UpdateColumns);
        Assert.Contains("description", result.UpdateColumns);
    }

    [Fact]
    public void Extract_TriggerWithAllEvents_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_audit_all
                AFTER INSERT OR UPDATE OR DELETE OR TRUNCATE ON audit_table
                FOR EACH STATEMENT
                EXECUTE FUNCTION log_all_changes();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_audit_all", result.Name);
        Assert.Equal("audit_table", result.TableName);
        Assert.Equal(4, result.Events.Count);
        Assert.Contains(TriggerEvent.Insert, result.Events);
        Assert.Contains(TriggerEvent.Update, result.Events);
        Assert.Contains(TriggerEvent.Delete, result.Events);
        Assert.Contains(TriggerEvent.Truncate, result.Events);
        Assert.Equal(TriggerLevel.Statement, result.Level);
    }

    #endregion

    #region Extract Trigger Level Tests

    [Fact]
    public void Extract_TriggerForEachRow_ReturnsRowLevel()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_row_level
                AFTER UPDATE ON test_table
                FOR EACH ROW
                EXECUTE FUNCTION test_function();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TriggerLevel.Row, result.Level);
    }

    [Fact]
    public void Extract_TriggerForEachStatement_ReturnsStatementLevel()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_statement_level
                AFTER TRUNCATE ON test_table
                FOR EACH STATEMENT
                EXECUTE FUNCTION test_function();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TriggerLevel.Statement, result.Level);
    }

    #endregion

    #region Extract WHEN Condition Tests

    [Fact]
    public void Extract_TriggerWithWhenCondition_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_order_status_history
                BEFORE UPDATE OF status ON orders
                FOR EACH ROW
                WHEN (OLD.status IS DISTINCT FROM NEW.status)
                EXECUTE FUNCTION add_order_status_history();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_order_status_history", result.Name);
        Assert.Equal("orders", result.TableName);
        Assert.NotNull(result.WhenCondition);
        Assert.Equal("OLD.status IS DISTINCT FROM NEW.status", result.WhenCondition);
        Assert.NotNull(result.UpdateColumns);
        Assert.Single(result.UpdateColumns);
        Assert.Contains("status", result.UpdateColumns);
    }

    [Fact]
    public void Extract_TriggerWithComplexWhenCondition_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_check_balance
                BEFORE UPDATE ON users
                FOR EACH ROW
                WHEN (NEW.balance < 0 AND OLD.balance >= 0)
                EXECUTE FUNCTION notify_negative_balance();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.WhenCondition);
        Assert.Equal("NEW.balance < 0 AND OLD.balance >= 0", result.WhenCondition);
    }

    #endregion

    #region Extract UPDATE OF Columns Tests

    [Fact]
    public void Extract_TriggerUpdateOfSingleColumn_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_update_status
                AFTER UPDATE OF status ON orders
                FOR EACH ROW
                EXECUTE FUNCTION handle_status_change();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.UpdateColumns);
        Assert.Single(result.UpdateColumns);
        Assert.Contains("status", result.UpdateColumns);
    }

    [Fact]
    public void Extract_TriggerUpdateOfMultipleColumns_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_update_name_email
                BEFORE UPDATE OF username, email, full_name ON users
                FOR EACH ROW
                EXECUTE FUNCTION validate_user_info();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.UpdateColumns);
        Assert.Equal(3, result.UpdateColumns.Count);
        Assert.Contains("username", result.UpdateColumns);
        Assert.Contains("email", result.UpdateColumns);
        Assert.Contains("full_name", result.UpdateColumns);
    }

    #endregion

    #region Old Syntax Tests

    [Fact]
    public void Extract_TriggerWithExecuteProcedure_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_old_syntax
                BEFORE INSERT ON legacy_table
                FOR EACH ROW
                EXECUTE PROCEDURE legacy_function();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_old_syntax", result.Name);
        Assert.Equal("legacy_table", result.TableName);
        Assert.Equal("legacy_function", result.FunctionName);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Extract_TriggerUpperCase_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER TRIGGER_UPPERCASE
                BEFORE INSERT ON USERS
                FOR EACH ROW
                EXECUTE FUNCTION VALIDATE_DATA();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TRIGGER_UPPERCASE", result.Name);
        Assert.Equal("USERS", result.TableName);
        Assert.Equal("VALIDATE_DATA", result.FunctionName);
    }

    [Fact]
    public void Extract_TriggerMixedCase_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            create TrIgGeR MixedCaseTrigger
                BeFoRe UpDaTe on TestTable
                fOr EaCh RoW
                ExEcUtE fUnCtIoN testFunc();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MixedCaseTrigger", result.Name);
        Assert.Equal("TestTable", result.TableName);
        Assert.Equal("testFunc", result.FunctionName);
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_InvalidSql_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("INVALID SQL STATEMENT");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_NullBlock_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    [Fact]
    public void Extract_EmptyBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_TriggerWithoutFunction_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER incomplete_trigger
                BEFORE INSERT ON users
                FOR EACH ROW;
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Real World Examples Tests

    [Fact]
    public void Extract_RealWorldUpdatedAtTrigger_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_users_updated_at
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at_column();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_users_updated_at", result.Name);
        Assert.Equal("users", result.TableName);
        Assert.Equal(TriggerTiming.Before, result.Timing);
        Assert.Single(result.Events);
        Assert.Contains(TriggerEvent.Update, result.Events);
        Assert.Equal("update_updated_at_column", result.FunctionName);
        Assert.Equal(TriggerLevel.Row, result.Level);
    }

    [Fact]
    public void Extract_RealWorldSearchVectorTrigger_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_update_category_search
                BEFORE INSERT OR UPDATE OF name, description
                ON categories
                FOR EACH ROW
                EXECUTE FUNCTION update_category_search_vector();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_update_category_search", result.Name);
        Assert.Equal("categories", result.TableName);
        Assert.Equal(TriggerTiming.Before, result.Timing);
        Assert.Equal(2, result.Events.Count);
        Assert.Contains(TriggerEvent.Insert, result.Events);
        Assert.Contains(TriggerEvent.Update, result.Events);
        Assert.Equal("update_category_search_vector", result.FunctionName);
        Assert.NotNull(result.UpdateColumns);
        Assert.Equal(2, result.UpdateColumns.Count);
    }

    [Fact]
    public void Extract_RealWorldAuditTrigger_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_order_status_history
                BEFORE UPDATE OF status ON orders
                FOR EACH ROW
                WHEN (OLD.status IS DISTINCT FROM NEW.status)
                EXECUTE FUNCTION add_order_status_history();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_order_status_history", result.Name);
        Assert.Equal("orders", result.TableName);
        Assert.Equal(TriggerTiming.Before, result.Timing);
        Assert.Single(result.Events);
        Assert.Contains(TriggerEvent.Update, result.Events);
        Assert.Equal("add_order_status_history", result.FunctionName);
        Assert.Equal(TriggerLevel.Row, result.Level);
        Assert.NotNull(result.WhenCondition);
        Assert.Equal("OLD.status IS DISTINCT FROM NEW.status", result.WhenCondition);
        Assert.NotNull(result.UpdateColumns);
        Assert.Single(result.UpdateColumns);
        Assert.Contains("status", result.UpdateColumns);
    }

    [Fact]
    public void Extract_RealWorldCategoryPathTrigger_ReturnsValidDefinition()
    {
        // Arrange
        var block = CreateBlock(@"
            CREATE TRIGGER trigger_update_category_path
                BEFORE INSERT OR UPDATE OF parent_id
                ON categories
                FOR EACH ROW
                EXECUTE FUNCTION update_category_path();
        ");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("trigger_update_category_path", result.Name);
        Assert.Equal("categories", result.TableName);
        Assert.Equal(TriggerTiming.Before, result.Timing);
        Assert.Equal(2, result.Events.Count);
        Assert.Contains(TriggerEvent.Insert, result.Events);
        Assert.Contains(TriggerEvent.Update, result.Events);
        Assert.Equal("update_category_path", result.FunctionName);
        Assert.NotNull(result.UpdateColumns);
        Assert.Single(result.UpdateColumns);
        Assert.Contains("parent_id", result.UpdateColumns);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Создает SQL блок для тестирования
    /// </summary>
    private static SqlBlock CreateBlock(string content)
    {
        return new SqlBlock
        {
            Content = content,
            RawContent = content,
            HeaderComment = null,
            InlineComments = null,
            StartLine = 1,
            EndLine = content.Split('\n').Length,
            SourcePath = "test.sql"
        };
    }

    #endregion
}
