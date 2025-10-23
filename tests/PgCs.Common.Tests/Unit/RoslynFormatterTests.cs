using PgCs.Common.Services;

namespace PgCs.Common.Tests.Unit;

/// <summary>
/// Тесты для RoslynFormatter - форматирование C# кода через Roslyn
/// </summary>
public sealed class RoslynFormatterTests
{
    private readonly RoslynFormatter _formatter = new();

    [Fact]
    public void Format_ValidClass_ReturnsFormattedCode()
    {
        // Arrange
        const string unformatted = "public class User{public int Id{get;set;}public string Name{get;set;}}";
        
        // Act
        var result = _formatter.Format(unformatted);
        
        // Assert
        Assert.Contains("public class User", result);
        Assert.Contains("public int Id", result);
        Assert.Contains("public string Name", result);
        Assert.Contains("{ get; set; }", result);
    }

    [Fact]
    public void Format_ValidMethod_ReturnsFormattedCode()
    {
        // Arrange
        const string unformatted = "public void Calculate(){int x=10;int y=20;return x+y;}";
        
        // Act
        var result = _formatter.Format(unformatted);
        
        // Assert
        Assert.Contains("public void Calculate()", result);
        Assert.Contains("int x = 10;", result);
        Assert.Contains("int y = 20;", result);
    }

    [Fact]
    public void Format_ValidProperty_ReturnsFormattedCode()
    {
        // Arrange
        const string unformatted = "public int Id{get;set;}";
        
        // Act
        var result = _formatter.Format(unformatted);
        
        // Assert
        Assert.Contains("public int Id { get; set; }", result);
    }

    [Fact]
    public void Format_AlreadyFormatted_ReturnsIdempotentResult()
    {
        // Arrange
        const string formatted = @"public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}";
        
        // Act
        var result1 = _formatter.Format(formatted);
        var result2 = _formatter.Format(result1);
        
        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Format_InvalidSyntax_ReturnsOriginalCode()
    {
        // Arrange
        const string invalid = "public class { invalid syntax }}}";
        
        // Act
        var result = _formatter.Format(invalid);
        
        // Assert - should return original code on error
        Assert.Equal(invalid, result);
    }

    [Fact]
    public void Format_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        const string empty = "";
        
        // Act
        var result = _formatter.Format(empty);
        
        // Assert
        Assert.Equal(empty, result);
    }

    [Fact]
    public void Format_WhitespaceOnly_FormatsToMinimal()
    {
        // Arrange
        const string whitespace = "   \n\t  ";
        
        // Act
        var result = _formatter.Format(whitespace);
        
        // Assert - Roslyn formats whitespace to minimal form
        Assert.Equal("\n", result);
    }

    [Fact]
    public void Format_ComplexClass_PreservesStructure()
    {
        // Arrange
        const string complex = @"
namespace MyNamespace{
public class MyClass{
private int _field;
public int Property{get=>_field;set=>_field=value;}
public void Method(){if(Property>0){Console.WriteLine(Property);}}
}
}";
        
        // Act
        var result = _formatter.Format(complex);
        
        // Assert
        Assert.Contains("namespace MyNamespace", result);
        Assert.Contains("public class MyClass", result);
        Assert.Contains("private int _field;", result);
        Assert.Contains("public int Property", result);
        Assert.Contains("public void Method()", result);
    }

    [Fact]
    public void Format_WithUsings_FormatsCorrectly()
    {
        // Arrange
        const string withUsings = @"using System;using System.Collections.Generic;
public class Test{}";
        
        // Act
        var result = _formatter.Format(withUsings);
        
        // Assert
        Assert.Contains("using System;", result);
        Assert.Contains("using System.Collections.Generic;", result);
        Assert.Contains("public class Test", result);
    }

    [Fact]
    public void Format_MultipleClasses_FormatsAll()
    {
        // Arrange
        const string multiple = @"
public class First{public int Id{get;set;}}
public class Second{public string Name{get;set;}}";
        
        // Act
        var result = _formatter.Format(multiple);
        
        // Assert
        Assert.Contains("public class First", result);
        Assert.Contains("public int Id { get; set; }", result);
        Assert.Contains("public class Second", result);
        Assert.Contains("public string Name { get; set; }", result);
    }

    [Fact]
    public void Format_NestedTypes_PreservesNesting()
    {
        // Arrange
        const string nested = @"
public class Outer{
public class Inner{
public int Value{get;set;}
}
}";
        
        // Act
        var result = _formatter.Format(nested);
        
        // Assert
        Assert.Contains("public class Outer", result);
        Assert.Contains("public class Inner", result);
        Assert.Contains("public int Value { get; set; }", result);
    }

    [Fact]
    public void Format_Records_FormatsCorrectly()
    {
        // Arrange
        const string record = "public record Person(string Name,int Age);";
        
        // Act
        var result = _formatter.Format(record);
        
        // Assert
        Assert.Contains("public record Person(string Name, int Age);", result);
    }
}
