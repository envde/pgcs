using PgCs.Cli.Configuration;
using PgCs.Common.CodeGeneration;
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
        IReadOnlyList<ValidationMessage> Issues
    );

    /// <summary>
    /// Validation message (error or warning)
    /// </summary>
    public record ValidationMessage(
        ValidationSeverity Severity,
        string Code,
        string Message,
        string? Location
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

        var issues = new List<ValidationMessage>();
        var filesCreated = new List<string>();

        // Create and configure pipeline
        var pipeline = CodeGenerationPipeline.Create();

        // Configure input
        if (!string.IsNullOrEmpty(config.Schema.Input.File))
        {
            pipeline.FromSchemaFile(config.Schema.Input.File);
        }
        else if (!string.IsNullOrEmpty(config.Schema.Input.Directory))
        {
            pipeline.FromSchemaDirectory(config.Schema.Input.Directory);
        }

        // Configure schema generation options
        pipeline.WithSchemaGeneration(builder =>
        {
            builder.WithNamespace(config.Schema.Output.Namespace)
                   .OutputTo(config.Schema.Output.Directory)
                   .UseRecords()
                   .WithXmlDocs()
                   .OverwriteFiles();
        });

        // Disable query generation for schema-only pipeline
        pipeline.WithoutQueryGeneration();

        // Execute pipeline
        var pipelineResult = await pipeline.ExecuteAsync(cancellationToken);

        if (!pipelineResult.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Schema generation failed: {pipelineResult.Error?.Message}",
                pipelineResult.Error);
        }

        // Extract results
        var stats = pipelineResult.Statistics!;
        var writeResult = pipelineResult.SchemaWriteResult;

        // Collect validation issues from schema metadata
        if (pipelineResult.AnalyzedSchemaMetadata?.ValidationIssues is not null)
        {
            foreach (var issue in pipelineResult.AnalyzedSchemaMetadata.ValidationIssues)
            {
                issues.Add(new ValidationMessage(
                    Severity: issue.Severity,
                    Code: issue.Code ?? "UNKNOWN",
                    Message: issue.Message ?? "Unknown error",
                    Location: issue.Location
                ));
            }
        }

        // Collect written files
        if (writeResult is not null)
        {
            filesCreated.AddRange(writeResult.WrittenFiles);
        }

        return new Result(
            TablesGenerated: stats.TablesAnalyzed,
            EnumsGenerated: stats.TypesAnalyzed,
            FilesCreated: filesCreated,
            Issues: issues
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
