namespace PgCs.SchemaAnalyzer.Tests.Unit;

using PgCs.SchemaAnalyzer.Extensions;
using PgCs.SchemaAnalyzer.Tests.Helpers;
using PgCs.Common.SchemaAnalyzer.Models.Tables;

public class SchemaAnalyzerExtensionsTests
{
    private readonly SchemaAnalyzer _analyzer = new();

    [Fact]
    public void FindTable_WithExistingTable_ShouldReturnTable()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"), ("name", "text"));
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.FindTable("users");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("users", result.Name);
    }

    [Fact]
    public void FindTable_WithNonExistingTable_ShouldReturnNull()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.FindTable("posts");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindTable_WithCaseInsensitiveMatch_ShouldReturnTable()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.FindTable("USERS");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("users", result.Name);
    }

    [Fact]
    public void FindView_WithExistingView_ShouldReturnView()
    {
        // Arrange
        var sql = "CREATE VIEW active_users AS SELECT * FROM users WHERE active = true;";
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.FindView("active_users");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("active_users", result.Name);
    }

    [Fact]
    public void FindView_WithNonExistingView_ShouldReturnNull()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.FindView("active_users");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindType_WithExistingType_ShouldReturnType()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateEnumType("user_status", "active", "inactive");
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.FindType("user_status");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user_status", result.Name);
    }

    [Fact]
    public void FindType_WithNonExistingType_ShouldReturnNull()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.FindType("user_status");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetTableIndexes_WithTableHavingIndexes_ShouldReturnIndexes()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"), ("email", "text")) +
                  "\n" + TestSchemaBuilder.CreateIndex("idx_users_email", "users", "email");
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.GetTableIndexes("users");

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, idx => idx.Name == "idx_users_email");
    }

    [Fact]
    public void GetTableIndexes_WithTableWithoutIndexes_ShouldReturnEmpty()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer"));
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.GetTableIndexes("users");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetTableTriggers_WithTableHavingTriggers_ShouldReturnTriggers()
    {
        // Arrange
        var sql = TestSchemaBuilder.CreateSimpleTable("users", ("id", "integer")) + "\n" +
                  TestSchemaBuilder.CreateFunction("update_timestamp", "trigger", "BEGIN RETURN NEW; END;") + "\n" +
                  TestSchemaBuilder.CreateTrigger("trg_update_users", "users", "update_timestamp");
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.GetTableTriggers("users");

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetTableConstraints_WithTableHavingConstraints_ShouldReturnConstraints()
    {
        // Arrange
        var sql = @"CREATE TABLE users (
            id integer PRIMARY KEY,
            email text UNIQUE NOT NULL
        );";
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.GetTableConstraints("users");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetTablesReferencingTable_WithReferences_ShouldReturnReferencingTables()
    {
        // Arrange
        var sql = @"
            CREATE TABLE users (id integer PRIMARY KEY);
            CREATE TABLE posts (id integer, user_id integer REFERENCES users(id));
        ";
        var schema = _analyzer.AnalyzeScript(sql);

        // Act
        var result = schema.GetTablesReferencingTable("users");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void HasColumn_WithExistingColumn_ShouldReturnTrue()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false },
                new ColumnDefinition { Name = "name", DataType = "text", IsNullable = false }
            ]
        };

        // Act
        var result = table.HasColumn("id");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasColumn_WithNonExistingColumn_ShouldReturnFalse()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }
            ]
        };

        // Act
        var result = table.HasColumn("email");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetColumn_WithExistingColumn_ShouldReturnColumn()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false },
                new ColumnDefinition { Name = "name", DataType = "text", IsNullable = false }
            ]
        };

        // Act
        var result = table.GetColumn("name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("name", result.Name);
        Assert.Equal("text", result.DataType);
    }

    [Fact]
    public void GetColumn_WithNonExistingColumn_ShouldReturnNull()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }
            ]
        };

        // Act
        var result = table.GetColumn("email");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPrimaryKeyColumns_WithPrimaryKey_ShouldReturnKeyColumns()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false, IsPrimaryKey = true },
                new ColumnDefinition { Name = "name", DataType = "text", IsNullable = false }
            ]
        };

        // Act
        var result = table.GetPrimaryKeyColumns();

        // Assert
        Assert.Single(result);
        Assert.Equal("id", result[0].Name);
    }

    [Fact]
    public void GetPrimaryKeyColumns_WithoutPrimaryKey_ShouldReturnEmpty()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false },
                new ColumnDefinition { Name = "name", DataType = "text", IsNullable = false }
            ]
        };

        // Act
        var result = table.GetPrimaryKeyColumns();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetRequiredColumns_ShouldReturnNonNullableColumns()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false },
                new ColumnDefinition { Name = "name", DataType = "text", IsNullable = false },
                new ColumnDefinition { Name = "email", DataType = "text", IsNullable = true }
            ]
        };

        // Act
        var result = table.GetRequiredColumns();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, col => Assert.False(col.IsNullable));
    }

    [Fact]
    public void GetRequiredColumns_WithAllNullable_ShouldReturnEmpty()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Columns =
            [
                new ColumnDefinition { Name = "email", DataType = "text", IsNullable = true },
                new ColumnDefinition { Name = "phone", DataType = "text", IsNullable = true }
            ]
        };

        // Act
        var result = table.GetRequiredColumns();

        // Assert
        Assert.Empty(result);
    }
}
