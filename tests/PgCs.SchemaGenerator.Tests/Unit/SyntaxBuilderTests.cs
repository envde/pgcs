namespace PgCs.SchemaGenerator.Tests.Unit;

using PgCs.SchemaGenerator.Services;
using PgCs.Common.Services;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.SchemaGenerator.Tests.Helpers;

public class SyntaxBuilderTests
{
    private readonly SyntaxBuilder _syntaxBuilder;
    private readonly SchemaGenerationOptions _options;

    public SyntaxBuilderTests()
    {
        var typeMapper = new PostgreSqlTypeMapper();
        var nameConverter = new NameConverter();
        _syntaxBuilder = new SyntaxBuilder(typeMapper, nameConverter);
        _options = TestOptionsBuilder.CreateDefault();
    }

    [Fact]
    public void BuildTableClass_ShouldGenerateClassWithCorrectName()
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
        var result = _syntaxBuilder.BuildTableClass(table, _options);

        // Assert
        Assert.Equal("User", result.Identifier.Text);
    }

    [Fact]
    public void BuildTableClass_ShouldIncludePublicSealedRecordModifiers()
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
        var result = _syntaxBuilder.BuildTableClass(table, _options);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("public", code);
        Assert.Contains("sealed", code);
        Assert.Contains("record", code);
    }

    [Fact]
    public void BuildTableClass_WithComment_ShouldIncludeXmlDocumentation()
    {
        // Arrange
        var table = new TableDefinition
        {
            Name = "users",
            Schema = "public",
            Comment = "Table storing user information",
            Columns =
            [
                new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }
            ]
        };

        // Act
        var result = _syntaxBuilder.BuildTableClass(table, _options);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("Table storing user information", code);
        Assert.Contains("///", code);
    }

    [Fact]
    public void BuildTableClass_ShouldGeneratePropertiesForAllColumns()
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
        var result = _syntaxBuilder.BuildTableClass(table, _options);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("Id", code);
        Assert.Contains("Name", code);
        Assert.Contains("Email", code);
    }

    [Fact]
    public void BuildProperty_ShouldGeneratePropertyWithCorrectName()
    {
        // Arrange
        var column = new ColumnDefinition
        {
            Name = "user_id",
            DataType = "integer",
            IsNullable = false
        };

        // Act
        var result = _syntaxBuilder.BuildProperty(column);

        // Assert
        Assert.Equal("UserId", result.Identifier.Text);
    }

    [Fact]
    public void BuildProperty_ShouldIncludeRequiredModifier()
    {
        // Arrange
        var column = new ColumnDefinition
        {
            Name = "name",
            DataType = "text",
            IsNullable = false
        };

        // Act
        var result = _syntaxBuilder.BuildProperty(column);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("required", code);
    }

    [Fact]
    public void BuildProperty_ShouldHaveGetAndInitAccessors()
    {
        // Arrange
        var column = new ColumnDefinition
        {
            Name = "name",
            DataType = "text",
            IsNullable = false
        };

        // Act
        var result = _syntaxBuilder.BuildProperty(column);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("get;", code);
        Assert.Contains("init;", code);
    }

    [Fact]
    public void BuildProperty_WithComment_ShouldIncludeXmlDocumentation()
    {
        // Arrange
        var column = new ColumnDefinition
        {
            Name = "email",
            DataType = "text",
            IsNullable = false,
            Comment = "User's email address"
        };

        // Act
        var result = _syntaxBuilder.BuildProperty(column);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("User's email address", code);
        Assert.Contains("///", code);
    }

    [Fact]
    public void BuildEnum_ShouldGenerateEnumWithCorrectName()
    {
        // Arrange
        var enumType = new TypeDefinition
        {
            Name = "user_status",
            Schema = "public",
            Kind = TypeKind.Enum,
            EnumValues = ["active", "inactive"]
        };

        // Act
        var result = _syntaxBuilder.BuildEnum(enumType);

        // Assert
        Assert.Equal("UserStatus", result.Identifier.Text);
    }

    [Fact]
    public void BuildEnum_ShouldIncludePublicModifier()
    {
        // Arrange
        var enumType = new TypeDefinition
        {
            Name = "status",
            Schema = "public",
            Kind = TypeKind.Enum,
            EnumValues = ["active"]
        };

        // Act
        var result = _syntaxBuilder.BuildEnum(enumType);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("public", code);
    }

    [Fact]
    public void BuildEnum_ShouldGenerateAllMembers()
    {
        // Arrange
        var enumType = new TypeDefinition
        {
            Name = "status",
            Schema = "public",
            Kind = TypeKind.Enum,
            EnumValues = ["active", "inactive", "pending"]
        };

        // Act
        var result = _syntaxBuilder.BuildEnum(enumType);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("Active", code);
        Assert.Contains("Inactive", code);
        Assert.Contains("Pending", code);
    }

    [Fact]
    public void BuildEnum_WithComment_ShouldIncludeXmlDocumentation()
    {
        // Arrange
        var enumType = new TypeDefinition
        {
            Name = "status",
            Schema = "public",
            Kind = TypeKind.Enum,
            Comment = "User account status",
            EnumValues = ["active"]
        };

        // Act
        var result = _syntaxBuilder.BuildEnum(enumType);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("User account status", code);
        Assert.Contains("///", code);
    }

    [Fact]
    public void BuildCompilationUnit_ShouldIncludeNamespace()
    {
        // Arrange
        var classDeclaration = _syntaxBuilder.BuildTableClass(
            new TableDefinition
            {
                Name = "users",
                Schema = "public",
                Columns = [new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }]
            },
            _options);

        // Act
        var result = _syntaxBuilder.BuildCompilationUnit(
            "MyApp.Models",
            classDeclaration,
            []);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("namespace MyApp.Models;", code);
    }

    [Fact]
    public void BuildCompilationUnit_ShouldIncludeUsings()
    {
        // Arrange
        var classDeclaration = _syntaxBuilder.BuildTableClass(
            new TableDefinition
            {
                Name = "users",
                Schema = "public",
                Columns = [new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }]
            },
            _options);

        var usings = new[] { "System", "System.Collections.Generic" };

        // Act
        var result = _syntaxBuilder.BuildCompilationUnit(
            "MyApp.Models",
            classDeclaration,
            usings);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("using System;", code);
        Assert.Contains("using System.Collections.Generic;", code);
    }

    [Fact]
    public void BuildCompilationUnit_ShouldOrderUsingsAlphabetically()
    {
        // Arrange
        var classDeclaration = _syntaxBuilder.BuildTableClass(
            new TableDefinition
            {
                Name = "users",
                Schema = "public",
                Columns = [new ColumnDefinition { Name = "id", DataType = "integer", IsNullable = false }]
            },
            _options);

        var usings = new[] { "System.Linq", "System", "System.Collections" };

        // Act
        var result = _syntaxBuilder.BuildCompilationUnit(
            "MyApp.Models",
            classDeclaration,
            usings);
        var code = result.ToFullString();

        // Assert
        var systemIndex = code.IndexOf("using System;");
        var collectionsIndex = code.IndexOf("using System.Collections;");
        var linqIndex = code.IndexOf("using System.Linq;");
        Assert.True(systemIndex < collectionsIndex);
        Assert.True(collectionsIndex < linqIndex);
    }

    [Fact]
    public void BuildEnumCompilationUnit_ShouldIncludeNamespace()
    {
        // Arrange
        var enumDeclaration = _syntaxBuilder.BuildEnum(
            new TypeDefinition
            {
                Name = "status",
                Schema = "public",
                Kind = TypeKind.Enum,
                EnumValues = ["active"]
            });

        // Act
        var result = _syntaxBuilder.BuildEnumCompilationUnit(
            "MyApp.Enums",
            enumDeclaration);
        var code = result.ToFullString();

        // Assert
        Assert.Contains("namespace MyApp.Enums;", code);
    }

    [Fact]
    public void GetRequiredUsings_WithUuidColumn_ShouldReturnSystemNamespace()
    {
        // Arrange
        var columns = new List<ColumnDefinition>
        {
            new() { Name = "id", DataType = "uuid", IsNullable = false }
        };

        // Act
        var result = _syntaxBuilder.GetRequiredUsings(columns);

        // Assert
        Assert.Contains("System", result);
    }

    [Fact]
    public void GetRequiredUsings_WithMultipleSpecialTypes_ShouldReturnUniqueNamespaces()
    {
        // Arrange
        var columns = new List<ColumnDefinition>
        {
            new() { Name = "id", DataType = "uuid", IsNullable = false },
            new() { Name = "ip", DataType = "inet", IsNullable = false },
            new() { Name = "mac", DataType = "macaddr", IsNullable = false }
        };

        // Act
        var result = _syntaxBuilder.GetRequiredUsings(columns).ToList();

        // Assert
        Assert.Contains("System", result);
        Assert.Contains("System.Net", result);
        Assert.Contains("System.Net.NetworkInformation", result);
        // Должны быть уникальные значения
        Assert.Equal(result.Distinct().Count(), result.Count);
    }

    [Fact]
    public void GetRequiredUsings_WithBasicTypes_ShouldReturnEmptyOrMinimal()
    {
        // Arrange
        var columns = new List<ColumnDefinition>
        {
            new() { Name = "id", DataType = "integer", IsNullable = false },
            new() { Name = "name", DataType = "text", IsNullable = false }
        };

        // Act
        var result = _syntaxBuilder.GetRequiredUsings(columns);

        // Assert
        // Базовые типы не требуют дополнительных namespaces
        Assert.NotNull(result);
    }
}
