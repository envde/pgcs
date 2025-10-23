using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaAnalyzer.Models.Functions;

namespace PgCs.SchemaGenerator.Tests.Helpers;

/// <summary>
/// Builder для создания тестовых SchemaMetadata
/// </summary>
public static class TestSchemaMetadataBuilder
{
    /// <summary>
    /// Создать пустую схему
    /// </summary>
    public static SchemaMetadata CreateEmpty()
    {
        return new SchemaMetadata
        {
            Tables = [],
            Views = [],
            Types = [],
            Functions = [],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Comments = new Dictionary<string, string>(),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать схему с простой таблицей
    /// </summary>
    public static SchemaMetadata CreateWithSimpleTable(
        string tableName = "users",
        string? schema = null)
    {
        var table = new TableDefinition
        {
            Name = tableName,
            Schema = schema,
            Columns =
            [
                new ColumnDefinition
                {
                    Name = "id",
                    DataType = "integer",
                    IsNullable = false,
                    IsPrimaryKey = true
                },
                new ColumnDefinition
                {
                    Name = "name",
                    DataType = "character varying",
                    MaxLength = 255,
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "email",
                    DataType = "character varying",
                    MaxLength = 255,
                    IsNullable = true
                }
            ],
            Constraints = []
        };

        return new SchemaMetadata
        {
            Tables = [table],
            Views = [],
            Types = [],
            Functions = [],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Comments = new Dictionary<string, string>(),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать схему с ENUM типом
    /// </summary>
    public static SchemaMetadata CreateWithEnumType(
        string typeName = "user_status",
        params string[] values)
    {
        var enumValues = values.Length > 0 ? values : ["active", "inactive", "pending"];

        var enumType = new TypeDefinition
        {
            Name = typeName,
            Schema = null,
            Kind = TypeKind.Enum,
            EnumValues = enumValues
        };

        return new SchemaMetadata
        {
            Tables = [],
            Views = [],
            Types = [enumType],
            Functions = [],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Comments = new Dictionary<string, string>(),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать схему с VIEW
    /// </summary>
    public static SchemaMetadata CreateWithView(
        string viewName = "active_users",
        string? schema = null)
    {
        var view = new ViewDefinition
        {
            Name = viewName,
            Schema = schema,
            Query = "SELECT id, name FROM users WHERE status = 'active'",
            Columns =
            [
                new ColumnDefinition
                {
                    Name = "id",
                    DataType = "integer",
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "name",
                    DataType = "character varying",
                    MaxLength = 255,
                    IsNullable = false
                }
            ]
        };

        return new SchemaMetadata
        {
            Tables = [],
            Views = [view],
            Types = [],
            Functions = [],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Comments = new Dictionary<string, string>(),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать схему с функцией
    /// </summary>
    public static SchemaMetadata CreateWithFunction(
        string functionName = "get_user_count",
        string returnType = "integer")
    {
        var function = new FunctionDefinition
        {
            Name = functionName,
            Schema = null,
            ReturnType = returnType,
            Parameters = [],
            Language = "plpgsql",
            Body = "BEGIN RETURN (SELECT COUNT(*) FROM users); END;"
        };

        return new SchemaMetadata
        {
            Tables = [],
            Views = [],
            Types = [],
            Functions = [function],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Comments = new Dictionary<string, string>(),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать комплексную схему с разными элементами
    /// </summary>
    public static SchemaMetadata CreateComplex()
    {
        var enumType = new TypeDefinition
        {
            Name = "user_role",
            Schema = null,
            Kind = TypeKind.Enum,
            EnumValues = ["admin", "user", "guest"]
        };

        var table = new TableDefinition
        {
            Name = "users",
            Schema = null,
            Columns =
            [
                new ColumnDefinition
                {
                    Name = "id",
                    DataType = "integer",
                    IsNullable = false,
                    IsPrimaryKey = true
                },
                new ColumnDefinition
                {
                    Name = "username",
                    DataType = "character varying",
                    MaxLength = 50,
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "role",
                    DataType = "user_role",
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "created_at",
                    DataType = "timestamp without time zone",
                    IsNullable = false,
                    DefaultValue = "CURRENT_TIMESTAMP"
                }
            ],
            Constraints = []
        };

        var view = new ViewDefinition
        {
            Name = "admin_users",
            Schema = null,
            Query = "SELECT * FROM users WHERE role = 'admin'",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false },
                new ColumnDefinition { Name = "username", DataType = "character varying", IsNullable = false },
                new ColumnDefinition { Name = "role", DataType = "user_role", IsNullable = false },
                new ColumnDefinition { Name = "created_at", DataType = "timestamp without time zone", IsNullable = false }
            ]
        };

        var function = new FunctionDefinition
        {
            Name = "get_admin_count",
            Schema = null,
            ReturnType = "integer",
            Parameters = [],
            Language = "plpgsql",
            Body = "BEGIN RETURN (SELECT COUNT(*) FROM users WHERE role = 'admin'); END;"
        };

        return new SchemaMetadata
        {
            Tables = [table],
            Views = [view],
            Types = [enumType],
            Functions = [function],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Comments = new Dictionary<string, string>(),
            AnalyzedAt = DateTime.UtcNow
        };
    }
}
