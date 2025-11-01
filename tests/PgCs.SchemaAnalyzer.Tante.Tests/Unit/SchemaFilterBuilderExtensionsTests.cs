using PgCs.Core.Schema.Analyzer;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Тесты для SchemaFilterBuilderExtensions
/// </summary>
public sealed class SchemaFilterBuilderExtensionsTests
{
    #region CreateProductionFilter Tests

    [Fact]
    public void CreateProductionFilter_ShouldExcludeSystemObjects()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("pg_stat", "pg_catalog"),
                CreateTable("tables", "information_schema"),
                CreateTable("pg_test", "public")
            ]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("users", result.Tables[0].Name);
        Assert.Equal("public", result.Tables[0].Schema);
    }

    [Fact]
    public void CreateProductionFilter_ShouldExcludeTestSchema()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("test_data", "test"),
                CreateTable("test_users", "tests")
            ]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("public", result.Tables[0].Schema);
    }

    [Fact]
    public void CreateProductionFilter_ShouldIncludeOnlyPublicSchema()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("test_users", "public"),
                CreateTable("data", "app")
            ]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert - CreateProductionFilter фильтрует только по схеме public и системным объектам
        Assert.Equal(2, result.Tables.Count);
        Assert.All(result.Tables, t => Assert.Equal("public", t.Schema));
    }

    [Fact]
    public void CreateProductionFilter_ShouldIncludeAllObjectTypes()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            enums: [CreateEnum("status", "public")],
            functions: [CreateFunction("get_user", "public")],
            indexes: [CreateIndex("idx_users", "public")],
            triggers: [CreateTrigger("trg_users", "public")],
            constraints: [CreateConstraint("pk_users", "public")]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Single(result.Views);
        Assert.Single(result.Enums);
        Assert.Single(result.Functions);
        Assert.Single(result.Indexes);
        Assert.Single(result.Triggers);
        Assert.Single(result.Constraints);
    }

    [Fact]
    public void CreateProductionFilter_ShouldIncludeComments()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            comments: [CreateTableComment("users", "public", "User table")]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.CommentDefinition);
    }

    #endregion

    #region CreateTablesAndViewsFilter Tests

    [Fact]
    public void CreateTablesAndViewsFilter_ShouldIncludeOnlyTablesAndViews()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            views: [CreateView("user_view", "public")],
            functions: [CreateFunction("get_user", "public")],
            enums: [CreateEnum("status", "public")],
            indexes: [CreateIndex("idx_users", "public")]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Single(result.Views);
        Assert.Empty(result.Functions);
        Assert.Empty(result.Enums);
        Assert.Empty(result.Indexes);
    }

    [Fact]
    public void CreateTablesAndViewsFilter_ShouldExcludeSystemObjects()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("pg_stat", "pg_catalog")
            ],
            views: [
                CreateView("user_view", "public"),
                CreateView("pg_views", "pg_catalog")
            ]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("users", result.Tables[0].Name);
        Assert.Single(result.Views);
        Assert.Equal("user_view", result.Views[0].Name);
    }

    [Fact]
    public void CreateTablesAndViewsFilter_ShouldIncludeOnlyTablesAndViewsWithoutComments()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("test_users", "public")
            ],
            views: [
                CreateView("user_view", "public"),
                CreateView("temp_view", "public")
            ],
            comments: [CreateTableComment("users", "public", "User table")]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert - TablesAndViewsFilter не включает WithCommentParsing
        Assert.Equal(2, result.Tables.Count);
        Assert.Equal(2, result.Views.Count);
        Assert.Empty(result.CommentDefinition); // Комментарии не включены
    }

    #endregion

    #region CreateCleanSchemaFilter Tests

    [Fact]
    public void CreateCleanSchemaFilter_ShouldIncludeAllObjectTypes()
    {
        // Arrange
        var metadata = CreateTestMetadata(
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

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Single(result.Views);
        Assert.Single(result.Enums);
        Assert.Single(result.Composites);
        Assert.Single(result.Domains);
        Assert.Single(result.Functions);
        Assert.Single(result.Indexes);
        Assert.Single(result.Triggers);
        Assert.Single(result.Constraints);
        Assert.Single(result.Partitions);
    }

    [Fact]
    public void CreateCleanSchemaFilter_ShouldExcludeSystemObjects()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("pg_stat", "pg_catalog"),
                CreateTable("tables", "information_schema")
            ]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("public", result.Tables[0].Schema);
    }

    [Fact]
    public void CreateCleanSchemaFilter_ShouldExcludeTestAndTempTables()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("test_users", "public"),
                CreateTable("temp_data", "public")
            ]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert - CleanSchemaFilter исключает тестовые и временные таблицы
        Assert.Single(result.Tables);
        Assert.Equal("users", result.Tables[0].Name);
    }

    [Fact]
    public void CreateCleanSchemaFilter_ShouldIncludeComments()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            comments: [CreateTableComment("users", "public", "User table")]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.CommentDefinition);
    }

    #endregion

    #region Filter Composition Tests

    [Fact]
    public void CreateProductionFilter_WithAdditionalFilters_ShouldCombine()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("orders", "public"),
                CreateTable("products", "app")
            ]
        );

        // Act
        var baseFilter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var builder = new SchemaFilterBuilder()
            .ExcludeSystemObjects()
            .IncludeOnlySchemas("public")
            .ExcludeTables("^orders$");
        var filter = builder.Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal("users", result.Tables[0].Name);
    }

    [Fact]
    public void CreateTablesAndViewsFilter_WithSchemaFilter_ShouldCombine()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "public"),
                CreateTable("orders", "app")
            ],
            views: [
                CreateView("user_view", "public"),
                CreateView("order_view", "app")
            ]
        );

        // Act
        var builder = new SchemaFilterBuilder()
            .OnlyTablesAndViews()
            .ExcludeSystemObjects()
            .IncludeOnlySchemas("public");
        var filter = builder.Build();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Single(result.Views);
        Assert.Equal("public", result.Tables[0].Schema);
        Assert.Equal("public", result.Views[0].Schema);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void CreateProductionFilter_WithEmptyMetadata_ShouldReturnEmptyResult()
    {
        // Arrange
        var metadata = CreateTestMetadata();

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Empty(result.Tables);
        Assert.Empty(result.Views);
        Assert.Empty(result.Functions);
    }

    [Fact]
    public void CreateTablesAndViewsFilter_WithEmptyMetadata_ShouldReturnEmptyResult()
    {
        // Arrange
        var metadata = CreateTestMetadata();

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Empty(result.Tables);
        Assert.Empty(result.Views);
    }

    [Fact]
    public void CreateCleanSchemaFilter_WithEmptyMetadata_ShouldReturnEmptyResult()
    {
        // Arrange
        var metadata = CreateTestMetadata();

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Empty(result.Tables);
        Assert.Empty(result.Views);
        Assert.Empty(result.Functions);
    }

    [Fact]
    public void CreateProductionFilter_WithMixedCaseSchemas_ShouldFilter()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("users", "Public"),
                CreateTable("data", "Test"),
                CreateTable("info", "INFORMATION_SCHEMA")
            ]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert - System schemas are case-insensitive
        Assert.Single(result.Tables);
        Assert.Equal("Public", result.Tables[0].Schema);
    }

    #endregion

    #region Regression Tests

    [Fact]
    public void CreateProductionFilter_ShouldNotExcludeValidProductionTables()
    {
        // Arrange - Таблицы, которые НЕ должны быть исключены
        var metadata = CreateTestMetadata(
            tables: [
                CreateTable("user_profiles", "public"),
                CreateTable("product_catalog", "public"),
                CreateTable("order_items", "public"),
                CreateTable("customer_data", "public")
            ]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateProductionFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Equal(4, result.Tables.Count);
    }

    [Fact]
    public void CreateTablesAndViewsFilter_ShouldNotAffectTableContent()
    {
        // Arrange
        var originalTable = CreateTable("users", "public");
        var metadata = CreateTestMetadata(tables: [originalTable]);

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateTablesAndViewsFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert
        Assert.Single(result.Tables);
        Assert.Equal(originalTable.Name, result.Tables[0].Name);
        Assert.Equal(originalTable.Schema, result.Tables[0].Schema);
    }

    [Fact]
    public void CreateCleanSchemaFilter_ShouldPreserveObjectRelationships()
    {
        // Arrange
        var metadata = CreateTestMetadata(
            tables: [CreateTable("users", "public")],
            indexes: [CreateIndex("idx_users", "public")],
            constraints: [CreateConstraint("pk_users", "public")]
        );

        // Act
        var filter = SchemaFilterBuilderExtensions.CreateCleanSchemaFilter();
        var result = filter.ApplyFilter(metadata);

        // Assert - Все связанные объекты должны быть включены
        Assert.Single(result.Tables);
        Assert.Single(result.Indexes);
        Assert.Single(result.Constraints);
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
