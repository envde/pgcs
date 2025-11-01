using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

public sealed class FunctionExtractorTests
{
    private readonly IExtractor<FunctionDefinition> _extractor = new FunctionExtractor();

    [Fact]
    public void CanExtract_ValidFunctionBlock_ReturnsTrue()
    {
        var sql = "CREATE FUNCTION test_function() RETURNS TEXT LANGUAGE sql AS $$ SELECT 'test'; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.CanExtract(blocks);

        Assert.True(result);
    }

    [Fact]
    public void CanExtract_ValidProcedureBlock_ReturnsTrue()
    {
        var sql = "CREATE PROCEDURE test_procedure() LANGUAGE sql AS $$ INSERT INTO test_table VALUES (1); $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.CanExtract(blocks);

        Assert.True(result);
    }

    [Fact]
    public void CanExtract_TableBlock_ReturnsFalse()
    {
        var sql = "CREATE TABLE test (id INT);";
        var blocks = CreateBlocks(sql);

        var result = _extractor.CanExtract(blocks);

        Assert.False(result);
    }

    [Fact]
    public void Extract_SimpleFunction_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION test_function() RETURNS TEXT LANGUAGE sql AS $$ SELECT 'Hello World'; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("test_function", result.Definition.Name);
        Assert.Equal("TEXT", result.Definition.ReturnType);
        Assert.Equal("sql", result.Definition.Language);
        Assert.Empty(result.Definition.Parameters);
        Assert.Contains("SELECT 'Hello World'", result.Definition.Body);
    }

    [Fact]
    public void Extract_FunctionWithSchema_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION public.test_function() RETURNS INTEGER LANGUAGE sql AS $$ SELECT 42; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("public", result.Definition.Schema);
        Assert.Equal("test_function", result.Definition.Name);
        Assert.Equal("INTEGER", result.Definition.ReturnType);
    }

    [Fact]
    public void Extract_SimpleProcedure_ExtractsCorrectly()
    {
        var sql = "CREATE PROCEDURE test_procedure() LANGUAGE sql AS $$ INSERT INTO test_table VALUES (1); $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("test_procedure", result.Definition.Name);
        Assert.Null(result.Definition.ReturnType);
        Assert.Equal("sql", result.Definition.Language);
    }

    [Fact]
    public void Extract_FunctionWithParameters_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION add_numbers(a INTEGER, b INTEGER) RETURNS INTEGER LANGUAGE sql AS $$ SELECT a + b; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("add_numbers", result.Definition.Name);
        Assert.Equal(2, result.Definition.Parameters.Count);
        Assert.Equal("a", result.Definition.Parameters[0].Name);
        Assert.Equal("INTEGER", result.Definition.Parameters[0].DataType);
        Assert.Equal("b", result.Definition.Parameters[1].Name);
        Assert.Equal("INTEGER", result.Definition.Parameters[1].DataType);
    }

    [Fact]
    public void Extract_FunctionWithOutParameter_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION get_user(IN user_id INTEGER, OUT user_name TEXT) LANGUAGE sql AS $$ SELECT name FROM users WHERE id = user_id; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(2, result.Definition.Parameters.Count);
        Assert.Equal(ParameterMode.In, result.Definition.Parameters[0].Mode);
        Assert.Equal(ParameterMode.Out, result.Definition.Parameters[1].Mode);
    }

    [Fact]
    public void Extract_FunctionWithArrayParameter_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION sum_array(arr INTEGER[]) RETURNS INTEGER LANGUAGE sql AS $$ SELECT SUM(x) FROM UNNEST(arr) AS x; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Single(result.Definition.Parameters);
        Assert.Equal("arr", result.Definition.Parameters[0].Name);
        Assert.Equal("INTEGER", result.Definition.Parameters[0].DataType);
        Assert.True(result.Definition.Parameters[0].IsArray);
    }

    [Fact]
    public void Extract_PlpgsqlFunction_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION calculate_total(order_id INTEGER) RETURNS NUMERIC LANGUAGE plpgsql AS $$ BEGIN RETURN (SELECT SUM(amount) FROM order_items WHERE order_id = order_id); END; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("plpgsql", result.Definition.Language);
        Assert.Contains("BEGIN", result.Definition.Body);
        Assert.Contains("END", result.Definition.Body);
    }

    [Fact]
    public void Extract_SqlFunction_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION get_user_count() RETURNS BIGINT LANGUAGE sql AS $$ SELECT COUNT(*) FROM users; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("sql", result.Definition.Language);
    }

    [Fact]
    public void Extract_ImmutableFunction_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION add_one(n INTEGER) RETURNS INTEGER IMMUTABLE LANGUAGE sql AS $$ SELECT n + 1; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(FunctionVolatility.Immutable, result.Definition.Volatility);
    }

    [Fact]
    public void Extract_StableFunction_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION get_current_timestamp() RETURNS TIMESTAMP STABLE LANGUAGE sql AS $$ SELECT CURRENT_TIMESTAMP; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(FunctionVolatility.Stable, result.Definition.Volatility);
    }

    [Fact]
    public void Extract_VolatileFunction_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION insert_log() RETURNS VOID VOLATILE LANGUAGE sql AS $$ INSERT INTO logs (message) VALUES ('test'); $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal(FunctionVolatility.Volatile, result.Definition.Volatility);
    }

    [Fact]
    public void Extract_FunctionWithOrReplace_ExtractsCorrectly()
    {
        var sql = "CREATE OR REPLACE FUNCTION test_function() RETURNS TEXT LANGUAGE sql AS $$ SELECT 'updated'; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("test_function", result.Definition.Name);
        Assert.Contains("'updated'", result.Definition.Body);
    }

    [Fact]
    public void Extract_FunctionWithHeaderComment_PreservesComment()
    {
        // Note: In real scenarios, the comment is passed via SqlBlock.HeaderComment, not in the SQL content
        var sql = "CREATE FUNCTION greet(name TEXT) RETURNS TEXT LANGUAGE sql AS $$ SELECT 'Hello, ' || name; $$;";
        var lines = sql.Split('\n');
        var blocks = new[]
        {
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                StartLine = 1,
                EndLine = lines.Length,
                HeaderComment = "This is a test function\nIt returns a greeting"
            }
        };

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.NotNull(result.Definition.SqlComment);
        Assert.Contains("test function", result.Definition.SqlComment);
    }

    [Fact]
    public void Extract_InvalidSyntax_ReturnsNotApplicable()
    {
        var sql = "CREATE FUNCTION incomplete";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Extract_FunctionWithoutReturnType_ReturnsWarning()
    {
        var sql = "CREATE FUNCTION test_function() LANGUAGE sql AS $$ SELECT 1; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        var issue = result.ValidationIssues.First(i => i.Code == "FUNCTION_NO_RETURN_TYPE");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, issue.Severity);
    }

    [Fact]
    public void Extract_FunctionWithoutLanguage_ReturnsWarning()
    {
        var sql = "CREATE FUNCTION test_function() RETURNS TEXT AS $$ SELECT 'test'; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        var issue = result.ValidationIssues.First(i => i.Code == "FUNCTION_NO_LANGUAGE");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, issue.Severity);
        Assert.Equal("sql", result.Definition?.Language);
    }

    [Fact]
    public void Extract_FunctionWithoutBody_ReturnsFailure()
    {
        var sql = "CREATE FUNCTION test_function() RETURNS TEXT LANGUAGE sql;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        var issue = result.ValidationIssues.First(i => i.Code == "FUNCTION_NO_BODY");
        Assert.Equal(ValidationIssue.ValidationSeverity.Error, issue.Severity);
    }

    [Fact]
    public void Extract_FunctionWithTooManyParameters_ReturnsWarning()
    {
        var sql = "CREATE FUNCTION test_function(p1 INT, p2 INT, p3 INT, p4 INT, p5 INT, p6 INT, p7 INT, p8 INT, p9 INT, p10 INT, p11 INT) RETURNS INT LANGUAGE sql AS $$ SELECT 1; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        var issue = result.ValidationIssues.First(i => i.Code == "FUNCTION_TOO_MANY_PARAMETERS");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, issue.Severity);
        Assert.Contains("11", issue.Message);
    }

    [Fact]
    public void Extract_FunctionWithTooLongBody_ReturnsWarning()
    {
        var bodyLines = string.Join("\n", Enumerable.Range(1, 250).Select(i => $"    SELECT {i};"));
        var sql = $"CREATE FUNCTION test_function() RETURNS INT LANGUAGE sql AS $$ {bodyLines} $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.ValidationIssues);
        var issue = result.ValidationIssues.First(i => i.Code == "FUNCTION_TOO_LONG");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, issue.Severity);
        Assert.Contains("250", issue.Message);
    }

    [Fact]
    public void Extract_NonFunctionBlock_ReturnsNotApplicable()
    {
        var sql = "CREATE TABLE test (id INT);";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Extract_EmptyBlock_ReturnsNotApplicable()
    {
        var blocks = CreateBlocks("");

        var result = _extractor.Extract(blocks);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Extract_ComplexPlpgsqlFunction_ExtractsCorrectly()
    {
        var sql = "CREATE OR REPLACE FUNCTION calculate_order_total(p_order_id INTEGER) RETURNS NUMERIC(10,2) LANGUAGE plpgsql STABLE AS $$ DECLARE v_total NUMERIC(10,2); BEGIN SELECT SUM(quantity * unit_price) INTO v_total FROM order_items WHERE order_id = p_order_id; RETURN COALESCE(v_total, 0); END; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Equal("calculate_order_total", result.Definition.Name);
        Assert.Equal("plpgsql", result.Definition.Language);
        Assert.Equal(FunctionVolatility.Stable, result.Definition.Volatility);
        Assert.Contains("DECLARE", result.Definition.Body);
        Assert.Contains("v_total", result.Definition.Body);
    }

    [Fact]
    public void Extract_FunctionReturningSetof_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION get_active_users() RETURNS SETOF users LANGUAGE sql AS $$ SELECT * FROM users WHERE is_active = true; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Contains("SETOF", result.Definition.ReturnType ?? "");
    }

    [Fact]
    public void Extract_FunctionReturningTable_ExtractsCorrectly()
    {
        var sql = "CREATE FUNCTION get_user_stats(p_user_id INTEGER) RETURNS TABLE(user_id INTEGER, order_count BIGINT, total_spent NUMERIC) LANGUAGE sql AS $$ SELECT u.id, COUNT(o.id), SUM(o.total) FROM users u LEFT JOIN orders o ON u.id = o.user_id WHERE u.id = p_user_id GROUP BY u.id; $$;";
        var blocks = CreateBlocks(sql);

        var result = _extractor.Extract(blocks);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Definition);
        Assert.Contains("TABLE", result.Definition.ReturnType ?? "");
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql)
    {
        // Extract header comment if present (lines starting with --)
        var lines = sql.Split('\n');
        var commentLines = new List<string>();
        int contentStartLine = 0;
        
        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("--"))
            {
                commentLines.Add(trimmed[2..].Trim());
            }
            else if (!string.IsNullOrWhiteSpace(trimmed))
            {
                contentStartLine = i;
                break;
            }
        }
        
        var headerComment = commentLines.Count > 0 ? string.Join("\n", commentLines) : null;
        
        return new[]
        {
            new SqlBlock
            {
                Content = sql,
                RawContent = sql,
                StartLine = 1,
                EndLine = lines.Length,
                HeaderComment = headerComment
            }
        };
    }
}
