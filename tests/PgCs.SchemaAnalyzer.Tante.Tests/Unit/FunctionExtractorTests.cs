using PgCs.Core.Extraction;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.Core.Validation;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Консолидированные тесты для FunctionExtractor
/// Покрывает функции, процедуры, параметры (IN/OUT/INOUT/VARIADIC), волатильность, языки, и специальные форматы комментариев
/// </summary>
public sealed class FunctionExtractorTests
{
    private readonly IExtractor<FunctionDefinition> _extractor = new FunctionExtractor();

    [Fact]
    public void Extract_BasicFunctionsAndProcedures_HandlesAllVariants()
    {
        // Покрывает: simple function, schema, procedure, OR REPLACE

        // Simple function without parameters
        var simpleBlock = CreateBlocks("CREATE FUNCTION test_function() RETURNS TEXT LANGUAGE sql AS $$ SELECT 'Hello World'; $$;");
        var simpleResult = _extractor.Extract(simpleBlock);
        
        Assert.True(simpleResult.IsSuccess);
        Assert.NotNull(simpleResult.Definition);
        Assert.Equal("test_function", simpleResult.Definition.Name);
        Assert.Equal("TEXT", simpleResult.Definition.ReturnType);
        Assert.Equal("sql", simpleResult.Definition.Language);
        Assert.Empty(simpleResult.Definition.Parameters);
        Assert.Contains("SELECT 'Hello World'", simpleResult.Definition.Body);
        
        // Function with schema
        var schemaBlock = CreateBlocks("CREATE FUNCTION public.test_function() RETURNS INTEGER LANGUAGE sql AS $$ SELECT 42; $$;");
        var schemaResult = _extractor.Extract(schemaBlock);
        
        Assert.True(schemaResult.IsSuccess);
        Assert.Equal("public", schemaResult.Definition!.Schema);
        Assert.Equal("test_function", schemaResult.Definition.Name);
        Assert.Equal("INTEGER", schemaResult.Definition.ReturnType);
        
        // Procedure (no return type)
        var procedureBlock = CreateBlocks("CREATE PROCEDURE test_procedure() LANGUAGE sql AS $$ INSERT INTO test_table VALUES (1); $$;");
        var procedureResult = _extractor.Extract(procedureBlock);
        
        Assert.True(procedureResult.IsSuccess);
        Assert.Equal("test_procedure", procedureResult.Definition!.Name);
        Assert.Null(procedureResult.Definition.ReturnType);
        Assert.Equal("sql", procedureResult.Definition.Language);
        
        // OR REPLACE
        var replaceBlock = CreateBlocks("CREATE OR REPLACE FUNCTION test_function() RETURNS TEXT LANGUAGE sql AS $$ SELECT 'updated'; $$;");
        var replaceResult = _extractor.Extract(replaceBlock);
        
        Assert.True(replaceResult.IsSuccess);
        Assert.Equal("test_function", replaceResult.Definition!.Name);
        Assert.Contains("'updated'", replaceResult.Definition.Body);
        
        // CanExtract validation
        Assert.True(_extractor.CanExtract(simpleBlock));
        Assert.True(_extractor.CanExtract(procedureBlock));
        Assert.False(_extractor.CanExtract(CreateBlocks("CREATE TABLE test (id INT);")));
    }

    [Fact]
    public void Extract_Parameters_HandlesAllModes()
    {
        // Покрывает: IN, OUT, INOUT, VARIADIC, array parameters, default values

        // Multiple IN parameters
        var inBlock = CreateBlocks("CREATE FUNCTION add_numbers(a INTEGER, b INTEGER) RETURNS INTEGER LANGUAGE sql AS $$ SELECT a + b; $$;");
        var inResult = _extractor.Extract(inBlock);
        
        Assert.True(inResult.IsSuccess);
        Assert.Equal(2, inResult.Definition!.Parameters.Count);
        Assert.Equal("a", inResult.Definition.Parameters[0].Name);
        Assert.Equal("INTEGER", inResult.Definition.Parameters[0].DataType);
        Assert.Equal(ParameterMode.In, inResult.Definition.Parameters[0].Mode);
        Assert.Equal("b", inResult.Definition.Parameters[1].Name);
        Assert.Equal("INTEGER", inResult.Definition.Parameters[1].DataType);
        
        // IN and OUT parameters
        var outBlock = CreateBlocks("CREATE FUNCTION get_user(IN user_id INTEGER, OUT user_name TEXT) LANGUAGE sql AS $$ SELECT name FROM users WHERE id = user_id; $$;");
        var outResult = _extractor.Extract(outBlock);
        
        Assert.True(outResult.IsSuccess);
        Assert.Equal(2, outResult.Definition!.Parameters.Count);
        Assert.Equal(ParameterMode.In, outResult.Definition.Parameters[0].Mode);
        Assert.Equal("user_id", outResult.Definition.Parameters[0].Name);
        Assert.Equal(ParameterMode.Out, outResult.Definition.Parameters[1].Mode);
        Assert.Equal("user_name", outResult.Definition.Parameters[1].Name);
        
        // Array parameter
        var arrayBlock = CreateBlocks("CREATE FUNCTION sum_array(arr INTEGER[]) RETURNS INTEGER LANGUAGE sql AS $$ SELECT SUM(x) FROM UNNEST(arr) AS x; $$;");
        var arrayResult = _extractor.Extract(arrayBlock);
        
        Assert.True(arrayResult.IsSuccess);
        Assert.Single(arrayResult.Definition!.Parameters);
        Assert.Equal("arr", arrayResult.Definition.Parameters[0].Name);
        Assert.Equal("INTEGER", arrayResult.Definition.Parameters[0].DataType);
        Assert.True(arrayResult.Definition.Parameters[0].IsArray);
    }

    [Fact]
    public void Extract_LanguagesAndVolatility_HandlesAllOptions()
    {
        // Покрывает: sql, plpgsql, IMMUTABLE, STABLE, VOLATILE

        // SQL language
        var sqlBlock = CreateBlocks("CREATE FUNCTION get_user_count() RETURNS BIGINT LANGUAGE sql AS $$ SELECT COUNT(*) FROM users; $$;");
        var sqlResult = _extractor.Extract(sqlBlock);
        
        Assert.True(sqlResult.IsSuccess);
        Assert.Equal("sql", sqlResult.Definition!.Language);
        
        // PLPGSQL language with DECLARE/BEGIN/END
        var plpgsqlBlock = CreateBlocks(
            "CREATE FUNCTION calculate_total(order_id INTEGER) RETURNS NUMERIC LANGUAGE plpgsql AS $$ " +
            "BEGIN RETURN (SELECT SUM(amount) FROM order_items WHERE order_id = order_id); END; $$;");
        var plpgsqlResult = _extractor.Extract(plpgsqlBlock);
        
        Assert.True(plpgsqlResult.IsSuccess);
        Assert.Equal("plpgsql", plpgsqlResult.Definition!.Language);
        Assert.Contains("BEGIN", plpgsqlResult.Definition.Body);
        Assert.Contains("END", plpgsqlResult.Definition.Body);
        
        // IMMUTABLE
        var immutableBlock = CreateBlocks("CREATE FUNCTION add_one(n INTEGER) RETURNS INTEGER IMMUTABLE LANGUAGE sql AS $$ SELECT n + 1; $$;");
        var immutableResult = _extractor.Extract(immutableBlock);
        
        Assert.True(immutableResult.IsSuccess);
        Assert.Equal(FunctionVolatility.Immutable, immutableResult.Definition!.Volatility);
        
        // STABLE
        var stableBlock = CreateBlocks("CREATE FUNCTION get_current_timestamp() RETURNS TIMESTAMP STABLE LANGUAGE sql AS $$ SELECT CURRENT_TIMESTAMP; $$;");
        var stableResult = _extractor.Extract(stableBlock);
        
        Assert.True(stableResult.IsSuccess);
        Assert.Equal(FunctionVolatility.Stable, stableResult.Definition!.Volatility);
        
        // VOLATILE (default)
        var volatileBlock = CreateBlocks("CREATE FUNCTION insert_log() RETURNS VOID VOLATILE LANGUAGE sql AS $$ INSERT INTO logs (message) VALUES ('test'); $$;");
        var volatileResult = _extractor.Extract(volatileBlock);
        
        Assert.True(volatileResult.IsSuccess);
        Assert.Equal(FunctionVolatility.Volatile, volatileResult.Definition!.Volatility);
        
        // Complex PLPGSQL with DECLARE and variables
        var complexBlock = CreateBlocks(
            "CREATE OR REPLACE FUNCTION calculate_order_total(p_order_id INTEGER) RETURNS NUMERIC(10,2) LANGUAGE plpgsql STABLE AS $$ " +
            "DECLARE v_total NUMERIC(10,2); BEGIN SELECT SUM(quantity * unit_price) INTO v_total FROM order_items WHERE order_id = p_order_id; " +
            "RETURN COALESCE(v_total, 0); END; $$;");
        var complexResult = _extractor.Extract(complexBlock);
        
        Assert.True(complexResult.IsSuccess);
        Assert.Equal("calculate_order_total", complexResult.Definition!.Name);
        Assert.Equal("plpgsql", complexResult.Definition.Language);
        Assert.Equal(FunctionVolatility.Stable, complexResult.Definition.Volatility);
        Assert.Contains("DECLARE", complexResult.Definition.Body);
        Assert.Contains("v_total", complexResult.Definition.Body);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: comment: формат и comment() формат для функций
        
        // Format 1: comment: Text; rename: NewName;
        var blocks1 = CreateBlocks(
            "CREATE FUNCTION get_user_email(user_id INTEGER) RETURNS TEXT LANGUAGE sql AS $$ SELECT email FROM users WHERE id = user_id; $$;",
            "comment: Получает email пользователя; rename: GetUserEmail;");
        var result1 = _extractor.Extract(blocks1);
        
        Assert.True(result1.IsSuccess);
        Assert.NotNull(result1.Definition);
        Assert.NotNull(result1.Definition.SqlComment);
        Assert.Contains("comment:", result1.Definition.SqlComment);
        Assert.Contains("Получает email пользователя", result1.Definition.SqlComment);
        Assert.Contains("rename:", result1.Definition.SqlComment);
        
        // Format 2: comment(Text); rename(NewName);
        var blocks2 = CreateBlocks(
            "CREATE PROCEDURE update_user_status(p_user_id INTEGER, p_status TEXT) LANGUAGE sql AS $$ UPDATE users SET status = p_status WHERE id = p_user_id; $$;",
            "comment(Обновляет статус пользователя); rename(UpdateUserStatus);");
        var result2 = _extractor.Extract(blocks2);
        
        Assert.True(result2.IsSuccess);
        Assert.NotNull(result2.Definition);
        Assert.NotNull(result2.Definition.SqlComment);
        Assert.Contains("comment(", result2.Definition.SqlComment);
        Assert.Contains("Обновляет статус пользователя", result2.Definition.SqlComment);
        Assert.Contains("rename(", result2.Definition.SqlComment);
        
        // Function with header comment (regular format)
        var blocks3 = CreateBlocks(
            "CREATE FUNCTION greet(name TEXT) RETURNS TEXT LANGUAGE sql AS $$ SELECT 'Hello, ' || name; $$;",
            "This is a test function\nIt returns a greeting");
        var result3 = _extractor.Extract(blocks3);
        
        Assert.True(result3.IsSuccess);
        Assert.NotNull(result3.Definition!.SqlComment);
        Assert.Contains("test function", result3.Definition.SqlComment);
        Assert.Contains("greeting", result3.Definition.SqlComment);
        
        // Return types: SETOF and TABLE
        var setofBlock = CreateBlocks("CREATE FUNCTION get_active_users() RETURNS SETOF users LANGUAGE sql AS $$ SELECT * FROM users WHERE is_active = true; $$;");
        var setofResult = _extractor.Extract(setofBlock);
        
        Assert.True(setofResult.IsSuccess);
        Assert.Contains("SETOF", setofResult.Definition!.ReturnType ?? "");
        
        var tableBlock = CreateBlocks(
            "CREATE FUNCTION get_user_stats(p_user_id INTEGER) RETURNS TABLE(user_id INTEGER, order_count BIGINT, total_spent NUMERIC) LANGUAGE sql AS $$ " +
            "SELECT u.id, COUNT(o.id), SUM(o.total) FROM users u LEFT JOIN orders o ON u.id = o.user_id WHERE u.id = p_user_id GROUP BY u.id; $$;");
        var tableResult = _extractor.Extract(tableBlock);
        
        Assert.True(tableResult.IsSuccess);
        Assert.Contains("TABLE", tableResult.Definition!.ReturnType ?? "");
    }

    [Fact]
    public void Extract_EdgeCasesAndValidation_HandlesCorrectly()
    {
        // Покрывает: validation issues, edge cases, errors

        // Function without return type - warning
        var noReturnBlock = CreateBlocks("CREATE FUNCTION test_function() LANGUAGE sql AS $$ SELECT 1; $$;");
        var noReturnResult = _extractor.Extract(noReturnBlock);
        
        Assert.True(noReturnResult.IsSuccess);
        Assert.NotEmpty(noReturnResult.ValidationIssues);
        var noReturnIssue = noReturnResult.ValidationIssues.First(i => i.Code == "FUNCTION_NO_RETURN_TYPE");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, noReturnIssue.Severity);
        
        // Function without language - warning (defaults to sql)
        var noLangBlock = CreateBlocks("CREATE FUNCTION test_function() RETURNS TEXT AS $$ SELECT 'test'; $$;");
        var noLangResult = _extractor.Extract(noLangBlock);
        
        Assert.True(noLangResult.IsSuccess);
        Assert.NotEmpty(noLangResult.ValidationIssues);
        var noLangIssue = noLangResult.ValidationIssues.First(i => i.Code == "FUNCTION_NO_LANGUAGE");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, noLangIssue.Severity);
        Assert.Equal("sql", noLangResult.Definition!.Language);
        
        // Function without body - error
        var noBodyBlock = CreateBlocks("CREATE FUNCTION test_function() RETURNS TEXT LANGUAGE sql;");
        var noBodyResult = _extractor.Extract(noBodyBlock);
        
        Assert.False(noBodyResult.IsSuccess);
        Assert.NotEmpty(noBodyResult.ValidationIssues);
        var noBodyIssue = noBodyResult.ValidationIssues.First(i => i.Code == "FUNCTION_NO_BODY");
        Assert.Equal(ValidationIssue.ValidationSeverity.Error, noBodyIssue.Severity);
        
        // Too many parameters - warning
        var tooManyParamsBlock = CreateBlocks(
            "CREATE FUNCTION test_function(p1 INT, p2 INT, p3 INT, p4 INT, p5 INT, p6 INT, p7 INT, p8 INT, p9 INT, p10 INT, p11 INT) " +
            "RETURNS INT LANGUAGE sql AS $$ SELECT 1; $$;");
        var tooManyParamsResult = _extractor.Extract(tooManyParamsBlock);
        
        Assert.True(tooManyParamsResult.IsSuccess);
        Assert.NotEmpty(tooManyParamsResult.ValidationIssues);
        var tooManyParamsIssue = tooManyParamsResult.ValidationIssues.First(i => i.Code == "FUNCTION_TOO_MANY_PARAMETERS");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, tooManyParamsIssue.Severity);
        Assert.Contains("11", tooManyParamsIssue.Message);
        
        // Too long body - warning
        var bodyLines = string.Join("\n", Enumerable.Range(1, 250).Select(i => $"    SELECT {i};"));
        var tooLongBlock = CreateBlocks($"CREATE FUNCTION test_function() RETURNS INT LANGUAGE sql AS $$ {bodyLines} $$;");
        var tooLongResult = _extractor.Extract(tooLongBlock);
        
        Assert.True(tooLongResult.IsSuccess);
        Assert.NotEmpty(tooLongResult.ValidationIssues);
        var tooLongIssue = tooLongResult.ValidationIssues.First(i => i.Code == "FUNCTION_TOO_LONG");
        Assert.Equal(ValidationIssue.ValidationSeverity.Warning, tooLongIssue.Severity);
        Assert.Contains("250", tooLongIssue.Message);
        
        // Invalid syntax
        var invalidBlock = CreateBlocks("CREATE FUNCTION incomplete");
        var invalidResult = _extractor.Extract(invalidBlock);
        Assert.False(invalidResult.IsSuccess);
        
        // Non-function block
        var nonFunctionBlock = CreateBlocks("CREATE TABLE test (id INT);");
        var nonFunctionResult = _extractor.Extract(nonFunctionBlock);
        Assert.False(nonFunctionResult.IsSuccess);
        
        // Empty block
        var emptyBlock = CreateBlocks("");
        var emptyResult = _extractor.Extract(emptyBlock);
        Assert.False(emptyResult.IsSuccess);
    }

    [Fact]
    public void Extract_MultilineCreateFunction_ShouldWork()
    {
        // Test для CREATE на отдельной строке от OR REPLACE FUNCTION
        var sql = @"CREATE
    OR REPLACE FUNCTION update_category_search_vector()
    RETURNS TRIGGER AS
$$
BEGIN
    NEW.search_vector := setweight(to_tsvector('english', COALESCE(NEW.name, '')), 'A');
    RETURN NEW;
END;
$$
    LANGUAGE plpgsql;";
        
        var blocks = CreateBlocks(sql);
        
        // Act
        var canExtract = _extractor.CanExtract(blocks);
        var result = _extractor.Extract(blocks);
        
        // Assert
        Assert.True(canExtract, "CanExtract should return true for multiline CREATE");
        Assert.True(result.IsSuccess, $"Extract should succeed. Issues: {string.Join(", ", result.ValidationIssues.Select(i => i.Message))}");
        Assert.NotNull(result.Definition);
        Assert.Equal("update_category_search_vector", result.Definition.Name);
        Assert.Equal("TRIGGER", result.Definition.ReturnType);
        Assert.Equal("plpgsql", result.Definition.Language);
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? headerComment = null)
    {
        var lines = sql.Split('\n');
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
