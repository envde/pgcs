using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для универсального CommentExtractor
/// </summary>
public sealed class CommentExtractorTests
{
    private readonly IExtractor<CommentDefinition> _extractor = new CommentExtractor();

    #region Helper Methods

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? comment = null)
    {
        return new List<SqlBlock>
        {
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                HeaderComment = comment,
                StartLine = 1,
                EndLine = 1
            }
        };
    }

    #endregion

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithTableComment_ReturnsTrue()
    {
        // Arrange
        var blocks = CreateBlocks("COMMENT ON TABLE users IS 'User accounts';");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithColumnComment_ReturnsTrue()
    {
        // Arrange
        var blocks = CreateBlocks("COMMENT ON COLUMN users.email IS 'User email address';");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithFunctionComment_ReturnsTrue()
    {
        // Arrange
        var blocks = CreateBlocks("COMMENT ON FUNCTION calculate_total(integer) IS 'Calculates order total';");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithNonCommentSql_ReturnsFalse()
    {
        // Arrange
        var blocks = CreateBlocks("CREATE TABLE users (id INT);");

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExtract_WithNullBlocks_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
    }

    [Fact]
    public void CanExtract_WithEmptyBlocks_ReturnsFalse()
    {
        // Arrange
        var blocks = new List<SqlBlock>();

        // Act
        var result = _extractor.CanExtract(blocks);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region TABLE Comment Tests

    [Fact]
    public void Extract_TableComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS 'User accounts table';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Tables, result.Definition.ObjectType);
        Assert.Equal("users", result.Definition.Name);
        Assert.Equal("User accounts table", result.Definition.Comment);
        Assert.Null(result.Definition.Schema);
        Assert.Null(result.Definition.TableName);
        Assert.Null(result.Definition.FunctionSignature);
    }

    [Fact]
    public void Extract_TableCommentWithSchema_IncludesSchema()
    {
        // Arrange
        var sql = "COMMENT ON TABLE public.users IS 'Public user accounts';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Tables, result.Definition.ObjectType);
        Assert.Equal("users", result.Definition.Name);
        Assert.Equal("public", result.Definition.Schema);
        Assert.Equal("Public user accounts", result.Definition.Comment);
    }

    [Fact]
    public void Extract_TableCommentWithSemicolon_ParsesCorrectly()
    {
        // Arrange
        var sql = "COMMENT ON TABLE orders IS 'Customer orders';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("orders", result.Definition!.Name);
    }

    #endregion

    #region COLUMN Comment Tests

    [Fact]
    public void Extract_ColumnComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON COLUMN users.email IS 'User email address';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Columns, result.Definition.ObjectType);
        Assert.Equal("email", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal("User email address", result.Definition.Comment);
        Assert.Null(result.Definition.Schema);
    }

    [Fact]
    public void Extract_ColumnCommentWithSchema_IncludesSchemaAndTable()
    {
        // Arrange
        var sql = "COMMENT ON COLUMN public.users.email IS 'Primary email';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Columns, result.Definition.ObjectType);
        Assert.Equal("email", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal("public", result.Definition.Schema);
        Assert.Equal("Primary email", result.Definition.Comment);
    }

    #endregion

    #region FUNCTION Comment Tests

    [Fact]
    public void Extract_FunctionComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON FUNCTION calculate_total(integer, numeric) IS 'Calculates order total';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Functions, result.Definition.ObjectType);
        Assert.Equal("calculate_total", result.Definition.Name);
        Assert.Equal("calculate_total(integer, numeric)", result.Definition.FunctionSignature);
        Assert.Equal("Calculates order total", result.Definition.Comment);
    }

    [Fact]
    public void Extract_FunctionCommentWithoutParams_ParsesCorrectly()
    {
        // Arrange
        var sql = "COMMENT ON FUNCTION get_current_user() IS 'Returns current user';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Functions, result.Definition.ObjectType);
        Assert.Equal("get_current_user", result.Definition.Name);
        Assert.Equal("get_current_user()", result.Definition.FunctionSignature);
    }

    [Fact]
    public void Extract_ProcedureComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON PROCEDURE update_user(integer, text) IS 'Updates user data';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Functions, result.Definition.ObjectType);
        Assert.Equal("update_user", result.Definition.Name);
        Assert.Equal("update_user(integer, text)", result.Definition.FunctionSignature);
    }

    [Fact]
    public void Extract_FunctionCommentWithSchema_IncludesSchema()
    {
        // Arrange
        var sql = "COMMENT ON FUNCTION public.calc_tax(numeric) IS 'Tax calculator';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("public", result.Definition!.Schema);
        Assert.Equal("calc_tax", result.Definition.Name);
    }

    #endregion

    #region INDEX Comment Tests

    [Fact]
    public void Extract_IndexComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON INDEX idx_users_email IS 'Email lookup index';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Indexes, result.Definition.ObjectType);
        Assert.Equal("idx_users_email", result.Definition.Name);
        Assert.Equal("Email lookup index", result.Definition.Comment);
        Assert.Null(result.Definition.TableName);
    }

    [Fact]
    public void Extract_IndexCommentWithSchema_IncludesSchema()
    {
        // Arrange
        var sql = "COMMENT ON INDEX public.idx_orders_date IS 'Date range index';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("public", result.Definition!.Schema);
        Assert.Equal("idx_orders_date", result.Definition.Name);
    }

    #endregion

    #region TRIGGER Comment Tests

    [Fact]
    public void Extract_TriggerComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TRIGGER update_timestamp ON users IS 'Updates timestamp on change';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Triggers, result.Definition.ObjectType);
        Assert.Equal("update_timestamp", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal("Updates timestamp on change", result.Definition.Comment);
    }

    [Fact]
    public void Extract_TriggerCommentWithSchema_IncludesSchemaAndTable()
    {
        // Arrange
        var sql = "COMMENT ON TRIGGER audit_trigger ON public.orders IS 'Audit log trigger';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("audit_trigger", result.Definition!.Name);
        Assert.Equal("orders", result.Definition.TableName);
        Assert.Equal("public", result.Definition.Schema);
    }

    #endregion

    #region CONSTRAINT Comment Tests

    [Fact]
    public void Extract_ConstraintComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON CONSTRAINT users_email_check ON users IS 'Email format validation';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Constraints, result.Definition.ObjectType);
        Assert.Equal("users_email_check", result.Definition.Name);
        Assert.Equal("users", result.Definition.TableName);
        Assert.Equal("Email format validation", result.Definition.Comment);
    }

    [Fact]
    public void Extract_ConstraintCommentWithSchema_IncludesSchemaAndTable()
    {
        // Arrange
        var sql = "COMMENT ON CONSTRAINT orders_total_check ON public.orders IS 'Total must be positive';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("orders_total_check", result.Definition!.Name);
        Assert.Equal("orders", result.Definition.TableName);
        Assert.Equal("public", result.Definition.Schema);
    }

    #endregion

    #region TYPE Comment Tests

    [Fact]
    public void Extract_TypeComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON TYPE address IS 'Address composite type';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Types, result.Definition.ObjectType);
        Assert.Equal("address", result.Definition.Name);
        Assert.Equal("Address composite type", result.Definition.Comment);
    }

    [Fact]
    public void Extract_TypeCommentWithSchema_IncludesSchema()
    {
        // Arrange
        var sql = "COMMENT ON TYPE public.user_status IS 'User status enum';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("public", result.Definition!.Schema);
        Assert.Equal("user_status", result.Definition.Name);
    }

    #endregion

    #region VIEW Comment Tests

    [Fact]
    public void Extract_ViewComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON VIEW active_users IS 'View of active users only';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Views, result.Definition.ObjectType);
        Assert.Equal("active_users", result.Definition.Name);
        Assert.Equal("View of active users only", result.Definition.Comment);
    }

    [Fact]
    public void Extract_MaterializedViewComment_ReturnsValidDefinition()
    {
        // Arrange
        var sql = "COMMENT ON MATERIALIZED VIEW order_summary IS 'Materialized order stats';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(SchemaObjectType.Views, result.Definition.ObjectType);
        Assert.Equal("order_summary", result.Definition.Name);
    }

    [Fact]
    public void Extract_ViewCommentWithSchema_IncludesSchema()
    {
        // Arrange
        var sql = "COMMENT ON VIEW public.recent_orders IS 'Last 30 days orders';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("public", result.Definition!.Schema);
        Assert.Equal("recent_orders", result.Definition.Name);
    }

    #endregion

    #region Special Characters Tests

    [Fact]
    public void Extract_CommentWithEscapedQuotes_UnescapesCorrectly()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS 'User''s account table';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("User's account table", result.Definition!.Comment);
    }

    [Fact]
    public void Extract_CommentWithMultipleEscapedQuotes_UnescapesAll()
    {
        // Arrange
        var sql = "COMMENT ON TABLE orders IS 'Customer''s order''s data';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Customer's order's data", result.Definition!.Comment);
    }

    [Fact]
    public void Extract_CommentWithSpecialCharacters_PreservesText()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS 'Users: @admin #tags $prices & more!';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Users: @admin #tags $prices & more!", result.Definition!.Comment);
    }

    #endregion

    #region Header Comment Tests

    [Fact]
    public void Extract_WithHeaderComment_PreservesHeaderComment()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS 'User accounts';";
        var blocks = CreateBlocks(sql, "-- This is a header comment");

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("-- This is a header comment", result.Definition!.SqlComment);
    }

    [Fact]
    public void Extract_WithoutHeaderComment_SqlCommentIsNull()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS 'User accounts';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Definition!.SqlComment);
    }

    #endregion

    #region ValidationIssue Tests

    [Fact]
    public void Extract_EmptyComment_ReturnsWarning()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS '';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        Assert.Contains(result.ValidationIssues, v => 
            v.Code == "COMMENT_EMPTY" && 
            v.Severity == ValidationIssue.ValidationSeverity.Warning);
    }

    [Fact]
    public void Extract_WhitespaceOnlyComment_ReturnsWarning()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS '   ';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        Assert.Contains(result.ValidationIssues, v => v.Code == "COMMENT_EMPTY");
    }

    [Fact]
    public void Extract_VeryLongComment_ReturnsWarning()
    {
        // Arrange
        var longComment = new string('A', 1500);
        var sql = $"COMMENT ON TABLE users IS '{longComment}';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        Assert.Contains(result.ValidationIssues, v => 
            v.Code == "COMMENT_TOO_LONG" && 
            v.Severity == ValidationIssue.ValidationSeverity.Warning);
    }

    [Fact]
    public void Extract_ValidComment_NoValidationIssues()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS 'User accounts table';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.ValidationIssues);
    }

    #endregion

    #region Error Cases Tests

    [Fact]
    public void Extract_InvalidSql_ReturnsFailure()
    {
        // Arrange
        var sql = "COMMENT ON SOMETHING invalid IS 'text';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        Assert.Contains(result.ValidationIssues, v => v.Code == "COMMENT_PARSE_ERROR");
    }

    [Fact]
    public void Extract_NotApplicable_ReturnsNotApplicable()
    {
        // Arrange
        var sql = "CREATE TABLE users (id INT);";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Definition);
    }

    [Fact]
    public void Extract_NullBlocks_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    #endregion

    #region RawSql Preservation Tests

    [Fact]
    public void Extract_PreservesRawSql()
    {
        // Arrange
        var sql = "COMMENT ON TABLE users IS 'User accounts';";
        var blocks = CreateBlocks(sql);

        // Act
        var result = _extractor.Extract(blocks);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(sql, result.Definition!.RawSql);
    }

    #endregion
}
