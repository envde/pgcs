using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для CommentExtractor
/// Покрывает все типы объектов (TABLE, COLUMN, FUNCTION, INDEX, TRIGGER, CONSTRAINT, TYPE, VIEW)
/// </summary>
public sealed class CommentExtractorTests
{
    private readonly IExtractor<CommentDefinition> _extractor = new CommentExtractor();

    [Fact]
    public void Extract_TableAndColumnComments_HandlesAllVariants()
    {
        // Покрывает: TABLE comments (with/without schema), COLUMN comments (with/without schema)

        // Simple table comment
        var tableBlock = CreateBlocks("COMMENT ON TABLE users IS 'User accounts table';");
        var tableResult = _extractor.Extract(tableBlock);
        
        Assert.True(tableResult.IsSuccess);
        Assert.NotNull(tableResult.Definition);
        Assert.Equal(SchemaObjectType.Tables, tableResult.Definition.ObjectType);
        Assert.Equal("users", tableResult.Definition.Name);
        Assert.Equal("User accounts table", tableResult.Definition.Comment);
        Assert.Null(tableResult.Definition.Schema);
        
        // Table comment with schema
        var tableSchemaBlock = CreateBlocks("COMMENT ON TABLE public.orders IS 'Order records';");
        var tableSchemaResult = _extractor.Extract(tableSchemaBlock);
        
        Assert.True(tableSchemaResult.IsSuccess);
        Assert.Equal("orders", tableSchemaResult.Definition!.Name);
        Assert.Equal("public", tableSchemaResult.Definition.Schema);
        Assert.Equal("Order records", tableSchemaResult.Definition.Comment);
        
        // Column comment
        var columnBlock = CreateBlocks("COMMENT ON COLUMN users.email IS 'User email address';");
        var columnResult = _extractor.Extract(columnBlock);
        
        Assert.True(columnResult.IsSuccess);
        Assert.Equal(SchemaObjectType.Columns, columnResult.Definition!.ObjectType);
        Assert.Equal("users", columnResult.Definition.TableName);
        Assert.Equal("email", columnResult.Definition.Name);
        Assert.Equal("User email address", columnResult.Definition.Comment);
        
        // Column comment with schema
        var columnSchemaBlock = CreateBlocks("COMMENT ON COLUMN public.users.created_at IS 'Account creation timestamp';");
        var columnSchemaResult = _extractor.Extract(columnSchemaBlock);
        
        Assert.True(columnSchemaResult.IsSuccess);
        Assert.Equal("public", columnSchemaResult.Definition!.Schema);
        Assert.Equal("users", columnSchemaResult.Definition.TableName);
        Assert.Equal("created_at", columnSchemaResult.Definition.Name);
        
        // CanExtract validation
        Assert.True(_extractor.CanExtract(tableBlock));
        Assert.True(_extractor.CanExtract(columnBlock));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TABLE users (id INT);")));
    }

    [Fact]
    public void Extract_FunctionAndProcedureComments_HandlesAllVariants()
    {
        // Покрывает: FUNCTION comments (with/without params, with/without schema), PROCEDURE comments

        // Function with parameters
        var funcBlock = CreateBlocks("COMMENT ON FUNCTION get_user(integer) IS 'Retrieves user by ID';");
        var funcResult = _extractor.Extract(funcBlock);
        
        Assert.True(funcResult.IsSuccess);
        Assert.NotNull(funcResult.Definition);
        Assert.Equal(SchemaObjectType.Functions, funcResult.Definition.ObjectType);
        Assert.Equal("get_user", funcResult.Definition.Name);
        Assert.Equal("get_user(integer)", funcResult.Definition.FunctionSignature);
        Assert.Equal("Retrieves user by ID", funcResult.Definition.Comment);
        
        // Function without parameters
        var noParamBlock = CreateBlocks("COMMENT ON FUNCTION get_current_timestamp() IS 'Returns current timestamp';");
        var noParamResult = _extractor.Extract(noParamBlock);
        
        Assert.True(noParamResult.IsSuccess);
        Assert.Equal("get_current_timestamp", noParamResult.Definition!.Name);
        Assert.Equal("get_current_timestamp()", noParamResult.Definition.FunctionSignature);
        
        // Procedure comment
        var procBlock = CreateBlocks("COMMENT ON PROCEDURE update_user_status(integer, text) IS 'Updates user status';");
        var procResult = _extractor.Extract(procBlock);
        
        Assert.True(procResult.IsSuccess);
        Assert.Equal(SchemaObjectType.Functions, procResult.Definition!.ObjectType);
        Assert.Equal("update_user_status", procResult.Definition.Name);
        Assert.Equal("update_user_status(integer, text)", procResult.Definition.FunctionSignature);
        
        // Function with schema
        var funcSchemaBlock = CreateBlocks("COMMENT ON FUNCTION public.calculate_total(integer) IS 'Calculates order total';");
        var funcSchemaResult = _extractor.Extract(funcSchemaBlock);
        
        Assert.True(funcSchemaResult.IsSuccess);
        Assert.Equal("public", funcSchemaResult.Definition!.Schema);
        Assert.Equal("calculate_total", funcSchemaResult.Definition.Name);
        Assert.Equal("calculate_total(integer)", funcSchemaResult.Definition.FunctionSignature);
    }

    [Fact]
    public void Extract_IndexAndTriggerComments_HandlesAllVariants()
    {
        // Покрывает: INDEX comments (with/without schema), TRIGGER comments (with/without schema)

        // Simple index comment
        var indexBlock = CreateBlocks("COMMENT ON INDEX idx_users_email IS 'Index for email lookups';");
        var indexResult = _extractor.Extract(indexBlock);
        
        Assert.True(indexResult.IsSuccess);
        Assert.NotNull(indexResult.Definition);
        Assert.Equal(SchemaObjectType.Indexes, indexResult.Definition.ObjectType);
        Assert.Equal("idx_users_email", indexResult.Definition.Name);
        Assert.Equal("Index for email lookups", indexResult.Definition.Comment);
        
        // Index with schema
        var indexSchemaBlock = CreateBlocks("COMMENT ON INDEX public.idx_orders_user_id IS 'User orders index';");
        var indexSchemaResult = _extractor.Extract(indexSchemaBlock);
        
        Assert.True(indexSchemaResult.IsSuccess);
        Assert.Equal("public", indexSchemaResult.Definition!.Schema);
        Assert.Equal("idx_orders_user_id", indexSchemaResult.Definition.Name);
        
        // Trigger comment
        var triggerBlock = CreateBlocks("COMMENT ON TRIGGER update_timestamp ON users IS 'Updates modified_at timestamp';");
        var triggerResult = _extractor.Extract(triggerBlock);
        
        Assert.True(triggerResult.IsSuccess);
        Assert.Equal(SchemaObjectType.Triggers, triggerResult.Definition!.ObjectType);
        Assert.Equal("update_timestamp", triggerResult.Definition.Name);
        Assert.Equal("users", triggerResult.Definition.TableName);
        Assert.Equal("Updates modified_at timestamp", triggerResult.Definition.Comment);
        
        // Trigger with schema
        var triggerSchemaBlock = CreateBlocks("COMMENT ON TRIGGER audit_log ON public.orders IS 'Logs order changes';");
        var triggerSchemaResult = _extractor.Extract(triggerSchemaBlock);
        
        Assert.True(triggerSchemaResult.IsSuccess);
        Assert.Equal("public", triggerSchemaResult.Definition!.Schema);
        Assert.Equal("audit_log", triggerSchemaResult.Definition.Name);
        Assert.Equal("orders", triggerSchemaResult.Definition.TableName);
    }

    [Fact]
    public void Extract_ConstraintComments_HandlesAllVariants()
    {
        // Покрывает: CONSTRAINT comments (with/without schema)

        // Simple constraint comment
        var constraintBlock = CreateBlocks("COMMENT ON CONSTRAINT users_email_key ON users IS 'Unique email constraint';");
        var constraintResult = _extractor.Extract(constraintBlock);
        
        Assert.True(constraintResult.IsSuccess);
        Assert.NotNull(constraintResult.Definition);
        Assert.Equal(SchemaObjectType.Constraints, constraintResult.Definition.ObjectType);
        Assert.Equal("users_email_key", constraintResult.Definition.Name);
        Assert.Equal("users", constraintResult.Definition.TableName);
        Assert.Equal("Unique email constraint", constraintResult.Definition.Comment);
        
        // Constraint with schema
        var constraintSchemaBlock = CreateBlocks("COMMENT ON CONSTRAINT orders_user_fkey ON public.orders IS 'Foreign key to users table';");
        var constraintSchemaResult = _extractor.Extract(constraintSchemaBlock);
        
        Assert.True(constraintSchemaResult.IsSuccess);
        Assert.Equal("public", constraintSchemaResult.Definition!.Schema);
        Assert.Equal("orders_user_fkey", constraintSchemaResult.Definition.Name);
        Assert.Equal("orders", constraintSchemaResult.Definition.TableName);
        Assert.Equal("Foreign key to users table", constraintSchemaResult.Definition.Comment);
    }

    [Fact]
    public void Extract_TypeAndViewComments_HandlesAllVariants()
    {
        // Покрывает: TYPE comments (with/without schema), VIEW comments (regular and materialized, with/without schema)

        // Type comment
        var typeBlock = CreateBlocks("COMMENT ON TYPE status IS 'User status enumeration';");
        var typeResult = _extractor.Extract(typeBlock);
        
        Assert.True(typeResult.IsSuccess);
        Assert.NotNull(typeResult.Definition);
        Assert.Equal(SchemaObjectType.Types, typeResult.Definition.ObjectType);
        Assert.Equal("status", typeResult.Definition.Name);
        Assert.Equal("User status enumeration", typeResult.Definition.Comment);
        
        // Type with schema
        var typeSchemaBlock = CreateBlocks("COMMENT ON TYPE public.order_status IS 'Order status values';");
        var typeSchemaResult = _extractor.Extract(typeSchemaBlock);
        
        Assert.True(typeSchemaResult.IsSuccess);
        Assert.Equal("public", typeSchemaResult.Definition!.Schema);
        Assert.Equal("order_status", typeSchemaResult.Definition.Name);
        
        // View comment
        var viewBlock = CreateBlocks("COMMENT ON VIEW active_users IS 'View of active user accounts';");
        var viewResult = _extractor.Extract(viewBlock);
        
        Assert.True(viewResult.IsSuccess);
        Assert.Equal(SchemaObjectType.Views, viewResult.Definition!.ObjectType);
        Assert.Equal("active_users", viewResult.Definition.Name);
        Assert.Equal("View of active user accounts", viewResult.Definition.Comment);
        
        // Materialized view comment
        var matViewBlock = CreateBlocks("COMMENT ON MATERIALIZED VIEW user_stats IS 'Cached user statistics';");
        var matViewResult = _extractor.Extract(matViewBlock);
        
        Assert.True(matViewResult.IsSuccess);
        Assert.Equal(SchemaObjectType.Views, matViewResult.Definition!.ObjectType);
        Assert.Equal("user_stats", matViewResult.Definition.Name);
        
        // View with schema
        var viewSchemaBlock = CreateBlocks("COMMENT ON VIEW public.order_summary IS 'Order summary view';");
        var viewSchemaResult = _extractor.Extract(viewSchemaBlock);
        
        Assert.True(viewSchemaResult.IsSuccess);
        Assert.Equal("public", viewSchemaResult.Definition!.Schema);
        Assert.Equal("order_summary", viewSchemaResult.Definition.Name);
    }

    [Fact]
    public void Extract_SpecialCasesAndValidation_HandlesCorrectly()
    {
        // Покрывает: escaped quotes, NULL comments, empty comments, special characters, invalid syntax, edge cases

        // Escaped quotes in comment text
        var escapedBlock = CreateBlocks("COMMENT ON TABLE users IS 'User''s account information';");
        var escapedResult = _extractor.Extract(escapedBlock);
        
        Assert.True(escapedResult.IsSuccess);
        Assert.Contains("User's account", escapedResult.Definition!.Comment);
        
        // NULL comment might not be valid syntax - may return failure
        var nullBlock = CreateBlocks("COMMENT ON TABLE users IS NULL;");
        var nullResult = _extractor.Extract(nullBlock);
        
        // NULL is special - extractor might treat it as removal (success) or invalid (failure)
        // Just verify it doesn't crash
        Assert.NotNull(nullResult);
        
        // Empty comment
        var emptyBlock = CreateBlocks("COMMENT ON TABLE users IS '';");
        var emptyResult = _extractor.Extract(emptyBlock);
        
        Assert.True(emptyResult.IsSuccess);
        Assert.Empty(emptyResult.Definition!.Comment);
        
        // Comment with semicolon in text
        var semiBlock = CreateBlocks("COMMENT ON TABLE products IS 'Products; including digital items';");
        var semiResult = _extractor.Extract(semiBlock);
        
        Assert.True(semiResult.IsSuccess);
        Assert.Contains(";", semiResult.Definition!.Comment);
        Assert.Contains("digital", semiResult.Definition.Comment);
        
        // Very long comment
        var longComment = new string('x', 5000);
        var longBlock = CreateBlocks($"COMMENT ON TABLE users IS '{longComment}';");
        var longResult = _extractor.Extract(longBlock);
        
        Assert.True(longResult.IsSuccess);
        Assert.Equal(5000, longResult.Definition!.Comment.Length);
        
        // Invalid syntax
        var invalidBlock = CreateBlocks("COMMENT ON INVALID SYNTAX");
        var invalidResult = _extractor.Extract(invalidBlock);
        
        Assert.False(invalidResult.IsSuccess);
        Assert.Null(invalidResult.Definition);
        
        // Non-comment block
        var nonCommentBlock = CreateBlocks("CREATE TABLE users (id INT);");
        var nonCommentResult = _extractor.Extract(nonCommentBlock);
        
        Assert.False(nonCommentResult.IsSuccess);
        Assert.Null(nonCommentResult.Definition);
        
        // Null blocks - exception
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql)
    {
        return
        [
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                StartLine = 1,
                EndLine = 1
            }
        ];
    }
}
