using PgCs.Cli.Configuration;

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
        IReadOnlyList<string> Warnings
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

        var warnings = new List<string>();
        var filesCreated = new List<string>();

        // TODO: Implement actual integration with CodeGenerationPipeline
        // This is a placeholder implementation

        // Simulate work
        await Task.Delay(100, cancellationToken);

        // Example: Use CodeGenerationPipeline
        /*
        var pipeline = new CodeGenerationPipeline();
        
        if (!string.IsNullOrEmpty(config.Queries.Input.File))
        {
            pipeline.FromQueryFile(config.Queries.Input.File);
        }
        else if (!string.IsNullOrEmpty(config.Queries.Input.Directory))
        {
            pipeline.FromQueryDirectory(
                config.Queries.Input.Directory,
                config.Queries.Input.Pattern,
                config.Queries.Input.Recursive
            );
        }

        pipeline.WithQueryGeneration(options =>
        {
            options.OutputDirectory = config.Queries.Output.Directory;
            options.Namespace = config.Queries.Output.Namespace;
            options.RepositoryPerFile = config.Queries.Output.RepositoryPerFile;
            options.SeparateModels = config.Queries.Output.SeparateModels;
            
            options.GenerateInterfaces = config.Queries.Repositories.GenerateInterfaces;
            options.GenerateImplementations = config.Queries.Repositories.GenerateImplementations;
            
            options.AsyncMethods = config.Queries.Methods.Async;
            options.CancellationToken = config.Queries.Methods.CancellationToken;
            
            // ... more options
        });

        var result = await pipeline.ExecuteAsync(cancellationToken);
        */

        // Return placeholder result
        return new Result(
            RepositoriesGenerated: 3,
            MethodsGenerated: 12,
            ModelsGenerated: 8,
            FilesCreated: filesCreated,
            Warnings: warnings
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
