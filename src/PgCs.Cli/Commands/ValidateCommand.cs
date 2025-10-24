using System.CommandLine;
using System.CommandLine.Invocation;
using PgCs.Cli.Configuration;
using PgCs.Cli.Output;

namespace PgCs.Cli.Commands;

/// <summary>
/// Command to validate configuration file
/// </summary>
public sealed class ValidateCommand : BaseCommand
{
    private static readonly Option<bool> StrictOption = new(
        aliases: new[] { "--strict" },
        description: "Enable strict validation mode",
        getDefaultValue: () => false
    );

    public ValidateCommand() : base("validate", "Validate configuration file")
    {
        AddOption(StrictOption);

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
            var configPath = GetConfigPath(context);
            var strict = context.ParseResult.GetValueForOption(StrictOption);

            Writer.Heading("Configuration Validation");
            Writer.TableRow("File:", configPath);
            Writer.TableRow("Mode:", strict ? "Strict" : "Normal");
            Writer.WriteLine();

            // Check if file exists
            var (fileValid, fileError) = ConfigurationLoader.ValidateFile(configPath);
            if (!fileValid)
            {
                ErrorFormatter.DisplayFileError(
                    configPath,
                    fileError!,
                    new[]
                    {
                        "Ensure the file exists and is readable",
                        "Use 'pgcs init' to create a new configuration file"
                    }
                );
                return 1;
            }

            Writer.Success($"Configuration file exists and is readable");
            Writer.WriteLine();

            // Load configuration
            Writer.Step("Loading configuration...");
            var loader = new ConfigurationLoader();
            PgCsConfiguration config;

            try
            {
                config = loader.Load(configPath);
                Writer.Success("Configuration loaded successfully");
            }
            catch (Exception ex)
            {
                Writer.WriteLine();
                ErrorFormatter.DisplayError(ex, "Failed to parse configuration");
                return 1;
            }

            // Validate configuration
            Writer.WriteLine();
            Writer.Step("Validating configuration...");
            var validator = new ConfigurationValidator();
            var isValid = validator.Validate(config);

            // Print results
            var resultPrinter = new ResultPrinter(Writer);
            resultPrinter.PrintValidationResult(isValid, validator.Errors, validator.Warnings);

            // In strict mode, warnings are treated as errors
            if (strict && validator.Warnings.Count > 0)
            {
                Writer.Error("Strict mode: warnings are treated as errors");
                return 1;
            }

            return isValid ? 0 : 1;
        }
        catch (Exception ex)
        {
            return HandleException(ex, "validate configuration");
        }
    }
}
