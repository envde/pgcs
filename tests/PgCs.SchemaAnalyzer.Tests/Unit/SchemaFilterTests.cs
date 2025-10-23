namespace PgCs.SchemaAnalyzer.Tests.Unit;

using PgCs.SchemaAnalyzer.Tests.Helpers;
using PgCs.Common.SchemaAnalyzer.Models.Types;

public class SchemaFilterTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    [Fact]
    public void From_ShouldCreateFilter()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var filter = SchemaFilter.From(schema);

        // Assert
        Assert.NotNull(filter);
    }

    [Fact]
    public void Build_WithoutFilters_ShouldReturnOriginalSchema()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  "CREATE VIEW active_users AS SELECT * FROM users;";
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.Build();

        // Assert
        Assert.Equal(schema.Tables.Count, result.Tables.Count);
        Assert.Equal(schema.Views.Count, result.Views.Count);
    }

    [Fact]
    public void ExcludeSchemas_ShouldRemoveTablesFromSchema()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.ExcludeSchemas("pg_catalog", "information_schema").Build();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void IncludeOnlySchemas_ShouldKeepOnlySpecifiedSchemas()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.IncludeOnlySchemas("public").Build();

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Tables, table => Assert.Equal("public", table.Schema ?? "public"));
    }

    [Fact]
    public void ExcludeTables_WithPattern_ShouldRemoveMatchingTables()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  TestSchemaBuilder.CreateSimpleTable("temp_data", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var originalCount = schema.Tables.Count;
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.ExcludeTables("temp_*").Build();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Tables.Count <= originalCount);
    }

    [Fact]
    public void ExcludeTables_WithMultiplePatterns_ShouldRemoveAllMatching()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.ExcludeTables("temp_*", "test_*").Build();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void IncludeOnlyTables_WithPattern_ShouldKeepOnlyMatching()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.IncludeOnlyTables("users").Build();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tables);
    }

    [Fact]
    public void IncludeOnlyTables_WithMultiplePatterns_ShouldKeepAllMatching()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  TestSchemaBuilder.CreateSimpleTable("posts", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.IncludeOnlyTables("users", "posts").Build();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tables.Count);
    }

    [Fact]
    public void ExcludeViews_WithPattern_ShouldRemoveMatchingViews()
    {
        // Arrange
        var sql = "CREATE VIEW active_users AS SELECT 1;";
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.ExcludeViews("active_*").Build();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Views);
    }

    [Fact]
    public void IncludeOnlyViews_WithPattern_ShouldKeepOnlyMatchingViews()
    {
        // Arrange
        var sql = "CREATE VIEW active_users AS SELECT 1;";
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.IncludeOnlyViews("active_*").Build();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Views);
    }

    [Fact]
    public void IncludeOnlyTypes_WithEnumKind_ShouldKeepOnlyEnums()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateEnumType("user_status", "active", "inactive");
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.IncludeOnlyTypes(TypeKind.Enum).Build();

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Types, type => Assert.Equal(TypeKind.Enum, type.Kind));
    }

    [Fact]
    public void RemoveSystemObjects_ShouldRemoveSystemSchemas()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.RemoveSystemObjects().Build();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void RemoveTables_ShouldRemoveAllTables()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.RemoveTables().Build();

        // Assert
        Assert.Empty(result.Tables);
    }

    [Fact]
    public void RemoveViews_ShouldRemoveAllViews()
    {
        // Arrange
        var sql = "CREATE VIEW active_users AS SELECT 1;";
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.RemoveViews().Build();

        // Assert
        Assert.Empty(result.Views);
    }

    [Fact]
    public void RemoveTypes_ShouldRemoveAllTypes()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateEnumType("user_status", "active");
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.RemoveTypes().Build();

        // Assert
        Assert.Empty(result.Types);
    }

    [Fact]
    public void RemoveFunctions_ShouldRemoveAllFunctions()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateFunction("test_func", "void", "BEGIN RETURN; END;");
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.RemoveFunctions().Build();

        // Assert
        Assert.Empty(result.Functions);
    }

    [Fact]
    public void RemoveIndexes_ShouldRemoveAllIndexes()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  TestSchemaBuilder.CreateIndex("idx_users_id", "users", "id");
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.RemoveIndexes().Build();

        // Assert
        Assert.Empty(result.Indexes);
    }

    [Fact]
    public void RemoveTriggers_ShouldRemoveAllTriggers()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  TestSchemaBuilder.CreateFunction("update_ts", "trigger", "BEGIN RETURN NEW; END;") + "\n" +
                  TestSchemaBuilder.CreateTrigger("trg_update", "users", "update_ts");
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.RemoveTriggers().Build();

        // Assert
        Assert.Empty(result.Triggers);
    }

    [Fact]
    public void RemoveConstraints_ShouldRemoveAllConstraints()
    {
        // Arrange
        var sql = @"CREATE TABLE users (id integer PRIMARY KEY);";
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.RemoveConstraints().Build();

        // Assert
        Assert.Empty(result.Constraints);
    }

    [Fact]
    public void OnlyTablesAndViews_ShouldKeepOnlyTablesAndViews()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  "CREATE VIEW active_users AS SELECT 1;" + "\n" +
                  TestSchemaBuilder.CreateFunction("test_func", "void", "BEGIN RETURN; END;");
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.OnlyTablesAndViews().Build();

        // Assert
        Assert.NotEmpty(result.Tables);
        Assert.NotEmpty(result.Views);
        Assert.Empty(result.Functions);
    }

    [Fact]
    public void OnlyTables_ShouldKeepOnlyTables()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  "CREATE VIEW active_users AS SELECT 1;";
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter.OnlyTables().Build();

        // Assert
        Assert.NotEmpty(result.Tables);
        Assert.Empty(result.Views);
    }

    [Fact]
    public void ChainedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  "CREATE VIEW active_users AS SELECT 1;";
        var schema = _analyzer.AnalyzeScript(sql);
        var filter = SchemaFilter.From(schema);

        // Act
        var result = filter
            .RemoveSystemObjects()
            .OnlyTables()
            .IncludeOnlySchemas("public")
            .Build();

        // Assert
        Assert.NotEmpty(result.Tables);
        Assert.Empty(result.Views);
    }
}
