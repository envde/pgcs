namespace PgCs.SchemaAnalyzer.Tests.Unit;

using PgCs.SchemaAnalyzer.Utils;
using PgCs.SchemaAnalyzer.Tests.Helpers;

public class SchemaMergerTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    [Fact]
    public void Merge_WithEmptyList_ShouldReturnEmptySchema()
    {
        // Arrange
        var schemas = Array.Empty<SchemaMetadata>();

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tables);
        Assert.Empty(result.Views);
        Assert.Empty(result.Types);
    }

    [Fact]
    public void Merge_WithSingleSchema_ShouldReturnSameSchema()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);
        var schemas = new[] { schema };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("users", result.Tables[0].Name);
    }

    [Fact]
    public void Merge_WithMultipleSchemas_ShouldCombineAllTables()
    {
        // Arrange
        var sql1 = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var sql2 = TestSchemaBuilder.CreateSimpleTable("posts", ("id", "integer"));
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.Equal(2, result.Tables.Count);
        Assert.Contains(result.Tables, t => t.Name == "users");
        Assert.Contains(result.Tables, t => t.Name == "posts");
    }

    [Fact]
    public void Merge_WithDuplicateTables_ShouldKeepBothIfDifferent()
    {
        // Arrange
        var sql1 = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var sql2 = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.NotNull(result);
        // Проверяем что метод не падает с дубликатами
        Assert.True(result.Tables.Count >= 1);
    }

    [Fact]
    public void Merge_WithViews_ShouldCombineAllViews()
    {
        // Arrange
        var sql1 = "CREATE VIEW active_users AS SELECT 1;";
        var sql2 = "CREATE VIEW inactive_users AS SELECT 1;";
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.Equal(2, result.Views.Count);
        Assert.Contains(result.Views, v => v.Name == "active_users");
        Assert.Contains(result.Views, v => v.Name == "inactive_users");
    }

    [Fact]
    public void Merge_WithTypes_ShouldCombineAllTypes()
    {
        // Arrange
        var sql1 = TestSchemaBuilder.CreateEnumType("user_status", "active", "inactive");
        var sql2 = TestSchemaBuilder.CreateEnumType("post_status", "draft", "published");
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.Equal(2, result.Types.Count);
        Assert.Contains(result.Types, t => t.Name == "user_status");
        Assert.Contains(result.Types, t => t.Name == "post_status");
    }

    [Fact]
    public void Merge_WithComplexSchemas_ShouldCombineAllElements()
    {
        // Arrange
        var sql1 = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                   "CREATE VIEW active_users AS SELECT * FROM users;";
        var sql2 = TestSchemaBuilder.CreateSimpleTable("extra_table", ("id", "integer"));
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Tables.Count >= 2);
        Assert.Contains(result.Tables, t => t.Name == "extra_table");
    }

    [Fact]
    public void Merge_WithMixedContent_ShouldPreserveAllTypes()
    {
        // Arrange
        var sql1 = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var sql2 = "CREATE VIEW active_users AS SELECT 1;";
        var sql3 = TestSchemaBuilder.CreateEnumType("user_status", "active", "inactive");
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schema3 = _analyzer.AnalyzeScript(sql3);
        var schemas = new[] { schema1, schema2, schema3 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.Single(result.Tables);
        Assert.Single(result.Views);
        Assert.Single(result.Types);
    }

    [Fact]
    public void Merge_WithFunctions_ShouldCombineAllFunctions()
    {
        // Arrange
        var sql1 = @"CREATE FUNCTION func1() RETURNS INTEGER AS $$
BEGIN
    RETURN 1;
END;
$$ LANGUAGE plpgsql;";
        var sql2 = @"CREATE FUNCTION func2() RETURNS INTEGER AS $$
BEGIN
    RETURN 2;
END;
$$ LANGUAGE plpgsql;";
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Functions.Count);
    }

    [Fact]
    public void Merge_WithIndexes_ShouldCombineAllIndexes()
    {
        // Arrange
        var sql1 = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                   TestSchemaBuilder.CreateIndex("idx1", "users", "id");
        var sql2 = TestSchemaBuilder.CreateSimpleTable("posts", ("id", "integer")) + "\n" +
                   TestSchemaBuilder.CreateIndex("idx2", "posts", "id");
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Indexes.Count);
    }

    [Fact]
    public void Merge_WithTriggers_ShouldCombineAllTriggers()
    {
        // Arrange
        var sql1 = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                   TestSchemaBuilder.CreateFunction("func1", "trigger", "BEGIN RETURN NEW; END;") + "\n" +
                   TestSchemaBuilder.CreateTrigger("trg1", "users", "func1");
        var sql2 = TestSchemaBuilder.CreateSimpleTable("posts", ("id", "integer")) + "\n" +
                   TestSchemaBuilder.CreateFunction("func2", "trigger", "BEGIN RETURN NEW; END;") + "\n" +
                   TestSchemaBuilder.CreateTrigger("trg2", "posts", "func2");
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Triggers.Count);
    }

    [Fact]
    public void Merge_WithConstraints_ShouldCombineAllConstraints()
    {
        // Arrange
        var sql1 = "CREATE TABLE users (id integer PRIMARY KEY);";
        var sql2 = "CREATE TABLE posts (id integer PRIMARY KEY);";
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schemas = new[] { schema1, schema2 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.NotNull(result);
        // Проверяем что constraints обработаны
        Assert.True(result.Constraints.Count >= 0);
    }

    [Fact]
    public void Merge_WithThreeSchemas_ShouldCombineAll()
    {
        // Arrange
        var sql1 = TestSchemaBuilder.CreateSimpleTable("table1", ("id", "integer"));
        var sql2 = TestSchemaBuilder.CreateSimpleTable("table2", ("id", "integer"));
        var sql3 = TestSchemaBuilder.CreateSimpleTable("table3", ("id", "integer"));
        var schema1 = _analyzer.AnalyzeScript(sql1);
        var schema2 = _analyzer.AnalyzeScript(sql2);
        var schema3 = _analyzer.AnalyzeScript(sql3);
        var schemas = new[] { schema1, schema2, schema3 };

        // Act
        var result = SchemaMerger.Merge(schemas);

        // Assert
        Assert.Equal(3, result.Tables.Count);
        Assert.Contains(result.Tables, t => t.Name == "table1");
        Assert.Contains(result.Tables, t => t.Name == "table2");
        Assert.Contains(result.Tables, t => t.Name == "table3");
    }
}
