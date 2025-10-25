using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using PgCs.Cli.Output;
using PgCs.Cli.Services;
using PgCs.Common.CodeGeneration;

namespace PgCs.Cli.Commands;

/// <summary>
/// Command to generate both schema and queries (or individually with subcommands)
/// </summary>
public sealed class GenerateCommand : BaseCommand
{
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

    private static readonly Option<bool> SchemaOnlyOption = new(
        aliases: new[] { "--schema-only" },
        description: "Generate only schema (skip queries)",
        getDefaultValue: () => false
    );

    private static readonly Option<bool> QueriesOnlyOption = new(
        aliases: new[] { "--queries-only" },
        description: "Generate only queries (skip schema)",
        getDefaultValue: () => false
    );

    public GenerateCommand() : base("generate", "Generate C# code from PostgreSQL schemas and queries")
    {
        AddOption(DryRunOption);
        AddOption(ForceOption);
        AddOption(SchemaOnlyOption);
        AddOption(QueriesOnlyOption);

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
            var dryRun = context.ParseResult.GetValueForOption(DryRunOption);
            var force = context.ParseResult.GetValueForOption(ForceOption);
            var schemaOnly = context.ParseResult.GetValueForOption(SchemaOnlyOption);
            var queriesOnly = context.ParseResult.GetValueForOption(QueriesOnlyOption);
            var verbose = GetVerbose(context);

            // Validate mutually exclusive options
            if (schemaOnly && queriesOnly)
            {
                Writer.Error("Cannot use --schema-only and --queries-only together");
                ErrorFormatter.DisplayUsageHint("all");
                return 1;
            }

            // Load configuration
            var (config, success) = LoadConfiguration(configPath, context);
            if (!success || config is null)
                return 1;

            // Check what to generate
            var generateSchema = !queriesOnly && config.Schema is not null;
            var generateQueries = !schemaOnly && config.Queries is not null;

            if (!generateSchema && !generateQueries)
            {
                Writer.Error("Nothing to generate - both schema and queries configurations are missing");
                return 1;
            }

            // Show header
            Writer.Heading("Full Code Generation");
            Writer.TableRow("Configuration:", configPath);
            Writer.TableRow("Generate Schema:", generateSchema ? "Yes" : "No");
            Writer.TableRow("Generate Queries:", generateQueries ? "Yes" : "No");
            Writer.TableRow("Dry Run:", dryRun ? "Yes" : "No");
            Writer.WriteLine();

            // Confirm overwrite if needed
            if (!dryRun && !force)
            {
                var needsConfirmation = false;
                
                if (generateSchema && config.Schema is not null && Directory.Exists(config.Schema.Output.Directory))
                {
                    var schemaFiles = Directory.GetFiles(config.Schema.Output.Directory, "*.cs", SearchOption.AllDirectories);
                    if (schemaFiles.Length > 0)
                    {
                        Writer.Warning($"Schema output directory contains {schemaFiles.Length} C# file(s)");
                        needsConfirmation = true;
                    }
                }

                if (generateQueries && config.Queries is not null && Directory.Exists(config.Queries.Output.Directory))
                {
                    var queryFiles = Directory.GetFiles(config.Queries.Output.Directory, "*.cs", SearchOption.AllDirectories);
                    if (queryFiles.Length > 0)
                    {
                        Writer.Warning($"Queries output directory contains {queryFiles.Length} C# file(s)");
                        needsConfirmation = true;
                    }
                }

                if (needsConfirmation && !Writer.Confirm("Do you want to overwrite existing files?", false))
                {
                    Writer.Info("Operation cancelled");
                    return 0;
                }
            }

            // Track totals
            var tablesGenerated = 0;
            var enumsGenerated = 0;
            var repositoriesGenerated = 0;
            var methodsGenerated = 0;
            var modelsGenerated = 0;

            // Generate schema
            if (generateSchema && config.Schema is not null)
            {
                Writer.Step("Generating schema...");
                var progress = new ProgressReporter(Writer);
                progress.Start("Schema generation", 4);

                try
                {
                    progress.Step("Loading schema file(s)");
                    progress.Step("Analyzing database schema");
                    progress.Step("Generating C# classes");
                    progress.Step("Writing output files");

                    var schemaService = new SchemaGenerationService();
                    var schemaResult = await schemaService.GenerateAsync(config, context.GetCancellationToken());

                    tablesGenerated = schemaResult.TablesGenerated;
                    enumsGenerated = schemaResult.EnumsGenerated;

                    if (verbose && schemaResult.FilesCreated.Count > 0)
                    {
                        Writer.Info($"Generated files:");
                        foreach (var file in schemaResult.FilesCreated)
                        {
                            Writer.Info($"  • {file}");
                        }
                    }

                    // Display validation issues if any
                    if (schemaResult.Issues.Count > 0)
                    {
                        Writer.WriteLine();
                        Writer.Info($"Found {schemaResult.Issues.Count} issue(s) during schema analysis:");
                        Writer.WriteLine();
                        
                        foreach (var issue in schemaResult.Issues)
                        {
                            var message = $"[{issue.Code}] {issue.Message}";
                            
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

                    progress.Complete("Schema generation");
                }
                catch (Exception ex)
                {
                    progress.Fail("Schema generation failed");
                    throw new InvalidOperationException($"Schema generation failed: {ex.Message}", ex);
                }
            }

            // Generate queries
            if (generateQueries && config.Queries is not null)
            {
                Writer.Step("Generating queries...");
                var progress = new ProgressReporter(Writer);
                progress.Start("Query generation", 5);

                try
                {
                    progress.Step("Loading query file(s)");
                    progress.Step("Parsing SQL queries");
                    progress.Step("Generating repositories");
                    progress.Step("Generating models");
                    progress.Step("Writing output files");

                    var queryService = new QueryGenerationService();
                    var queryResult = await queryService.GenerateAsync(config, context.GetCancellationToken());

                    repositoriesGenerated = queryResult.RepositoriesGenerated;
                    methodsGenerated = queryResult.MethodsGenerated;
                    modelsGenerated = queryResult.ModelsGenerated;

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
                            var message = $"[{issue.Code}] {issue.Message}";
                            
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
                }
                catch (Exception ex)
                {
                    progress.Fail("Query generation failed");
                    throw new InvalidOperationException($"Query generation failed: {ex.Message}", ex);
                }
            }

            stopwatch.Stop();

            // Print combined results
            var resultPrinter = new ResultPrinter(Writer);
            resultPrinter.PrintCombinedResult(
                tablesGenerated,
                enumsGenerated,
                repositoriesGenerated,
                methodsGenerated,
                modelsGenerated,
                stopwatch.Elapsed
            );

            return 0;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return HandleException(ex, "generate code");
        }
    }
}
