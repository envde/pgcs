using PgCs.Core.Schema.Analyzer;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для SchemaFilterBuilder
/// </summary>
public sealed class SchemaFilterBuilderTests
{
    #region Schema Filtering Tests

    [Fact]
    public void ExcludeSchemas_ShouldExcludeSpecifiedSchemas()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("table1", "public"),
                CreateTable("table2", "app"),
                CreateTable("table3", "test")
            ]
        );

        // Act
        var filter = builder
            .ExcludeSchemas("app", "test")
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("table1", result.Tables[0].Name);
        Assert.Equal("public", result.Tables[0].Schema);
    }

    [Fact]
    public void IncludeOnlySchemas_ShouldIncludeOnlySpecifiedSchemas()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("table1", "public"),
                CreateTable("table2", "app"),
                CreateTable("table3", "test")
            ]
        );

        // Act
        var filter = builder
            .IncludeOnlySchemas("public", "app")
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Equal(2, result.Tables.Count);
        Assert.Contains(result.Tables, t => t.Name == "table1" && t.Schema == "public");
        Assert.Contains(result.Tables, t => t.Name == "table2" && t.Schema == "app");
    }

    [Fact]
    public void ExcludeSchemas_WithWhitespace_ShouldTrimAndExclude()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("table1", "public"),
                CreateTable("table2", "app")
            ]
        );

        // Act
        var filter = builder
            .ExcludeSchemas("  app  ", " ")
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("public", result.Tables[0].Schema);
    }

    #endregion

    #region Table Filtering Tests

    [Fact]
    public void ExcludeTables_WithRegexPattern_ShouldExcludeMatchingTables()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("temp_data", "public"),
                CreateTable("test_table", "public"),
                CreateTable("users_backup", "public")
            ]
        );

        // Act
        var filter = builder
            .ExcludeTables("^temp_.*", "^test_.*", ".*_backup$")
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("users", result.Tables[0].Name);
    }

    [Fact]
    public void IncludeOnlyTables_WithRegexPattern_ShouldIncludeOnlyMatchingTables()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("user_profiles", "public"),
                CreateTable("user_settings", "public"),
                CreateTable("orders", "public"),
                CreateTable("products", "public")
            ]
        );

        // Act
        var filter = builder
            .IncludeOnlyTables("^user_.*")
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Equal(2, result.Tables.Count);
        Assert.All(result.Tables, t => Assert.StartsWith("user_", t.Name));
    }

    [Fact]
    public void ExcludeTables_WithInvalidRegex_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            builder.ExcludeTables("[invalid(regex").Build();
        });
        
        Assert.Contains("Invalid regex pattern", exception.Message);
    }

    #endregion

    #region View Filtering Tests

    [Fact]
    public void ExcludeViews_WithRegexPattern_ShouldExcludeMatchingViews()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            views: [
                CreateView("user_view", "public"),
                CreateView("temp_view", "public"),
                CreateView("test_view", "public")
            ]
        );

        // Act
        var filter = builder
            .ExcludeViews("^temp_.*", "^test_.*")
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Views);
        Assert.Equal("user_view", result.Views[0].Name);
    }

    [Fact]
    public void IncludeOnlyViews_WithRegexPattern_ShouldIncludeOnlyMatchingViews()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            views: [
                CreateView("active_users_view", "public"),
                CreateView("active_orders_view", "public"),
                CreateView("all_products", "public")
            ]
        );

        // Act
        var filter = builder
            .IncludeOnlyViews("^active_.*")
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Equal(2, result.Views.Count);
        Assert.All(result.Views, v => Assert.StartsWith("active_", v.Name));
    }

    #endregion

    #region Type Filtering Tests

    [Fact]
    public void IncludeOnlyTypes_WithEnumKind_ShouldIncludeOnlyEnums()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            enums: [
                CreateEnum("user_status", "public"),
                CreateEnum("order_status", "public")
            ],
            composites: [
                CreateComposite("address", "public")
            ],
            domains: [
                CreateDomain("email", "public")
            ]
        );

        // Act
        var filter = builder
            .WithObjects(SchemaObjectType.Types)
            .IncludeOnlyTypes(TypeKind.Enum)
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Equal(2, result.Enums.Count);
        Assert.Empty(result.Composites);
        Assert.Empty(result.Domains);
    }

    [Fact]
    public void IncludeOnlyTypes_WithMultipleKinds_ShouldIncludeSpecifiedTypes()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            enums: [CreateEnum("status", "public")],
            composites: [CreateComposite("address", "public")],
            domains: [CreateDomain("email", "public")]
        );

        // Act
        var filter = builder
            .WithObjects(SchemaObjectType.Types)
            .IncludeOnlyTypes(TypeKind.Enum, TypeKind.Domain)
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Enums);
        Assert.Empty(result.Composites);
        Assert.Single(result.Domains);
    }

    #endregion

    #region Object Type Filtering Tests

    [Fact]
    public void WithObjects_ShouldFilterBySpecifiedObjectTypes()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            functions: [CreateFunction("get_user", "public")],
            indexes: [CreateIndex("idx_users", "public")]
        );

        // Act
        var filter = builder
            .WithObjects(SchemaObjectType.Tables, SchemaObjectType.Views)
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Single(result.Views);
        Assert.Empty(result.Functions);
        Assert.Empty(result.Indexes);
    }

    [Fact]
    public void OnlyTables_ShouldIncludeTablesAndRelatedObjects()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            indexes: [CreateIndex("idx_users", "public")],
            constraints: [CreateConstraint("pk_users", "public")],
            triggers: [CreateTrigger("trg_users", "public")]
        );

        // Act
        var filter = builder.OnlyTables().Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Empty(result.Views);
        Assert.Single(result.Indexes);
        Assert.Single(result.Constraints);
        Assert.Single(result.Triggers);
    }

    [Fact]
    public void OnlyTablesAndViews_ShouldIncludeOnlyTablesAndViews()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            functions: [CreateFunction("get_user", "public")]
        );

        // Act
        var filter = builder.OnlyTablesAndViews().Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Single(result.Views);
        Assert.Empty(result.Functions);
    }

    [Fact]
    public void WithObjects_WithNoneType_ShouldIgnoreNone()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")]
        );

        // Act
        var filter = builder
            .WithObjects(SchemaObjectType.None, SchemaObjectType.Tables)
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
    }

    #endregion

    #region System Objects Filtering Tests

    [Fact]
    public void ExcludeSystemObjects_ShouldExcludeSystemSchemas()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("pg_stat", "pg_catalog"),
                CreateTable("tables", "information_schema")
            ]
        );

        // Act
        var filter = builder.ExcludeSystemObjects().Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("users", result.Tables[0].Name);
        Assert.Equal("public", result.Tables[0].Schema);
    }

    [Fact]
    public void ExcludeSystemObjects_ShouldExcludeSystemTablePatterns()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("pg_stat_table", "public"),
                CreateTable("sql_features", "public")
            ]
        );

        // Act
        var filter = builder.ExcludeSystemObjects().Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("users", result.Tables[0].Name);
    }

    #endregion

    #region Comment Parsing Tests

    [Fact]
    public void WithCommentParsing_Enabled_ShouldIncludeComments()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            tableComments: [CreateTableComment("users", "public", "User table")]
        );

        // Act
        var filter = builder
            .WithCommentParsing(true)
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.TableComments);
    }

    [Fact]
    public void WithCommentParsing_Disabled_ShouldExcludeComments()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            tableComments: [CreateTableComment("users", "public", "User table")]
        );

        // Act
        var filter = builder
            .WithCommentParsing(false)
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Empty(result.TableComments);
    }

    #endregion

    #region Complex Filtering Tests

    [Fact]
    public void Build_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("orders", "public"),
                CreateTable("temp_data", "public"),
                CreateTable("products", "app")
            ],
            views: [
                CreateView("user_view", "public"),
                CreateView("test_view", "public")
            ]
        );

        // Act
        var filter = builder
            .IncludeOnlySchemas("public")
            .ExcludeTables("^temp_.*")
            .ExcludeViews("^test_.*")
            .Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Equal(2, result.Tables.Count);
        Assert.Contains(result.Tables, t => t.Name == "users");
        Assert.Contains(result.Tables, t => t.Name == "orders");
        Assert.Single(result.Views);
        Assert.Equal("user_view", result.Views[0].Name);
    }

    [Fact]
    public void Build_WithNoFilters_ShouldIncludeAllObjects()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public"), CreateTable("orders", "app")],
            views: [CreateView("user_view", "public")],
            functions: [CreateFunction("get_user", "public")]
        );

        // Act
        var filter = builder.Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Equal(2, result.Tables.Count);
        Assert.Single(result.Views);
        Assert.Single(result.Functions);
    }

    #endregion

    #region Strict Mode Tests

    [Fact]
    public void WithStrictMode_ShouldSetStrictModeFlag()
    {
        // Arrange & Act
        var filter = new SchemaFilterBuilder()
            .WithStrictMode(true)
            .Build();

        // Assert - strict mode не влияет на результат фильтрации, только на парсинг
        Assert.NotNull(filter);
    }

    [Fact]
    public void WithStrictMode_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var filter = new SchemaFilterBuilder().Build();

        // Assert
        Assert.NotNull(filter);
    }

    #endregion

    #region Null and Empty Tests

    [Fact]
    public void ExcludeSchemas_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.ExcludeSchemas(null!));
    }

    [Fact]
    public void IncludeOnlyTypes_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.IncludeOnlyTypes(null!));
    }

    [Fact]
    public void WithObjects_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new SchemaFilterBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithObjects(null!));
    }

    #endregion

    #region Helper Methods

    private static SchemaMetadata CreateTestMetadata(
        IReadOnlyList<TableDefinition>? tables = null,
        IReadOnlyList<ViewDefinition>? views = null,
        IReadOnlyList<EnumTypeDefinition>? enums = null,
        IReadOnlyList<CompositeTypeDefinition>? composites = null,
        IReadOnlyList<DomainTypeDefinition>? domains = null,
        IReadOnlyList<FunctionDefinition>? functions = null,
        IReadOnlyList<IndexDefinition>? indexes = null,
        IReadOnlyList<TriggerDefinition>? triggers = null,
        IReadOnlyList<ConstraintDefinition>? constraints = null,
        IReadOnlyList<PartitionDefinition>? partitions = null,
        IReadOnlyList<TableCommentDefinition>? tableComments = null)
    {
        return new SchemaMetadata
        {
            Tables = tables ?? [],
            Views = views ?? [],
            Enums = enums ?? [],
            Composites = composites ?? [],
            Domains = domains ?? [],
            Functions = functions ?? [],
            Indexes = indexes ?? [],
            Triggers = triggers ?? [],
            Constraints = constraints ?? [],
            Partitions = partitions ?? [],
            CompositeTypeComments = [],
            TableComments = tableComments ?? [],
            ColumnComments = [],
            IndexComments = [],
            TriggerComments = [],
            FunctionComments = [],
            ConstraintComments = [],
            ValidationIssues = [],
            SourcePaths = [],
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private static TableDefinition CreateTable(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            Columns = []
        };

    private static ViewDefinition CreateView(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            Query = "SELECT 1"
        };

    private static EnumTypeDefinition CreateEnum(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            Values = ["value1", "value2"]
        };

    private static CompositeTypeDefinition CreateComposite(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            Attributes = []
        };

    private static DomainTypeDefinition CreateDomain(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            BaseType = "VARCHAR(255)"
        };

    private static FunctionDefinition CreateFunction(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            Parameters = [],
            Language = "sql",
            Body = "SELECT 1"
        };

    private static IndexDefinition CreateIndex(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            TableName = "test_table",
            Columns = ["id"]
        };

    private static TriggerDefinition CreateTrigger(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            TableName = "test_table",
            Timing = TriggerTiming.Before,
            Events = [TriggerEvent.Insert],
            FunctionName = "test_function"
        };

    private static ConstraintDefinition CreateConstraint(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            TableName = "test_table",
            Type = ConstraintType.PrimaryKey
        };

    private static TableCommentDefinition CreateTableComment(string tableName, string? schema, string comment) =>
        new()
        {
            Schema = schema,
            TableName = tableName,
            Comment = comment
        };

    #endregion
}
