using System.CommandLine;
using System.CommandLine.Invocation;
using PgCs.Cli.Configuration;

namespace PgCs.Cli.Commands;

/// <summary>
/// Command to initialize a new configuration file
/// </summary>
public sealed class InitCommand : BaseCommand
{
    private static readonly Option<string> OutputPathOption = new(
        aliases: new[] { "--output", "-o" },
        description: "Output configuration file path",
        getDefaultValue: () => "config.yml"
    );

    private static readonly Option<bool> MinimalOption = new(
        aliases: new[] { "--minimal", "-m" },
        description: "Create minimal configuration",
        getDefaultValue: () => false
    );

    private static readonly Option<bool> ForceOption = new(
        aliases: new[] { "--force", "-f" },
        description: "Overwrite existing file without confirmation",
        getDefaultValue: () => false
    );

    private static readonly Option<string?> SchemaInputOption = new(
        "--schema-input",
        description: "Path to schema SQL file"
    );

    private static readonly Option<string?> SchemaOutputOption = new(
        "--schema-output",
        description: "Output directory for schema classes"
    );

    private static readonly Option<string?> QueriesInputOption = new(
        "--queries-input",
        description: "Path to queries SQL file"
    );

    private static readonly Option<string?> QueriesOutputOption = new(
        "--queries-output",
        description: "Output directory for query repositories"
    );

    public InitCommand() : base("init", "Initialize a new configuration file")
    {
        AddOption(OutputPathOption);
        AddOption(MinimalOption);
        AddOption(ForceOption);
        AddOption(SchemaInputOption);
        AddOption(SchemaOutputOption);
        AddOption(QueriesInputOption);
        AddOption(QueriesOutputOption);

        this.SetHandler((InvocationContext context) =>
        {
            context.ExitCode = ExecuteHandler(context);
        });
    }

    private int ExecuteHandler(InvocationContext context)
    {
        try
        {
            // Initialize writer with color settings
            InitializeWriter(context);

            // Get options
            var outputPath = context.ParseResult.GetValueForOption(OutputPathOption)!;
            var minimal = context.ParseResult.GetValueForOption(MinimalOption);
            var force = context.ParseResult.GetValueForOption(ForceOption);
            var schemaInput = context.ParseResult.GetValueForOption(SchemaInputOption);
            var schemaOutput = context.ParseResult.GetValueForOption(SchemaOutputOption);
            var queriesInput = context.ParseResult.GetValueForOption(QueriesInputOption);
            var queriesOutput = context.ParseResult.GetValueForOption(QueriesOutputOption);

            Writer.Heading("Initialize Configuration");

            // Check if file exists
            if (File.Exists(outputPath) && !force)
            {
                Writer.Warning($"Configuration file already exists: {outputPath}");
                if (!Writer.Confirm("Do you want to overwrite it?", false))
                {
                    Writer.Info("Operation cancelled");
                    return 0;
                }
            }

            // Determine what to create
            string content;

            if (minimal)
            {
                // Create minimal configuration
                schemaInput ??= "./schema.sql";
                schemaOutput ??= "./Generated/Schema";
                queriesInput ??= "./queries.sql";
                queriesOutput ??= "./Generated/Queries";

                content = ConfigurationLoader.CreateMinimalConfig(
                    schemaInput,
                    schemaOutput,
                    queriesInput,
                    queriesOutput
                );

                Writer.Info("Creating minimal configuration...");
            }
            else
            {
                // Copy full configuration from embedded resource or template
                // For now, we'll use the example from the Examples folder
                var examplePath = Path.Combine(AppContext.BaseDirectory, "Examples", "Configurations", "full_config.yml");
                
                if (File.Exists(examplePath))
                {
                    content = File.ReadAllText(examplePath);
                    Writer.Info("Creating full configuration from template...");
                }
                else
                {
                    // Fallback to minimal if template not found
                    content = ConfigurationLoader.CreateMinimalConfig(
                        "./schema.sql",
                        "./Generated/Schema",
                        "./queries.sql",
                        "./Generated/Queries"
                    );
                    Writer.Warning("Full template not found, creating minimal configuration...");
                }
            }

            // Write configuration file
            File.WriteAllText(outputPath, content);

            Writer.WriteLine();
            Writer.Success($"Configuration file created: {outputPath}");
            Writer.WriteLine();

            // Show next steps
            Writer.Info("Next steps:");
            Writer.Dim("  1. Edit the configuration file to match your project");
            Writer.Dim("  2. Validate with: pgcs validate");
            Writer.Dim("  3. Generate code with: pgcs generate");
            Writer.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            return HandleException(ex, "initialize configuration");
        }
    }
}
