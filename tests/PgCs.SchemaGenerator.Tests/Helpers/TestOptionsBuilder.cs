namespace PgCs.SchemaGenerator.Tests.Helpers;

/// <summary>
/// Helper для создания тестовых опций генерации
/// </summary>
public static class TestOptionsBuilder
{
    /// <summary>
    /// Создать опции с минимальными требуемыми параметрами
    /// </summary>
    public static SchemaGenerationOptions CreateDefault()
    {
        return new SchemaGenerationOptions
        {
            RootNamespace = "TestNamespace",
            OutputDirectory = "/tmp/test"
        };
    }

    /// <summary>
    /// Создать опции с генерацией функций
    /// </summary>
    public static SchemaGenerationOptions CreateWithFunctions(bool generateFunctions = true)
    {
        return new SchemaGenerationOptions
        {
            RootNamespace = "TestNamespace",
            OutputDirectory = "/tmp/test",
            GenerateFunctions = generateFunctions
        };
    }

    /// <summary>
    /// Создать опции с mapping атрибутами
    /// </summary>
    public static SchemaGenerationOptions CreateWithMappingAttributes(bool generate = true)
    {
        return new SchemaGenerationOptions
        {
            RootNamespace = "TestNamespace",
            OutputDirectory = "/tmp/test",
            GenerateMappingAttributes = generate
        };
    }

    /// <summary>
    /// Создать опции с validation атрибутами
    /// </summary>
    public static SchemaGenerationOptions CreateWithValidationAttributes(bool generate = true)
    {
        return new SchemaGenerationOptions
        {
            RootNamespace = "TestNamespace",
            OutputDirectory = "/tmp/test",
            GenerateValidationAttributes = generate
        };
    }

    /// <summary>
    /// Создать опции с primary constructors
    /// </summary>
    public static SchemaGenerationOptions CreateWithPrimaryConstructors(bool use = true)
    {
        return new SchemaGenerationOptions
        {
            RootNamespace = "TestNamespace",
            OutputDirectory = "/tmp/test",
            UsePrimaryConstructors = use
        };
    }
}
