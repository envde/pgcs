using PgCs.Core.Schema.Analyzer;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Consolidated tests for SchemaFilterBuilderExtensions - testing production, tables/views, and clean schema filters
/// </summary>
public sealed class SchemaFilterBuilderExtensionsTests
{
    /// <summary>
    /// Test 1: CreateProductionFilter - excludes system objects, test schemas, includes public schema and comments
    /// </summary>
    [Fact]
    public void CreateProductionFilter_AppliesAllRulesCorrectly()
    {
        // Test 1.1: Exclude system objects (pg_catalog, information_schema, pg_* tables)
        var metadata1 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("pg_stat", "pg_catalog"),
                CreateTable("tables", "information_schema"),
                CreateTable("pg_test", "public")
            ]
        );
        
        var filter1 = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result1 = filter1.ApplyFilter(metadata1);
        
        Assert.Single(result1.Tables);
        Assert.Equal("users", result1.Tables[0].Name);
        Assert.Equal("public", result1.Tables[0].Schema);
        
        // Test 1.2: Exclude test schemas (test, tests)
        var metadata2 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("test_data", "test"),
                CreateTable("test_users", "tests")
            ]
        );
        
        var filter2 = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result2 = filter2.ApplyFilter(metadata2);
        
        Assert.Single(result2.Tables);
        Assert.Equal("public", result2.Tables[0].Schema);
        
        // Test 1.3: Include only public schema
        var metadata3 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("test_users", "public"),
                CreateTable("data", "app")
            ]
        );
        
        var filter3 = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result3 = filter3.ApplyFilter(metadata3);
        
        Assert.Equal(2, result3.Tables.Count);
        Assert.All(result3.Tables, t => Assert.Equal("public", t.Schema));
        
        // Test 1.4: Include all object types (tables, views, enums, functions, indexes, triggers, constraints)
        var metadata4 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            enums: [CreateEnum("status", "public")],
            functions: [CreateFunction("get_user", "public")],
            indexes: [CreateIndex("idx_users", "public")],
            triggers: [CreateTrigger("trg_users", "public")],
            constraints: [CreateConstraint("pk_users", "public")]
        );
        
        var filter4 = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result4 = filter4.ApplyFilter(metadata4);
        
        Assert.Single(result4.Tables);
        Assert.Single(result4.Views);
        Assert.Single(result4.Enums);
        Assert.Single(result4.Functions);
        Assert.Single(result4.Indexes);
        Assert.Single(result4.Triggers);
        Assert.Single(result4.Constraints);
        
        // Test 1.5: Include comments
        var metadata5 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            comments: [CreateTableComment("users", "public", "User table")]
        );
        
        var filter5 = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result5 = filter5.ApplyFilter(metadata5);
        
        Assert.Single(result5.CommentDefinition);
        
        // Test 1.6: Mixed case schemas (case-insensitive for system schemas)
        var metadata6 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "Public"),
                CreateTable("data", "Test"),
                CreateTable("info", "INFORMATION_SCHEMA")
            ]
        );
        
        var filter6 = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result6 = filter6.ApplyFilter(metadata6);
        
        Assert.Single(result6.Tables);
        Assert.Equal("Public", result6.Tables[0].Schema);
        
        // Test 1.7: Should not exclude valid production tables
        var metadata7 = CreateTestMetadata(
            tables:
            [
                CreateTable("user_profiles", "public"),
                CreateTable("product_catalog", "public"),
                CreateTable("order_items", "public"),
                CreateTable("customer_data", "public")
            ]
        );
        
        var filter7 = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result7 = filter7.ApplyFilter(metadata7);
        
        Assert.Equal(4, result7.Tables.Count);
    }

    /// <summary>
    /// Test 2: CreateTablesAndViewsFilter - includes only tables and views, excludes system objects, no comments
    /// </summary>
    [Fact]
    public void CreateTablesAndViewsFilter_IncludesOnlyTablesAndViewsCorrectly()
    {
        // Test 2.1: Include only tables and views, exclude other object types
        var metadata1 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            functions: [CreateFunction("get_user", "public")],
            enums: [CreateEnum("status", "public")],
            indexes: [CreateIndex("idx_users", "public")]
        );
        
        var filter1 = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result1 = filter1.ApplyFilter(metadata1);
        
        Assert.Single(result1.Tables);
        Assert.Single(result1.Views);
        Assert.Empty(result1.Functions);
        Assert.Empty(result1.Enums);
        Assert.Empty(result1.Indexes);
        
        // Test 2.2: Exclude system objects (pg_catalog, information_schema)
        var metadata2 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("pg_stat", "pg_catalog")
            ],
            views:
            [
                CreateView("user_view", "public"),
                CreateView("pg_views", "pg_catalog")
            ]
        );
        
        var filter2 = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result2 = filter2.ApplyFilter(metadata2);
        
        Assert.Single(result2.Tables);
        Assert.Equal("users", result2.Tables[0].Name);
        Assert.Single(result2.Views);
        Assert.Equal("user_view", result2.Views[0].Name);
        
        // Test 2.3: Should not include comments (no WithCommentParsing)
        var metadata3 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("test_users", "public")
            ],
            views:
            [
                CreateView("user_view", "public"),
                CreateView("temp_view", "public")
            ],
            comments: [CreateTableComment("users", "public", "User table")]
        );
        
        var filter3 = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result3 = filter3.ApplyFilter(metadata3);
        
        Assert.Equal(2, result3.Tables.Count);
        Assert.Equal(2, result3.Views.Count);
        Assert.Empty(result3.CommentDefinition);
        
        // Test 2.4: Should not affect table content
        var originalTable = CreateTable("users", "public");
        var metadata4 = CreateTestMetadata(tables: [originalTable]);
        
        var filter4 = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result4 = filter4.ApplyFilter(metadata4);
        
        Assert.Single(result4.Tables);
        Assert.Equal(originalTable.Name, result4.Tables[0].Name);
        Assert.Equal(originalTable.Schema, result4.Tables[0].Schema);
    }

    /// <summary>
    /// Test 3: CreateCleanSchemaFilter - includes all object types, excludes system/test/temp objects, includes comments
    /// </summary>
    [Fact]
    public void CreateCleanSchemaFilter_AppliesCleaningRulesCorrectly()
    {
        // Test 3.1: Include all object types (tables, views, enums, composites, domains, functions, indexes, triggers, constraints, partitions)
        var metadata1 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            enums: [CreateEnum("status", "public")],
            composites: [CreateComposite("address", "public")],
            domains: [CreateDomain("email", "public")],
            functions: [CreateFunction("get_user", "public")],
            indexes: [CreateIndex("idx_users", "public")],
            triggers: [CreateTrigger("trg_users", "public")],
            constraints: [CreateConstraint("pk_users", "public")],
            partitions: [CreatePartition("users_2024", "public")]
        );
        
        var filter1 = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result1 = filter1.ApplyFilter(metadata1);
        
        Assert.Single(result1.Tables);
        Assert.Single(result1.Views);
        Assert.Single(result1.Enums);
        Assert.Single(result1.Composites);
        Assert.Single(result1.Domains);
        Assert.Single(result1.Functions);
        Assert.Single(result1.Indexes);
        Assert.Single(result1.Triggers);
        Assert.Single(result1.Constraints);
        Assert.Single(result1.Partitions);
        
        // Test 3.2: Exclude system objects (pg_catalog, information_schema)
        var metadata2 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("pg_stat", "pg_catalog"),
                CreateTable("tables", "information_schema")
            ]
        );
        
        var filter2 = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result2 = filter2.ApplyFilter(metadata2);
        
        Assert.Single(result2.Tables);
        Assert.Equal("public", result2.Tables[0].Schema);
        
        // Test 3.3: Exclude test and temp tables (test_*, temp_*)
        var metadata3 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("test_users", "public"),
                CreateTable("temp_data", "public")
            ]
        );
        
        var filter3 = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result3 = filter3.ApplyFilter(metadata3);
        
        Assert.Single(result3.Tables);
        Assert.Equal("users", result3.Tables[0].Name);
        
        // Test 3.4: Include comments
        var metadata4 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            comments: [CreateTableComment("users", "public", "User table")]
        );
        
        var filter4 = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result4 = filter4.ApplyFilter(metadata4);
        
        Assert.Single(result4.CommentDefinition);
        
        // Test 3.5: Preserve object relationships (tables, indexes, constraints)
        var metadata5 = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            indexes: [CreateIndex("idx_users", "public")],
            constraints: [CreateConstraint("pk_users", "public")]
        );
        
        var filter5 = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result5 = filter5.ApplyFilter(metadata5);
        
        Assert.Single(result5.Tables);
        Assert.Single(result5.Indexes);
        Assert.Single(result5.Constraints);
    }

    /// <summary>
    /// Test 4: Filter composition and edge cases - combining filters, empty metadata
    /// </summary>
    [Fact]
    public void FilterCompositionAndEdgeCases_WorkCorrectly()
    {
        // Test 4.1: Combine production filter with additional filters
        var metadata1 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("orders", "public"),
                CreateTable("products", "app")
            ]
        );
        
        var builder1 = new SchemaFilterBuilder()
            .ExcludeSystemObjects()
            .IncludeOnlySchemas("public")
            .ExcludeTables("^orders$");
        var filter1 = builder1.Build();
        var result1 = filter1.ApplyFilter(metadata1);
        
        Assert.Single(result1.Tables);
        Assert.Equal("users", result1.Tables[0].Name);
        
        // Test 4.2: Combine tables/views filter with schema filter
        var metadata2 = CreateTestMetadata(
            tables:
            [
                CreateTable("users", "public"),
                CreateTable("orders", "app")
            ],
            views:
            [
                CreateView("user_view", "public"),
                CreateView("order_view", "app")
            ]
        );
        
        var builder2 = new SchemaFilterBuilder()
            .OnlyTablesAndViews()
            .ExcludeSystemObjects()
            .IncludeOnlySchemas("public");
        var filter2 = builder2.Build();
        var result2 = filter2.ApplyFilter(metadata2);
        
        Assert.Single(result2.Tables);
        Assert.Single(result2.Views);
        Assert.Equal("public", result2.Tables[0].Schema);
        Assert.Equal("public", result2.Views[0].Schema);
        
        // Test 4.3: Production filter with empty metadata
        var emptyMetadata1 = CreateTestMetadata();
        
        var filter3 = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result3 = filter3.ApplyFilter(emptyMetadata1);
        
        Assert.Empty(result3.Tables);
        Assert.Empty(result3.Views);
        Assert.Empty(result3.Functions);
        
        // Test 4.4: Tables/Views filter with empty metadata
        var emptyMetadata2 = CreateTestMetadata();
        
        var filter4 = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result4 = filter4.ApplyFilter(emptyMetadata2);
        
        Assert.Empty(result4.Tables);
        Assert.Empty(result4.Views);
        
        // Test 4.5: Clean schema filter with empty metadata
        var emptyMetadata3 = CreateTestMetadata();
        
        var filter5 = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result5 = filter5.ApplyFilter(emptyMetadata3);
        
        Assert.Empty(result5.Tables);
        Assert.Empty(result5.Views);
        Assert.Empty(result5.Functions);
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

    private static PartitionDefinition CreatePartition(string name, string? schema) =>
        new()
        {
            Name = name,
            Schema = schema,
            ParentTableName = "parent_table",
            Strategy = PartitionStrategy.Range
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
