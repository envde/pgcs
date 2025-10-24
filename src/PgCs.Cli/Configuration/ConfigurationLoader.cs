using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PgCs.Cli.Configuration;

/// <summary>
/// Loads and parses YAML configuration files
/// </summary>
public sealed class ConfigurationLoader
{
    private readonly IDeserializer _deserializer;

    public ConfigurationLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Load configuration from file
    /// </summary>
    public PgCsConfiguration Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        try
        {
            var yaml = File.ReadAllText(filePath);
            var config = _deserializer.Deserialize<PgCsConfiguration>(yaml);

            if (config is null)
            {
                throw new InvalidOperationException("Failed to deserialize configuration file");
            }

            return config;
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new InvalidOperationException($"Invalid YAML syntax in configuration file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Load configuration from string content
    /// </summary>
    public PgCsConfiguration LoadFromString(string yaml)
    {
        try
        {
            var config = _deserializer.Deserialize<PgCsConfiguration>(yaml);

            if (config is null)
            {
                throw new InvalidOperationException("Failed to deserialize configuration");
            }

            return config;
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new InvalidOperationException($"Invalid YAML syntax: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create default minimal configuration
    /// </summary>
    public static string CreateMinimalConfig(string schemaInput, string schemaOutput, string queriesInput, string queriesOutput)
    {
        return $"""
            schema:
              input:
                file: "{schemaInput}"
              output:
                directory: "{schemaOutput}"
                namespace: "Generated.Schema"
            
            queries:
              input:
                file: "{queriesInput}"
              output:
                directory: "{queriesOutput}"
                namespace: "Generated.Queries"
            """;
    }

    /// <summary>
    /// Validate configuration file exists and is readable
    /// </summary>
    public static (bool IsValid, string? Error) ValidateFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return (false, "Configuration file path is empty");
        }

        if (!File.Exists(filePath))
        {
            return (false, $"Configuration file not found: {filePath}");
        }

        try
        {
            // Try to read file
            _ = File.ReadAllText(filePath);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Cannot read configuration file: {ex.Message}");
        }
    }
}
