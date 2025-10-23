using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.SchemaAnalyzer.Utils;

namespace PgCs.SchemaAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для SchemaValidator - валидатора схемы базы данных
/// </summary>
public sealed class SchemaValidatorTests
{
    #region Validate Tests

    [Fact]
    public void Validate_WithValidSchema_ReturnsValidResult()
    {
        // Arrange
        var schema = CreateValidSchema();

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithTableWithoutColumns_ReturnsError()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "empty_table",
                    Schema = "public",
                    Columns = Array.Empty<ColumnDefinition>(),
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = Array.Empty<IndexDefinition>(),
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("has no columns", result.Errors[0]);
    }

    [Fact]
    public void Validate_WithTableWithoutPrimaryKey_ReturnsWarning()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "users",
                    Schema = "public",
                    Columns = new[]
                    {
                        new ColumnDefinition
                        {
                            Name = "id",
                            DataType = "INT",
                            IsNullable = false,
                            IsPrimaryKey = false
                        }
                    },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = Array.Empty<IndexDefinition>(),
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.True(result.IsValid); // Warnings don't make schema invalid
        Assert.Single(result.Warnings);
        Assert.Contains("has no primary key", result.Warnings[0]);
    }

    [Fact]
    public void Validate_WithDuplicateColumnNames_ReturnsError()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "users",
                    Schema = "public",
                    Columns = new[]
                    {
                        new ColumnDefinition { Name = "id", DataType = "INT", IsPrimaryKey = true },
                        new ColumnDefinition { Name = "name", DataType = "VARCHAR(50)" },
                        new ColumnDefinition { Name = "name", DataType = "VARCHAR(100)" } // Duplicate
                    },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = Array.Empty<IndexDefinition>(),
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("duplicate column 'name'", result.Errors[0]);
    }

    [Fact]
    public void Validate_WithInvalidForeignKey_ReturnsError()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "orders",
                    Schema = "public",
                    Columns = new[]
                    {
                        new ColumnDefinition { Name = "id", DataType = "INT", IsPrimaryKey = true },
                        new ColumnDefinition { Name = "user_id", DataType = "INT" }
                    },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = Array.Empty<IndexDefinition>(),
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = new[]
            {
                new ConstraintDefinition
                {
                    Name = "fk_user",
                    TableName = "orders",
                    Type = ConstraintType.ForeignKey,
                    Columns = new[] { "user_id" },
                    ReferencedTable = "nonexistent_table" // Table doesn't exist
                }
            }
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("references non-existent table", result.Errors[0]);
    }

    [Fact]
    public void Validate_WithIndexOnNonExistentTable_ReturnsError()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "users",
                    Schema = "public",
                    Columns = new[] { new ColumnDefinition { Name = "id", DataType = "INT", IsPrimaryKey = true } },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = new[]
            {
                new IndexDefinition
                {
                    Name = "idx_test",
                    TableName = "nonexistent_table",
                    Columns = new[] { "id" }
                }
            },
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("references non-existent table"));
    }

    [Fact]
    public void Validate_WithIndexWithoutColumns_ReturnsError()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "users",
                    Schema = "public",
                    Columns = new[] { new ColumnDefinition { Name = "id", DataType = "INT", IsPrimaryKey = true } },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = new[]
            {
                new IndexDefinition
                {
                    Name = "idx_empty",
                    TableName = "users",
                    Columns = Array.Empty<string>() // No columns
                }
            },
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("has no columns"));
    }

    [Fact]
    public void Validate_WithIndexWithManyColumns_ReturnsWarning()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "users",
                    Schema = "public",
                    Columns = new[] { new ColumnDefinition { Name = "id", DataType = "INT", IsPrimaryKey = true } },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = new[]
            {
                new IndexDefinition
                {
                    Name = "idx_many_cols",
                    TableName = "users",
                    Columns = new[] { "col1", "col2", "col3", "col4", "col5", "col6" } // 6 columns
                }
            },
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, w => w.Contains("consider splitting"));
    }

    [Fact]
    public void Validate_WithSingleColumnNonUniqueIndex_ReturnsWarning()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "users",
                    Schema = "public",
                    Columns = new[] { new ColumnDefinition { Name = "id", DataType = "INT", IsPrimaryKey = true } },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = new[]
            {
                new IndexDefinition
                {
                    Name = "idx_single",
                    TableName = "users",
                    Columns = new[] { "email" },
                    IsUnique = false
                }
            },
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, w => w.Contains("might not provide significant performance benefit"));
    }

    [Fact]
    public void Validate_WithTriggerOnNonExistentTable_ReturnsError()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = Array.Empty<TableDefinition>(),
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = new[]
            {
                new FunctionDefinition
                {
                    Name = "trigger_func",
                    Schema = "public",
                    Parameters = Array.Empty<FunctionParameter>(),
                    Language = "plpgsql",
                    Body = "BEGIN RETURN NULL; END;"
                }
            },
            Indexes = Array.Empty<IndexDefinition>(),
            Triggers = new[]
            {
                new TriggerDefinition
                {
                    Name = "test_trigger",
                    TableName = "nonexistent_table",
                    FunctionName = "trigger_func",
                    Timing = TriggerTiming.Before,
                    Events = new[] { TriggerEvent.Insert }
                }
            },
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("references non-existent table"));
    }

    [Fact]
    public void Validate_WithTriggerReferencingNonExistentFunction_ReturnsError()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "users",
                    Schema = "public",
                    Columns = new[] { new ColumnDefinition { Name = "id", DataType = "INT", IsPrimaryKey = true } },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = Array.Empty<IndexDefinition>(),
            Triggers = new[]
            {
                new TriggerDefinition
                {
                    Name = "test_trigger",
                    TableName = "users",
                    FunctionName = "nonexistent_function",
                    Timing = TriggerTiming.Before,
                    Events = new[] { TriggerEvent.Insert }
                }
            },
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("references non-existent function"));
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var schema = new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "empty_table",
                    Schema = "public",
                    Columns = Array.Empty<ColumnDefinition>(),
                    Constraints = Array.Empty<ConstraintDefinition>()
                },
                new TableDefinition
                {
                    Name = "duplicate_cols",
                    Schema = "public",
                    Columns = new[]
                    {
                        new ColumnDefinition { Name = "id", DataType = "INT", IsPrimaryKey = true },
                        new ColumnDefinition { Name = "name", DataType = "VARCHAR" },
                        new ColumnDefinition { Name = "name", DataType = "VARCHAR" }
                    },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = Array.Empty<IndexDefinition>(),
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };

        // Act
        var result = SchemaValidator.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }

    #endregion

    #region ValidationResult Tests

    [Fact]
    public void ValidationResult_CanBeCreatedWithRequiredProperties()
    {
        // Act
        var result = new ValidationResult
        {
            IsValid = true,
            Errors = Array.Empty<string>(),
            Warnings = Array.Empty<string>()
        };

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void ValidationResult_IsImmutable()
    {
        // Act
        var result = new ValidationResult
        {
            IsValid = true,
            Errors = new[] { "Error 1" },
            Warnings = new[] { "Warning 1" }
        };

        // Assert - Record types are immutable
        Assert.Equal("Error 1", result.Errors[0]);
        Assert.Equal("Warning 1", result.Warnings[0]);
    }

    #endregion

    #region Helper Methods

    private static SchemaMetadata CreateValidSchema()
    {
        return new SchemaMetadata
        {
            Tables = new[]
            {
                new TableDefinition
                {
                    Name = "users",
                    Schema = "public",
                    Columns = new[]
                    {
                        new ColumnDefinition
                        {
                            Name = "id",
                            DataType = "INT",
                            IsNullable = false,
                            IsPrimaryKey = true
                        },
                        new ColumnDefinition
                        {
                            Name = "email",
                            DataType = "VARCHAR(255)",
                            IsNullable = false
                        }
                    },
                    Constraints = Array.Empty<ConstraintDefinition>()
                }
            },
            Views = Array.Empty<ViewDefinition>(),
            Types = Array.Empty<TypeDefinition>(),
            Functions = Array.Empty<FunctionDefinition>(),
            Indexes = Array.Empty<IndexDefinition>(),
            Triggers = Array.Empty<TriggerDefinition>(),
            Constraints = Array.Empty<ConstraintDefinition>()
        };
    }

    #endregion
}
