using PgCs.Common.SchemaAnalyzer.Models.Types;

namespace PgCs.SchemaAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для SchemaAnalyzerBuilder - Fluent API для анализа схемы
/// </summary>
public sealed class SchemaAnalyzerBuilderTests
{
    #region Builder Creation Tests

    [Fact]
    public void Create_ReturnsNewBuilderInstance()
    {
        // Act
        var builder = SchemaAnalyzerBuilder.Create();

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void Create_ReturnsDifferentInstances()
    {
        // Act
        var builder1 = SchemaAnalyzerBuilder.Create();
        var builder2 = SchemaAnalyzerBuilder.Create();

        // Assert
        Assert.NotSame(builder1, builder2);
    }

    #endregion

    #region FromScript Tests

    [Fact]
    public async Task FromScript_WithValidSql_AnalyzesCorrectly()
    {
        // Arrange
        const string sql = "CREATE TABLE test (id INT);";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .AnalyzeAsync();

        // Assert
        Assert.NotNull(metadata);
        Assert.Single(metadata.Tables);
    }

    [Fact]
    public void FromScript_WithNullSql_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SchemaAnalyzerBuilder.Create().FromScript(null!));
    }

    [Fact]
    public void FromScript_WithEmptySql_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            SchemaAnalyzerBuilder.Create().FromScript(""));
    }

    [Fact]
    public async Task FromScript_WithMultipleScripts_CombinesResults()
    {
        // Arrange
        const string sql1 = "CREATE TABLE table1 (id INT);";
        const string sql2 = "CREATE TABLE table2 (id INT);";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql1)
            .FromScript(sql2)
            .AnalyzeAsync();

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
        Assert.Contains(metadata.Tables, t => t.Name == "table1");
        Assert.Contains(metadata.Tables, t => t.Name == "table2");
    }

    #endregion

    #region Extract Filters Tests

    [Fact]
    public async Task WithTables_WhenTrue_ExtractsTables()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE VIEW active_users AS SELECT * FROM users;
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithTables(true)
            .WithViews(false)
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Tables);
        Assert.Empty(metadata.Views);
    }

    [Fact]
    public async Task WithoutTables_ExcludesTables()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE TYPE status AS ENUM ('active');
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithoutTables()
            .AnalyzeAsync();

        // Assert
        Assert.Empty(metadata.Tables);
        Assert.Single(metadata.Types);
    }

    [Fact]
    public async Task WithViews_WhenTrue_ExtractsViews()
    {
        // Arrange
        const string sql = "CREATE VIEW test_view AS SELECT 1;";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithViews(true)
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Views);
    }

    [Fact]
    public async Task WithoutViews_ExcludesViews()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE VIEW active_users AS SELECT * FROM users;
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithoutViews()
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Tables);
        Assert.Empty(metadata.Views);
    }

    [Fact]
    public async Task WithTypes_WhenTrue_ExtractsTypes()
    {
        // Arrange
        const string sql = "CREATE TYPE status AS ENUM ('active', 'inactive');";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithTypes(true)
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Types);
    }

    [Fact]
    public async Task WithoutTypes_ExcludesTypes()
    {
        // Arrange
        const string sql = @"
            CREATE TYPE status AS ENUM ('active');
            CREATE TABLE users (id INT);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithoutTypes()
            .AnalyzeAsync();

        // Assert
        Assert.Empty(metadata.Types);
        Assert.Single(metadata.Tables);
    }

    [Fact]
    public async Task WithFunctions_WhenTrue_ExtractsFunctions()
    {
        // Arrange
        const string sql = @"
            CREATE FUNCTION test_func() RETURNS INT AS $$
            BEGIN RETURN 1; END;
            $$ LANGUAGE plpgsql;
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithFunctions(true)
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Functions);
    }

    [Fact]
    public async Task WithoutFunctions_ExcludesFunctions()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE FUNCTION test_func() RETURNS INT AS $$ BEGIN RETURN 1; END; $$ LANGUAGE plpgsql;
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithoutFunctions()
            .AnalyzeAsync();

        // Assert
        Assert.Empty(metadata.Functions);
        Assert.Single(metadata.Tables);
    }

    [Fact]
    public async Task WithIndexes_WhenTrue_ExtractsIndexes()
    {
        // Arrange
        const string sql = "CREATE INDEX idx_test ON users(email);";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithIndexes(true)
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Indexes);
    }

    [Fact]
    public async Task WithoutIndexes_ExcludesIndexes()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE INDEX idx_users_id ON users(id);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithoutIndexes()
            .AnalyzeAsync();

        // Assert
        Assert.Empty(metadata.Indexes);
    }

    [Fact]
    public async Task WithTriggers_WhenTrue_ExtractsTriggers()
    {
        // Arrange
        const string sql = @"
            CREATE TRIGGER test_trigger
                BEFORE INSERT ON users
                FOR EACH ROW EXECUTE FUNCTION trigger_func();
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithTriggers(true)
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Triggers);
    }

    [Fact]
    public async Task WithoutTriggers_ExcludesTriggers()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE TRIGGER test_trigger BEFORE INSERT ON users 
                FOR EACH ROW EXECUTE FUNCTION func();
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithoutTriggers()
            .AnalyzeAsync();

        // Assert
        Assert.Empty(metadata.Triggers);
    }

    [Fact]
    public async Task WithConstraints_WhenTrue_ExtractsConstraints()
    {
        // Arrange
        const string sql = @"
            ALTER TABLE orders
            ADD CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithConstraints(true)
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Constraints);
    }

    [Fact]
    public async Task WithoutConstraints_ExcludesConstraints()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            ALTER TABLE users ADD CONSTRAINT unique_id UNIQUE (id);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithoutConstraints()
            .AnalyzeAsync();

        // Assert
        Assert.Empty(metadata.Constraints);
    }

    #endregion

    #region Quick Filter Tests

    [Fact]
    public async Task OnlyTables_ExtractsOnlyTables()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE VIEW active_users AS SELECT * FROM users;
            CREATE TYPE status AS ENUM ('active');
            CREATE INDEX idx_users ON users(id);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .OnlyTables()
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Tables);
        Assert.Empty(metadata.Views);
        Assert.Empty(metadata.Types);
        Assert.Empty(metadata.Indexes);
    }

    [Fact]
    public async Task OnlyTablesAndViews_ExtractsTablesAndViewsOnly()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE VIEW active_users AS SELECT * FROM users;
            CREATE TYPE status AS ENUM ('active');
            CREATE FUNCTION test() RETURNS INT AS $$ BEGIN RETURN 1; END; $$ LANGUAGE plpgsql;
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .OnlyTablesAndViews()
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Tables);
        Assert.Single(metadata.Views);
        Assert.Empty(metadata.Types);
        Assert.Empty(metadata.Functions);
    }

    [Fact]
    public async Task OnlyTypes_ExtractsOnlyTypes()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE TYPE status AS ENUM ('active');
            CREATE TYPE priority AS ENUM ('low', 'high');
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .OnlyTypes()
            .AnalyzeAsync();

        // Assert
        Assert.Empty(metadata.Tables);
        Assert.Equal(2, metadata.Types.Count);
    }

    #endregion

    #region Schema Filtering Tests

    [Fact]
    public async Task ExcludeSchemas_FiltersSpecifiedSchemas()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE public.users (id INT);
            CREATE TABLE private.secrets (id INT);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .ExcludeSchemas("private")
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Tables);
        Assert.Equal("public", metadata.Tables[0].Schema);
    }

    [Fact]
    public async Task ExcludeSystemSchemas_FiltersSystemSchemas()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE public.users (id INT);
            CREATE TABLE pg_catalog.system_table (id INT);
            CREATE TABLE information_schema.columns (id INT);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .ExcludeSystemSchemas()
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Tables);
        Assert.Equal("public", metadata.Tables[0].Schema);
    }

    [Fact]
    public async Task IncludeOnlySchemas_IncludesOnlySpecifiedSchemas()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE public.users (id INT);
            CREATE TABLE app.data (id INT);
            CREATE TABLE temp.cache (id INT);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .IncludeOnlySchemas("public", "app")
            .AnalyzeAsync();

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
        Assert.DoesNotContain(metadata.Tables, t => t.Schema == "temp");
    }

    #endregion

    #region Table Pattern Filtering Tests

    [Fact]
    public async Task ExcludeTables_FiltersMatchingTables()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE TABLE temp_users (id INT);
            CREATE TABLE temp_data (id INT);
            CREATE TABLE products (id INT);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .ExcludeTables("^temp_.*")
            .AnalyzeAsync();

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
        Assert.Contains(metadata.Tables, t => t.Name == "users");
        Assert.Contains(metadata.Tables, t => t.Name == "products");
        Assert.DoesNotContain(metadata.Tables, t => t.Name.StartsWith("temp_"));
    }

    [Fact]
    public async Task IncludeOnlyTables_IncludesOnlyMatchingTables()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE user_data (id INT);
            CREATE TABLE user_settings (id INT);
            CREATE TABLE products (id INT);
            CREATE TABLE orders (id INT);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .IncludeOnlyTables("^user_.*")
            .AnalyzeAsync();

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
        Assert.All(metadata.Tables, t => Assert.StartsWith("user_", t.Name));
    }

    [Fact]
    public async Task ExcludeTables_WithMultiplePatterns_FiltersAll()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE users (id INT);
            CREATE TABLE temp_data (id INT);
            CREATE TABLE test_table (id INT);
            CREATE TABLE products (id INT);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .ExcludeTables("^temp_.*", "^test_.*")
            .AnalyzeAsync();

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
        Assert.Contains(metadata.Tables, t => t.Name == "users");
        Assert.Contains(metadata.Tables, t => t.Name == "products");
    }

    #endregion

    #region Duplicate Handling Tests

    [Fact]
    public async Task RemoveDuplicates_WhenTrue_RemovesDuplicateTables()
    {
        // Arrange
        const string sql1 = "CREATE TABLE users (id INT);";
        const string sql2 = "CREATE TABLE users (id INT);";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql1)
            .FromScript(sql2)
            .RemoveDuplicates(true)
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Tables);
    }

    [Fact]
    public async Task KeepDuplicates_KeepsDuplicateTables()
    {
        // Arrange
        const string sql1 = "CREATE TABLE users (id INT);";
        const string sql2 = "CREATE TABLE users (id INT);";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql1)
            .FromScript(sql2)
            .KeepDuplicates()
            .AnalyzeAsync();

        // Assert
        Assert.Equal(2, metadata.Tables.Count);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task AnalyzeAsync_WithoutSource_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await SchemaAnalyzerBuilder.Create().AnalyzeAsync());
    }

    [Fact]
    public void FromFile_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SchemaAnalyzerBuilder.Create().FromFile(null!));
    }

    [Fact]
    public void FromFile_WithEmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            SchemaAnalyzerBuilder.Create().FromFile(""));
    }

    [Fact]
    public void FromFiles_WithNullArray_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SchemaAnalyzerBuilder.Create().FromFiles(null!));
    }

    [Fact]
    public void FromDirectory_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SchemaAnalyzerBuilder.Create().FromDirectory(null!));
    }

    [Fact]
    public void FromDirectories_WithNullArray_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SchemaAnalyzerBuilder.Create().FromDirectories(null!));
    }

    [Fact]
    public void ExcludeSchemas_WithNullArray_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SchemaAnalyzerBuilder.Create().ExcludeSchemas(null!));
    }

    [Fact]
    public void IncludeOnlySchemas_WithNullArray_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SchemaAnalyzerBuilder.Create().IncludeOnlySchemas(null!));
    }

    #endregion

    #region Fluent API Chain Tests

    [Fact]
    public async Task FluentChain_WithMultipleFilters_AppliesAllFilters()
    {
        // Arrange
        const string sql = @"
            CREATE TABLE public.users (id INT);
            CREATE TABLE public.temp_data (id INT);
            CREATE TABLE private.secrets (id INT);
            CREATE VIEW public.user_view AS SELECT * FROM users;
            CREATE TYPE status AS ENUM ('active');
            CREATE INDEX idx_users ON public.users(id);
        ";

        // Act
        var metadata = await SchemaAnalyzerBuilder.Create()
            .FromScript(sql)
            .WithTables()
            .WithoutViews()
            .WithoutTypes()
            .WithoutIndexes()
            .ExcludeSchemas("private")
            .ExcludeTables("^temp_.*")
            .AnalyzeAsync();

        // Assert
        Assert.Single(metadata.Tables);
        Assert.Equal("users", metadata.Tables[0].Name);
        Assert.Empty(metadata.Views);
        Assert.Empty(metadata.Types);
        Assert.Empty(metadata.Indexes);
    }

    [Fact]
    public void FluentChain_ReturnsBuilderForChaining()
    {
        // Act
        var builder = SchemaAnalyzerBuilder.Create()
            .FromScript("CREATE TABLE test (id INT);")
            .WithTables()
            .WithoutViews()
            .ExcludeSystemSchemas();

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<SchemaAnalyzerBuilder>(builder);
    }

    #endregion
}
