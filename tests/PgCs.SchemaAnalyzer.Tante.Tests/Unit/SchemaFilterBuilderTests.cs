using PgCs.Core.Schema.Analyzer;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Consolidated tests for SchemaFilterBuilder - testing schema filtering, object type selection, and pattern matching
/// </summary>
public sealed class SchemaFilterBuilderTests
{
    /// <summary>
    /// Test 1: Schema inclusion/exclusion with whitespace handling
    /// </summary>
    [Fact]
    public void Filter_SchemaInclusionAndExclusion_WorksCorrectly()
    {
        // Test 1.1: Exclude specific schemas
        var metadata1 = CreateTestMetadata(
            tables:
            [
                CreateTable("table1", "public"),
                CreateTable("table2", "app"),
                CreateTable("table3", "test")
            ]
        );
        
        var builder1 = new SchemaFilterBuilder()
            .ExcludeSchemas("app", "test")
            .Build();
        var result1 = builder1.ApplyFilter(metadata1);
        
        Assert.Single(result1.Tables);
        Assert.Equal("table1", result1.Tables[0].Name);
        Assert.Equal("public", result1.Tables[0].Schema);
        
        // Test 1.2: Include only specified schemas
        var metadata2 = CreateTestMetadata(
            tables:
            [
                CreateTable("table1", "public"),
                CreateTable("table2", "app"),
                CreateTable("table3", "test")
            ]
        );
        
        var builder2 = new SchemaFilterBuilder()
            .IncludeOnlySchemas("public", "app")
            .Build();
        var result2 = builder2.ApplyFilter(metadata2);
        
        Assert.Equal(2, result2.Tables.Count);
        Assert.Contains(result2.Tables, t => t.Name == "table1" && t.Schema == "public");
        Assert.Contains(result2.Tables, t => t.Name == "table2" && t.Schema == "app");
        
        // Test 1.3: Exclude schemas with whitespace (should trim)
        var metadata3 = CreateTestMetadata(
            tables:
            [
                CreateTable("table1", "public"),
                CreateTable("table2", "app")
            ]
        );
        
        var builder3 = new SchemaFilterBuilder()
            .ExcludeSchemas("  app  ", " ")
            .Build();
        var result3 = builder3.ApplyFilter(metadata3);
        
        Assert.Single(result3.Tables);
        Assert.Equal("public", result3.Tables[0].Schema);
        
        // Test 1.4: Exclude system objects (pg_catalog, information_schema, pg_* tables)
        var metadata4 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("pg_stat", "pg_catalog"),
                CreateTable("tables", "information_schema"),
                CreateTable("pg_stat_table", "public"),
                CreateTable("sql_features", "public")
            ]
        );
        
        var builder4 = new SchemaFilterBuilder()
            .ExcludeSystemObjects()
            .Build();
        var result4 = builder4.ApplyFilter(metadata4);
        
        Assert.Single(result4.Tables);
        Assert.Equal("users", result4.Tables[0].Name);
        Assert.Equal("public", result4.Tables[0].Schema);
    }

    /// <summary>
    /// Test 2: Table and view regex pattern filtering with validation
    /// </summary>
    [Fact]
    public void Filter_TableAndViewRegexPatterns_WorksCorrectly()
    {
        // Test 2.1: Exclude tables matching regex patterns
        var metadata1 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("temp_data", "public"),
                CreateTable("test_table", "public"),
                CreateTable("users_backup", "public")
            ]
        );
        
        var builder1 = new SchemaFilterBuilder()
            .ExcludeTables("^temp_.*", "^test_.*", ".*_backup$")
            .Build();
        var result1 = builder1.ApplyFilter(metadata1);
        
        Assert.Single(result1.Tables);
        Assert.Equal("users", result1.Tables[0].Name);
        
        // Test 2.2: Include only tables matching regex patterns
        var metadata2 = CreateTestMetadata(
            tables:
            [
                CreateTable("user_profiles", "public"),
                CreateTable("user_settings", "public"),
                CreateTable("orders", "public"),
                CreateTable("products", "public")
            ]
        );
        
        var builder2 = new SchemaFilterBuilder()
            .IncludeOnlyTables("^user_.*")
            .Build();
        var result2 = builder2.ApplyFilter(metadata2);
        
        Assert.Equal(2, result2.Tables.Count);
        Assert.All(result2.Tables, t => Assert.StartsWith("user_", t.Name));
        
        // Test 2.3: Invalid regex should throw ArgumentException
        var builder3 = new SchemaFilterBuilder();
        
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            builder3.ExcludeTables("[invalid(regex").Build();
        });
        
        Assert.Contains("Invalid regex pattern", exception.Message);
        
        // Test 2.4: Exclude views matching regex patterns
        var metadata4 = CreateTestMetadata(
            views:
            [
                CreateView("user_view", "public"),
                CreateView("temp_view", "public"),
                CreateView("test_view", "public")
            ]
        );
        
        var builder4 = new SchemaFilterBuilder()
            .ExcludeViews("^temp_.*", "^test_.*")
            .Build();
        var result4 = builder4.ApplyFilter(metadata4);
        
        Assert.Single(result4.Views);
        Assert.Equal("user_view", result4.Views[0].Name);
        
        // Test 2.5: Include only views matching regex patterns
        var metadata5 = CreateTestMetadata(
            views:
            [
                CreateView("active_users_view", "public"),
                CreateView("active_orders_view", "public"),
                CreateView("all_products", "public")
            ]
        );
        
        var builder5 = new SchemaFilterBuilder()
            .IncludeOnlyViews("^active_.*")
            .Build();
        var result5 = builder5.ApplyFilter(metadata5);
        
        Assert.Equal(2, result5.Views.Count);
        Assert.All(result5.Views, v => Assert.StartsWith("active_", v.Name));
    }

    /// <summary>
    /// Test 3: Type kind filtering (Enum, Composite, Domain)
    /// </summary>
    [Fact]
    public void Filter_TypeKinds_SelectsCorrectTypes()
    {
        // Test 3.1: Include only Enum types
        var metadata1 = CreateTestMetadata(
            enums:
            [
                CreateEnum("user_status", "public"),
                CreateEnum("order_status", "public")
            ],
            composites:
            [
                CreateComposite("address", "public")
            ],
            domains:
            [
                CreateDomain("email", "public")
            ]
        );
        
        var builder1 = new SchemaFilterBuilder()
            .WithObjects(SchemaObjectType.Types)
            .IncludeOnlyTypes(TypeKind.Enum)
            .Build();
        var result1 = builder1.ApplyFilter(metadata1);
        
        Assert.Equal(2, result1.Enums.Count);
        Assert.Empty(result1.Composites);
        Assert.Empty(result1.Domains);
        
        // Test 3.2: Include multiple type kinds (Enum and Domain, but not Composite)
        var metadata2 = CreateTestMetadata(
            enums: [CreateEnum("status", "public")],
            composites: [CreateComposite("address", "public")],
            domains: [CreateDomain("email", "public")]
        );
        
        var builder2 = new SchemaFilterBuilder()
            .WithObjects(SchemaObjectType.Types)
            .IncludeOnlyTypes(TypeKind.Enum, TypeKind.Domain)
            .Build();
        var result2 = builder2.ApplyFilter(metadata2);
        
        Assert.Single(result2.Enums);
        Assert.Empty(result2.Composites);
        Assert.Single(result2.Domains);
    }

    /// <summary>
    /// Test 4: Object type selection (WithObjects, OnlyTables, OnlyTablesAndViews)
    /// </summary>
    [Fact]
    public void Filter_ObjectTypes_SelectsCorrectObjects()
    {
        // Test 4.1: WithObjects - filter by specified object types
        var metadata1 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            functions: [CreateFunction("get_user", "public")],
            indexes: [CreateIndex("idx_users", "public")]
        );
        
        var builder1 = new SchemaFilterBuilder()
            .WithObjects(SchemaObjectType.Tables, SchemaObjectType.Views)
            .Build();
        var result1 = builder1.ApplyFilter(metadata1);
        
        Assert.Single(result1.Tables);
        Assert.Single(result1.Views);
        Assert.Empty(result1.Functions);
        Assert.Empty(result1.Indexes);
        
        // Test 4.2: OnlyTables - should include tables and related objects (indexes, constraints, triggers)
        var metadata2 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            indexes: [CreateIndex("idx_users", "public")],
            constraints: [CreateConstraint("pk_users", "public")],
            triggers: [CreateTrigger("trg_users", "public")]
        );
        
        var builder2 = new SchemaFilterBuilder()
            .OnlyTables()
            .Build();
        var result2 = builder2.ApplyFilter(metadata2);
        
        Assert.Single(result2.Tables);
        Assert.Empty(result2.Views);
        Assert.Single(result2.Indexes);
        Assert.Single(result2.Constraints);
        Assert.Single(result2.Triggers);
        
        // Test 4.3: OnlyTablesAndViews - should include only tables and views
        var metadata3 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            functions: [CreateFunction("get_user", "public")]
        );
        
        var builder3 = new SchemaFilterBuilder()
            .OnlyTablesAndViews()
            .Build();
        var result3 = builder3.ApplyFilter(metadata3);
        
        Assert.Single(result3.Tables);
        Assert.Single(result3.Views);
        Assert.Empty(result3.Functions);
        
        // Test 4.4: WithObjects with SchemaObjectType.None should ignore None
        var metadata4 = CreateTestMetadata(
            tables: [CreateTable("users", "public")]
        );
        
        var builder4 = new SchemaFilterBuilder()
            .WithObjects(SchemaObjectType.None, SchemaObjectType.Tables)
            .Build();
        var result4 = builder4.ApplyFilter(metadata4);
        
        Assert.Single(result4.Tables);
    }

    /// <summary>
    /// Test 5: Complex filters, comment parsing, strict mode, null handling, and edge cases
    /// </summary>
    [Fact]
    public void Filter_ComplexScenariosAndEdgeCases_WorksCorrectly()
    {
        // Test 5.1: Multiple filters combined (schema, table, view patterns)
        var metadata1 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("orders", "public"),
                CreateTable("temp_data", "public"),
                CreateTable("products", "app")
            ],
            views:
            [
                CreateView("user_view", "public"),
                CreateView("test_view", "public")
            ]
        );
        
        var builder1 = new SchemaFilterBuilder()
            .IncludeOnlySchemas("public")
            .ExcludeTables("^temp_.*")
            .ExcludeViews("^test_.*")
            .Build();
        var result1 = builder1.ApplyFilter(metadata1);
        
        Assert.Equal(2, result1.Tables.Count);
        Assert.Contains(result1.Tables, t => t.Name == "users");
        Assert.Contains(result1.Tables, t => t.Name == "orders");
        Assert.Single(result1.Views);
        Assert.Equal("user_view", result1.Views[0].Name);
        
        // Test 5.2: No filters - should include all objects
        var metadata2 = CreateTestMetadata(
            tables: [CreateTable("users", "public"), CreateTable("orders", "app")],
            views: [CreateView("user_view", "public")],
            functions: [CreateFunction("get_user", "public")]
        );
        
        var builder2 = new SchemaFilterBuilder()
            .Build();
        var result2 = builder2.ApplyFilter(metadata2);
        
        Assert.Equal(2, result2.Tables.Count);
        Assert.Single(result2.Views);
        Assert.Single(result2.Functions);
        
        // Test 5.3: Comment parsing enabled - should include comments
        var metadata3 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            comments: [CreateTableComment("users", "public", "User table")]
        );
        
        var builder3 = new SchemaFilterBuilder()
            .WithCommentParsing(true)
            .Build();
        var result3 = builder3.ApplyFilter(metadata3);
        
        Assert.Single(result3.CommentDefinition);
        
        // Test 5.4: Comment parsing disabled - should exclude comments
        var metadata4 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            comments: [CreateTableComment("users", "public", "User table")]
        );
        
        var builder4 = new SchemaFilterBuilder()
            .WithCommentParsing(false)
            .Build();
        var result4 = builder4.ApplyFilter(metadata4);
        
        Assert.Empty(result4.CommentDefinition);
        
        // Test 5.5: Strict mode enabled (doesn't affect filtering results, only parsing)
        var builder5 = new SchemaFilterBuilder()
            .WithStrictMode(true)
            .Build();
        
        Assert.NotNull(builder5);
        
        // Test 5.6: Strict mode default value should be false (doesn't throw)
        var builder6 = new SchemaFilterBuilder().Build();
        
        Assert.NotNull(builder6);
        
        // Test 5.7: Null arguments should throw ArgumentNullException
        var builder7 = new SchemaFilterBuilder();
        
        Assert.Throws<ArgumentNullException>(() => builder7.ExcludeSchemas(null!));
        Assert.Throws<ArgumentNullException>(() => builder7.IncludeOnlyTypes(null!));
        Assert.Throws<ArgumentNullException>(() => builder7.WithObjects(null!));
    }

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
        IReadOnlyList<CommentDefinition>? comments = null)
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
            CommentDefinition = comments ?? [],
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

    private static CommentDefinition CreateTableComment(string tableName, string? schema, string comment) =>
        new()
        {
            ObjectType = SchemaObjectType.Tables,
            Schema = schema,
            Name = tableName,
            Comment = comment
        };

    #endregion
}
