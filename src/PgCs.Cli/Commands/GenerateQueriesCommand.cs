using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using PgCs.Cli.Output;
using PgCs.Cli.Services;
using PgCs.Common.CodeGeneration;

namespace PgCs.Cli.Commands;

/// <summary>
/// Command to generate C# repository code from SQL queries
/// </summary>
public sealed class GenerateQueriesCommand : BaseCommand
{
    private static readonly Option<string?> InputOption = new(
        aliases: new[] { "--input", "-i" },
        description: "Query SQL file or directory (overrides config)"
    );

    private static readonly Option<string?> OutputOption = new(
        aliases: new[] { "--output", "-o" },
        description: "Output directory (overrides config)"
    );

    private static readonly Option<bool> DryRunOption = new(
        aliases: new[] { "--dry-run" },
        description: "Show what would be generated without writing files",
        getDefaultValue: () => false
    );

    private static readonly Option<bool> ForceOption = new(
        aliases: new[] { "--force", "-f" },
        description: "Overwrite existing files without confirmation",
        getDefaultValue: () => false
    );

    public GenerateQueriesCommand() : base("queries", "Generate C# repository code from SQL queries")
    {
        AddOption(InputOption);
        AddOption(OutputOption);
        AddOption(DryRunOption);
        AddOption(ForceOption);

        this.SetHandler(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(InvocationContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Initialize writer with color settings
            InitializeWriter(context);

            // Get options
            var configPath = GetConfigPath(context);
            var inputOverride = context.ParseResult.GetValueForOption(InputOption);
            var outputOverride = context.ParseResult.GetValueForOption(OutputOption);
            var dryRun = context.ParseResult.GetValueForOption(DryRunOption);
            var force = context.ParseResult.GetValueForOption(ForceOption);
            var verbose = GetVerbose(context);

            // Load configuration
            var (config, success) = LoadConfiguration(configPath, context);
            if (!success || config is null)
                return 1;

            // Check if queries config exists
            if (config.Queries is null)
            {
                Writer.Error("Queries configuration is missing in config file");
                ErrorFormatter.DisplayUsageHint("queries");
                return 1;
            }

            // Apply overrides
            if (inputOverride is not null)
            {
                if (File.Exists(inputOverride))
                {
                    config.Queries.Input.File = inputOverride;
                    config.Queries.Input.Directory = null;
                }
                else if (Directory.Exists(inputOverride))
                {
                    config.Queries.Input.Directory = inputOverride;
                    config.Queries.Input.File = null;
                }
                else
                {
                    Writer.Error($"Input path not found: {inputOverride}");
                    return 1;
                }
            }

            if (outputOverride is not null)
            {
                config.Queries.Output.Directory = outputOverride;
            }

            // Show configuration
            Writer.Heading("Query Generation");
            Writer.TableRow("Input:", config.Queries.Input.File ?? config.Queries.Input.Directory!);
            Writer.TableRow("Output:", config.Queries.Output.Directory);
            Writer.TableRow("Namespace:", config.Queries.Output.Namespace);
            Writer.TableRow("Dry Run:", dryRun ? "Yes" : "No");
            Writer.WriteLine();

            // Confirm overwrite if needed
            if (!dryRun && !force && Directory.Exists(config.Queries.Output.Directory))
            {
                var files = Directory.GetFiles(config.Queries.Output.Directory, "*.cs", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    Writer.Warning($"Output directory contains {files.Length} C# file(s)");
                    if (!Writer.Confirm("Do you want to overwrite existing files?", false))
                    {
                        Writer.Info("Operation cancelled");
                        return 0;
                    }
                }
            }

            // Create progress reporter
            var progress = new ProgressReporter(Writer);
            progress.Start("Query generation", 5);

            try
            {
                progress.Step("Loading query file(s)");
                progress.Step("Parsing SQL queries");
                progress.Step("Generating repositories");
                progress.Step("Generating models");
                progress.Step("Writing output files");

                // Use real query generation service
                var queryService = new QueryGenerationService();
                var queryResult = await queryService.GenerateAsync(config, context.GetCancellationToken());

                stopwatch.Stop();

                if (verbose && queryResult.FilesCreated.Count > 0)
                {
                    Writer.Info($"Generated files:");
                    foreach (var file in queryResult.FilesCreated)
                    {
                        Writer.Info($"  • {file}");
                    }
                }

                // Display validation issues if any
                if (queryResult.Issues.Count > 0)
                {
                    Writer.WriteLine();
                    Writer.Info($"Found {queryResult.Issues.Count} issue(s) during query analysis:");
                    Writer.WriteLine();
                    
                    foreach (var issue in queryResult.Issues)
                    {
                        // Format message
                        var message = $"[{issue.Code}] {issue.Message}";
                        
                        // Display based on severity
                        if (issue.Severity == ValidationSeverity.Error)
                        {
                            Writer.Error($"ERROR: {message}");
                        }
                        else if (issue.Severity == ValidationSeverity.Warning)
                        {
                            Writer.Warning($"{message}");
                        }
                        else
                        {
                            Writer.Info($"{message}");
                        }
                        
                        // Display location if available
                        if (!string.IsNullOrEmpty(issue.Location))
                        {
                            var locationPreview = issue.Location.Length > 100 
                                ? issue.Location.Substring(0, 100) + "..." 
                                : issue.Location;
                            Writer.Info($"  → {locationPreview}");
                        }
                    }
                    Writer.WriteLine();
                }

                progress.Complete("Query generation");

                // Print results
                var resultPrinter = new ResultPrinter(Writer);
                resultPrinter.PrintQueryResult(
                    repositoriesGenerated: queryResult.RepositoriesGenerated,
                    methodsGenerated: queryResult.MethodsGenerated,
                    modelsGenerated: queryResult.ModelsGenerated,
                    outputDirectory: config.Queries.Output.Directory,
                    elapsed: stopwatch.Elapsed
                );
            }
            catch (Exception ex)
            {
                progress.Fail("Query generation failed");
                throw new InvalidOperationException($"Query generation failed: {ex.Message}", ex);
            }

            return 0;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return HandleException(ex, "generate queries");
        }
    }
}
