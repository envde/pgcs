using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using PgCs.Cli.Output;
using PgCs.Cli.Services;
using PgCs.Common.CodeGeneration;

namespace PgCs.Cli.Commands;

/// <summary>
/// Command to generate C# code from PostgreSQL schemas
/// </summary>
public sealed class GenerateSchemaCommand : BaseCommand
{
    private static readonly Option<string?> InputOption = new(
        aliases: new[] { "--input", "-i" },
        description: "Schema SQL file or directory (overrides config)"
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

    public GenerateSchemaCommand() : base("schema", "Generate C# classes from PostgreSQL schema")
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

            // Check if schema config exists
            if (config.Schema is null)
            {
                Writer.Error("Schema configuration is missing in config file");
                ErrorFormatter.DisplayUsageHint("schema");
                return 1;
            }

            // Apply overrides
            if (inputOverride is not null)
            {
                if (File.Exists(inputOverride))
                {
                    config.Schema.Input.File = inputOverride;
                    config.Schema.Input.Directory = null;
                }
                else if (Directory.Exists(inputOverride))
                {
                    config.Schema.Input.Directory = inputOverride;
                    config.Schema.Input.File = null;
                }
                else
                {
                    Writer.Error($"Input path not found: {inputOverride}");
                    return 1;
                }
            }

            if (outputOverride is not null)
            {
                config.Schema.Output.Directory = outputOverride;
            }

            // Show configuration
            Writer.Heading("Schema Generation");
            Writer.TableRow("Input:", config.Schema.Input.File ?? config.Schema.Input.Directory!);
            Writer.TableRow("Output:", config.Schema.Output.Directory);
            Writer.TableRow("Namespace:", config.Schema.Output.Namespace);
            Writer.TableRow("Dry Run:", dryRun ? "Yes" : "No");
            Writer.WriteLine();

            // Confirm overwrite if needed
            if (!dryRun && !force && Directory.Exists(config.Schema.Output.Directory))
            {
                var files = Directory.GetFiles(config.Schema.Output.Directory, "*.cs", SearchOption.AllDirectories);
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
            progress.Start("Schema generation", 4);

            try
            {
                progress.Step("Loading schema file(s)");
                progress.Step("Analyzing database schema");
                progress.Step("Generating C# classes");
                progress.Step("Writing output files");

                // Use real schema generation service
                var schemaService = new SchemaGenerationService();
                var schemaResult = await schemaService.GenerateAsync(config, context.GetCancellationToken());

                stopwatch.Stop();

                if (verbose && schemaResult.FilesCreated.Count > 0)
                {
                    Writer.Info($"Generated files:");
                    foreach (var file in schemaResult.FilesCreated)
                    {
                        Writer.Info($"  • {file}");
                    }
                }

                // Display validation issues if any
                ValidationIssueDisplayHelper.DisplayValidationIssues(
                    Writer, 
                    schemaResult.Issues, 
                    "schema analysis");

                progress.Complete("Schema generation");

                // Print results
                var resultPrinter = new ResultPrinter(Writer);
                resultPrinter.PrintSchemaResult(
                    tablesGenerated: schemaResult.TablesGenerated,
                    enumsGenerated: schemaResult.EnumsGenerated,
                    outputDirectory: config.Schema.Output.Directory,
                    elapsed: stopwatch.Elapsed
                );
            }
            catch (Exception ex)
            {
                progress.Fail("Schema generation failed");
                throw new InvalidOperationException($"Schema generation failed: {ex.Message}", ex);
            }

            return 0;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return HandleException(ex, "generate schema");
        }
    }
}
