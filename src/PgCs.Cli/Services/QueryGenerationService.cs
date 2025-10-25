using PgCs.Cli.Configuration;
using PgCs.Common.CodeGeneration;
using PgCs.Common.Services;

namespace PgCs.Cli.Services;

/// <summary>
/// Service for generating C# query repository code
/// </summary>
public sealed class QueryGenerationService
{
    /// <summary>
    /// Generate query result
    /// </summary>
    public record Result(
        int RepositoriesGenerated,
        int MethodsGenerated,
        int ModelsGenerated,
        IReadOnlyList<string> FilesCreated,
        IReadOnlyList<ValidationMessage> Issues
    );

    /// <summary>
    /// Generate C# repository code from SQL queries
    /// </summary>
    public async Task<Result> GenerateAsync(
        PgCsConfiguration config,
        CancellationToken cancellationToken = default)
    {
        if (config.Queries is null)
        {
            throw new InvalidOperationException("Queries configuration is missing");
        }

        var issues = new List<ValidationMessage>();
        var filesCreated = new List<string>();

        // Create and configure pipeline
        var pipeline = CodeGenerationPipeline.Create();

        // Загружаем схему если она указана в конфигурации
        if (config.Schema is not null)
        {
            if (!string.IsNullOrEmpty(config.Schema.Input.File))
            {
                pipeline.FromSchemaFile(config.Schema.Input.File);
            }
            else if (!string.IsNullOrEmpty(config.Schema.Input.Directory))
            {
                var schemaFiles = Directory.GetFiles(
                    config.Schema.Input.Directory,
                    config.Schema.Input.Pattern ?? "*.sql",
                    config.Schema.Input.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                );

                foreach (var file in schemaFiles)
                {
                    pipeline.FromSchemaFile(file);
                }
            }
        }

        // Configure input
        if (!string.IsNullOrEmpty(config.Queries.Input.File))
        {
            pipeline.FromQueryFile(config.Queries.Input.File);
        }
        else if (!string.IsNullOrEmpty(config.Queries.Input.Directory))
        {
            var queryFiles = Directory.GetFiles(
                config.Queries.Input.Directory,
                config.Queries.Input.Pattern,
                config.Queries.Input.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
            );

            foreach (var file in queryFiles)
            {
                pipeline.FromQueryFile(file);
            }
        }

        // Configure query generation options
        pipeline.WithQueryGeneration(builder =>
        {
            builder.WithNamespace(config.Queries.Output.Namespace)
                   .OutputTo(config.Queries.Output.Directory)
                   .UseAsync()
                   .UseRecords()
                   .WithXmlDocs()
                   .OverwriteFiles();
        });

        // Disable schema generation for query-only pipeline
        pipeline.WithoutSchemaGeneration();

        // Execute pipeline
        var pipelineResult = await pipeline.ExecuteAsync(cancellationToken);

        if (!pipelineResult.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Query generation failed: {pipelineResult.Error?.Message}",
                pipelineResult.Error);
        }

        // Extract results
        var stats = pipelineResult.Statistics;
        var writeResult = pipelineResult.QueryWriteResult;

        // Collect validation issues from query analysis
        if (pipelineResult.QueryValidationIssues is not null)
        {
            foreach (var issue in pipelineResult.QueryValidationIssues)
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

        // Handle case where statistics might be null
        if (stats is null)
        {
            throw new InvalidOperationException("Pipeline statistics are not available");
        }

        return new Result(
            RepositoriesGenerated: stats.QueriesAnalyzed > 0 ? 1 : 0,
            MethodsGenerated: stats.QueriesAnalyzed,
            ModelsGenerated: stats.QueryFilesGenerated,
            FilesCreated: filesCreated,
            Issues: issues
        );
    }

    /// <summary>
    /// Validate query input files exist
    /// </summary>
    public static (bool IsValid, string? Error) ValidateInput(QueriesConfiguration queries)
    {
        if (!string.IsNullOrEmpty(queries.Input.File))
        {
            if (!File.Exists(queries.Input.File))
            {
                return (false, $"Query file not found: {queries.Input.File}");
            }
        }
        else if (!string.IsNullOrEmpty(queries.Input.Directory))
        {
            if (!Directory.Exists(queries.Input.Directory))
            {
                return (false, $"Query directory not found: {queries.Input.Directory}");
            }

            var files = Directory.GetFiles(
                queries.Input.Directory,
                queries.Input.Pattern,
                queries.Input.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
            );

            if (files.Length == 0)
            {
                return (false, $"No files matching pattern '{queries.Input.Pattern}' found in: {queries.Input.Directory}");
            }
        }
        else
        {
            return (false, "Query input: either 'file' or 'directory' must be specified");
        }

        return (true, null);
    }
}
