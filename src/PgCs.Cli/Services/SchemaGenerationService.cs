using PgCs.Cli.Configuration;
using PgCs.Common.SchemaAnalyzer.Models;

namespace PgCs.Cli.Services;

/// <summary>
/// Service for generating C# schema code
/// </summary>
public sealed class SchemaGenerationService
{
    /// <summary>
    /// Generate schema result
    /// </summary>
    public record Result(
        int TablesGenerated,
        int EnumsGenerated,
        IReadOnlyList<string> FilesCreated,
        IReadOnlyList<string> Warnings
    );

    /// <summary>
    /// Generate C# classes from PostgreSQL schema
    /// </summary>
    public async Task<Result> GenerateAsync(
        PgCsConfiguration config,
        CancellationToken cancellationToken = default)
    {
        if (config.Schema is null)
        {
            throw new InvalidOperationException("Schema configuration is missing");
        }

        var warnings = new List<string>();
        var filesCreated = new List<string>();

        // TODO: Implement actual integration with CodeGenerationPipeline
        // This is a placeholder implementation

        // Simulate work
        await Task.Delay(100, cancellationToken);

        // Example: Use CodeGenerationPipeline
        /*
        var pipeline = new CodeGenerationPipeline();
        
        if (!string.IsNullOrEmpty(config.Schema.Input.File))
        {
            pipeline.FromSchemaFile(config.Schema.Input.File);
        }
        else if (!string.IsNullOrEmpty(config.Schema.Input.Directory))
        {
            pipeline.FromSchemaDirectory(
                config.Schema.Input.Directory,
                config.Schema.Input.Pattern,
                config.Schema.Input.Recursive
            );
        }

        if (config.Schema.Filter is not null)
        {
            pipeline.FilterSchema(filter =>
            {
                if (config.Schema.Filter.Schemas.Count > 0)
                    filter.IncludeSchemas = config.Schema.Filter.Schemas;
                
                if (config.Schema.Filter.ExcludeSchemas.Count > 0)
                    filter.ExcludeSchemas = config.Schema.Filter.ExcludeSchemas;
                
                if (config.Schema.Filter.Tables.Count > 0)
                    filter.IncludeTables = config.Schema.Filter.Tables;
                
                if (config.Schema.Filter.ExcludeTables.Count > 0)
                    filter.ExcludeTables = config.Schema.Filter.ExcludeTables;
            });
        }

        pipeline.WithSchemaGeneration(options =>
        {
            options.OutputDirectory = config.Schema.Output.Directory;
            options.Namespace = config.Schema.Output.Namespace;
            options.FilePerTable = config.Schema.Output.FilePerTable;
            // ... more options
        });

        var result = await pipeline.ExecuteAsync(cancellationToken);
        */

        // Return placeholder result
        return new Result(
            TablesGenerated: 10,
            EnumsGenerated: 3,
            FilesCreated: filesCreated,
            Warnings: warnings
        );
    }

    /// <summary>
    /// Validate schema input files exist
    /// </summary>
    public static (bool IsValid, string? Error) ValidateInput(SchemaConfiguration schema)
    {
        if (!string.IsNullOrEmpty(schema.Input.File))
        {
            if (!File.Exists(schema.Input.File))
            {
                return (false, $"Schema file not found: {schema.Input.File}");
            }
        }
        else if (!string.IsNullOrEmpty(schema.Input.Directory))
        {
            if (!Directory.Exists(schema.Input.Directory))
            {
                return (false, $"Schema directory not found: {schema.Input.Directory}");
            }

            var files = Directory.GetFiles(
                schema.Input.Directory,
                schema.Input.Pattern,
                schema.Input.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
            );

            if (files.Length == 0)
            {
                return (false, $"No files matching pattern '{schema.Input.Pattern}' found in: {schema.Input.Directory}");
            }
        }
        else
        {
            return (false, "Schema input: either 'file' or 'directory' must be specified");
        }

        return (true, null);
    }
}
