using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Модульные тесты для FunctionExtractor
/// </summary>
public sealed class FunctionExtractorTests
{
    private readonly IFunctionExtractor _extractor = new FunctionExtractor();

    /// <summary>
    /// Создает SqlBlock для тестирования
    /// </summary>
    private static SqlBlock CreateBlock(string sql) => new()
    {
        Content = sql,
        RawContent = sql,
        StartLine = 1,
        EndLine = sql.Split('\n').Length
    };

    #region CanExtract Tests

    [Fact]
    public void CanExtract_WithValidFunction_ReturnsTrue()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION calculate_total(price DECIMAL, quantity INT)
            RETURNS DECIMAL
            LANGUAGE sql
            AS $$
                SELECT price * quantity;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithValidProcedure_ReturnsTrue()
    {
        // Arrange
        var sql = """
            CREATE PROCEDURE update_balance(user_id INT, amount DECIMAL)
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE users SET balance = balance + amount WHERE id = user_id;
            END;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithOrReplace_ReturnsTrue()
    {
        // Arrange
        var sql = """
            CREATE OR REPLACE FUNCTION get_user_name(user_id INT)
            RETURNS TEXT
            AS $$
                SELECT name FROM users WHERE id = user_id;
            $$ LANGUAGE sql;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WithTableBlock_ReturnsFalse()
    {
        // Arrange
        var sql = """
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL
            );
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.CanExtract(block);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExtract_WithNullBlock_ReturnsFalse()
    {
        // Act
        var result = _extractor.CanExtract(null!);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Extract Simple Function Tests

    [Fact]
    public void Extract_SimpleFunctionWithNoParameters_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION get_current_timestamp()
            RETURNS TIMESTAMP
            LANGUAGE sql
            AS $$
                SELECT NOW();
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("get_current_timestamp", result.Name);
        Assert.Null(result.Schema);
        Assert.False(result.IsProcedure);
        Assert.Empty(result.Parameters);
        Assert.Equal("TIMESTAMP", result.ReturnType);
        Assert.Equal("sql", result.Language);
        Assert.Contains("SELECT NOW();", result.Body);
    }

    [Fact]
    public void Extract_SimpleFunctionWithOneParameter_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION double_value(x INT)
            RETURNS INT
            LANGUAGE sql
            AS $$
                SELECT x * 2;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("double_value", result.Name);
        Assert.Single(result.Parameters);
        Assert.Equal("x", result.Parameters[0].Name);
        Assert.Equal("INT", result.Parameters[0].DataType);
        Assert.Equal(ParameterMode.In, result.Parameters[0].Mode);
        Assert.Equal("INT", result.ReturnType);
    }

    [Fact]
    public void Extract_FunctionWithMultipleParameters_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION calculate_price(base_price DECIMAL, discount DECIMAL, quantity INT)
            RETURNS DECIMAL
            LANGUAGE sql
            AS $$
                SELECT (base_price - discount) * quantity;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("calculate_price", result.Name);
        Assert.Equal(3, result.Parameters.Count);
        
        Assert.Equal("base_price", result.Parameters[0].Name);
        Assert.Equal("DECIMAL", result.Parameters[0].DataType);
        
        Assert.Equal("discount", result.Parameters[1].Name);
        Assert.Equal("DECIMAL", result.Parameters[1].DataType);
        
        Assert.Equal("quantity", result.Parameters[2].Name);
        Assert.Equal("INT", result.Parameters[2].DataType);
    }

    [Fact]
    public void Extract_FunctionWithSchemaQualifiedName_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION public.calculate_total(price DECIMAL)
            RETURNS DECIMAL
            LANGUAGE sql
            AS $$
                SELECT price * 1.1;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("calculate_total", result.Name);
        Assert.Equal("public", result.Schema);
    }

    #endregion

    #region Extract Procedure Tests

    [Fact]
    public void Extract_SimpleProcedure_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE PROCEDURE log_message(message TEXT)
            LANGUAGE plpgsql
            AS $$
            BEGIN
                INSERT INTO logs (message, created_at) VALUES (message, NOW());
            END;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("log_message", result.Name);
        Assert.True(result.IsProcedure);
        Assert.Null(result.ReturnType); // Процедуры не имеют RETURNS
        Assert.Equal("plpgsql", result.Language);
        Assert.Single(result.Parameters);
        Assert.Equal("message", result.Parameters[0].Name);
    }

    #endregion

    #region OR REPLACE Tests

    [Fact]
    public void Extract_FunctionWithOrReplace_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE OR REPLACE FUNCTION get_user_count()
            RETURNS BIGINT
            LANGUAGE sql
            AS $$
                SELECT COUNT(*) FROM users;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("get_user_count", result.Name);
        Assert.Equal("BIGINT", result.ReturnType);
    }

    [Fact]
    public void Extract_ProcedureWithOrReplace_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE OR REPLACE PROCEDURE update_status(order_id INT, new_status TEXT)
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE orders SET status = new_status WHERE id = order_id;
            END;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("update_status", result.Name);
        Assert.True(result.IsProcedure);
    }

    #endregion

    #region Parameter Mode Tests

    [Fact]
    public void Extract_FunctionWithInParameter_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION add_numbers(IN x INT, IN y INT)
            RETURNS INT
            LANGUAGE sql
            AS $$
                SELECT x + y;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Parameters.Count);
        Assert.Equal(ParameterMode.In, result.Parameters[0].Mode);
        Assert.Equal(ParameterMode.In, result.Parameters[1].Mode);
    }

    [Fact]
    public void Extract_FunctionWithOutParameter_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION get_user_info(user_id INT, OUT user_name TEXT, OUT user_email TEXT)
            LANGUAGE plpgsql
            AS $$
            BEGIN
                SELECT name, email INTO user_name, user_email FROM users WHERE id = user_id;
            END;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Parameters.Count);
        Assert.Equal(ParameterMode.In, result.Parameters[0].Mode);
        Assert.Equal(ParameterMode.Out, result.Parameters[1].Mode);
        Assert.Equal(ParameterMode.Out, result.Parameters[2].Mode);
    }

    [Fact]
    public void Extract_FunctionWithInOutParameter_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION increment(INOUT counter INT)
            LANGUAGE plpgsql
            AS $$
            BEGIN
                counter := counter + 1;
            END;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal(ParameterMode.InOut, result.Parameters[0].Mode);
    }

    [Fact]
    public void Extract_FunctionWithVariadicParameter_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION sum_all(VARIADIC numbers INT[])
            RETURNS INT
            LANGUAGE plpgsql
            AS $$
            DECLARE
                total INT := 0;
                num INT;
            BEGIN
                FOREACH num IN ARRAY numbers LOOP
                    total := total + num;
                END LOOP;
                RETURN total;
            END;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal(ParameterMode.Variadic, result.Parameters[0].Mode);
        Assert.Equal("numbers", result.Parameters[0].Name);
        Assert.Equal("INT[]", result.Parameters[0].DataType);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Extract_FunctionWithDefaultValue_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION greet(name TEXT DEFAULT 'World')
            RETURNS TEXT
            LANGUAGE sql
            AS $$
                SELECT 'Hello, ' || name || '!';
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("'World'", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public void Extract_FunctionWithNumericDefault_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION multiply(x INT, multiplier INT DEFAULT 2)
            RETURNS INT
            LANGUAGE sql
            AS $$
                SELECT x * multiplier;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Parameters.Count);
        Assert.Null(result.Parameters[0].DefaultValue);
        Assert.Equal("2", result.Parameters[1].DefaultValue);
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void Extract_FunctionWithSimpleReturnType_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION get_pi()
            RETURNS DOUBLE PRECISION
            LANGUAGE sql
            AS $$
                SELECT 3.14159265359;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DOUBLE PRECISION", result.ReturnType);
    }

    [Fact]
    public void Extract_FunctionWithComplexReturnType_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION get_user_ids()
            RETURNS SETOF INT
            LANGUAGE sql
            AS $$
                SELECT id FROM users;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SETOF INT", result.ReturnType);
    }

    [Fact]
    public void Extract_FunctionReturningTable_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION get_users()
            RETURNS TABLE(id INT, name TEXT)
            LANGUAGE sql
            AS $$
                SELECT id, name FROM users;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TABLE(id INT, name TEXT)", result.ReturnType);
    }

    #endregion

    #region Volatility Tests

    [Fact]
    public void Extract_FunctionWithVolatile_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION generate_random()
            RETURNS DOUBLE PRECISION
            LANGUAGE sql
            VOLATILE
            AS $$
                SELECT random();
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(FunctionVolatility.Volatile, result.Volatility);
    }

    [Fact]
    public void Extract_FunctionWithStable_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION get_current_date()
            RETURNS DATE
            LANGUAGE sql
            STABLE
            AS $$
                SELECT CURRENT_DATE;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(FunctionVolatility.Stable, result.Volatility);
    }

    [Fact]
    public void Extract_FunctionWithImmutable_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION calculate_circle_area(radius DOUBLE PRECISION)
            RETURNS DOUBLE PRECISION
            LANGUAGE sql
            IMMUTABLE
            AS $$
                SELECT 3.14159 * radius * radius;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(FunctionVolatility.Immutable, result.Volatility);
    }

    #endregion

    #region Language Tests

    [Fact]
    public void Extract_FunctionWithSqlLanguage_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION get_count()
            RETURNS BIGINT
            LANGUAGE sql
            AS $$
                SELECT COUNT(*) FROM users;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sql", result.Language);
    }

    [Fact]
    public void Extract_FunctionWithPlpgsqlLanguage_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION complex_calculation()
            RETURNS INT
            LANGUAGE plpgsql
            AS $$
            DECLARE
                result INT;
            BEGIN
                result := 42;
                RETURN result;
            END;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("plpgsql", result.Language);
    }

    #endregion

    #region Body Extraction Tests

    [Fact]
    public void Extract_FunctionWithDollarQuotedBody_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION test_function()
            RETURNS TEXT
            LANGUAGE sql
            AS $$
                SELECT 'Hello World';
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("SELECT 'Hello World';", result.Body);
    }

    [Fact]
    public void Extract_FunctionWithTaggedDollarQuote_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION test_function()
            RETURNS TEXT
            LANGUAGE sql
            AS $function$
                SELECT 'Test';
            $function$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("SELECT 'Test';", result.Body);
    }

    [Fact]
    public void Extract_FunctionWithSingleQuotedBody_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE FUNCTION test_function()
            RETURNS TEXT
            LANGUAGE sql
            AS 'SELECT NOW()';
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("SELECT NOW()", result.Body);
    }

    #endregion

    #region Real-World Examples

    [Fact]
    public void Extract_ComplexFunctionWithAllFeatures_ExtractsCorrectly()
    {
        // Arrange
        var sql = """
            CREATE OR REPLACE FUNCTION public.calculate_order_total(
                IN order_id INT,
                IN discount_percent DECIMAL DEFAULT 0,
                OUT total_amount DECIMAL,
                OUT item_count INT
            )
            RETURNS RECORD
            LANGUAGE plpgsql
            STABLE
            AS $$
            BEGIN
                SELECT 
                    SUM(price * quantity) * (1 - discount_percent / 100),
                    COUNT(*)
                INTO total_amount, item_count
                FROM order_items
                WHERE order_id = order_id;
            END;
            $$;
            """;

        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("calculate_order_total", result.Name);
        Assert.Equal("public", result.Schema);
        Assert.False(result.IsProcedure);
        Assert.Equal(4, result.Parameters.Count);
        
        // IN параметры
        Assert.Equal("order_id", result.Parameters[0].Name);
        Assert.Equal(ParameterMode.In, result.Parameters[0].Mode);
        
        Assert.Equal("discount_percent", result.Parameters[1].Name);
        Assert.Equal(ParameterMode.In, result.Parameters[1].Mode);
        Assert.Equal("0", result.Parameters[1].DefaultValue);
        
        // OUT параметры
        Assert.Equal("total_amount", result.Parameters[2].Name);
        Assert.Equal(ParameterMode.Out, result.Parameters[2].Mode);
        
        Assert.Equal("item_count", result.Parameters[3].Name);
        Assert.Equal(ParameterMode.Out, result.Parameters[3].Mode);
        
        Assert.Equal("RECORD", result.ReturnType);
        Assert.Equal("plpgsql", result.Language);
        Assert.Equal(FunctionVolatility.Stable, result.Volatility);
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Extract_WithEmptyBlock_ReturnsNull()
    {
        // Arrange
        var block = CreateBlock("");

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_WithNullBlock_ReturnsNull()
    {
        // Act
        var result = _extractor.Extract(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_WithInvalidSql_ReturnsNull()
    {
        // Arrange
        var sql = "This is not valid SQL";
        var block = CreateBlock(sql);

        // Act
        var result = _extractor.Extract(block);

        // Assert
        Assert.Null(result);
    }

    #endregion
}
