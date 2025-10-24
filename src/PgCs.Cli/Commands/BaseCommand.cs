using System.CommandLine;
using System.CommandLine.Invocation;
using PgCs.Cli.Configuration;
using PgCs.Cli.Output;

namespace PgCs.Cli.Commands;

/// <summary>
/// Base class for all commands with common functionality
/// </summary>
public abstract class BaseCommand : Command
{
    protected readonly ConsoleWriter Writer;
    protected readonly ErrorFormatter ErrorFormatter;

    // Common options as static properties for reuse
    protected static readonly Option<string> ConfigOption = new(
        aliases: new[] { "--config", "-c" },
        description: "Path to configuration file",
        getDefaultValue: () => "config.yml"
    );

    protected static readonly Option<bool> VerboseOption = new(
        aliases: new[] { "--verbose", "-v" },
        description: "Enable verbose output",
        getDefaultValue: () => false
    );

    protected static readonly Option<bool> NoColorOption = new(
        aliases: new[] { "--no-color" },
        description: "Disable colored output",
        getDefaultValue: () => false
    );

    protected BaseCommand(string name, string? description = null) : base(name, description)
    {
        // Note: Writer will be initialized in command execution with actual --no-color value
        Writer = new ConsoleWriter();
        ErrorFormatter = new ErrorFormatter(Writer);

        // Add common options
        AddOption(ConfigOption);
        AddOption(VerboseOption);
        AddOption(NoColorOption);
    }

    /// <summary>
    /// Initialize writer with context (respecting --no-color option)
    /// </summary>
    protected void InitializeWriter(InvocationContext context)
    {
        var noColor = context.ParseResult.GetValueForOption(NoColorOption);
        Writer.SetColorEnabled(!noColor);
    }

    /// <summary>
    /// Load and validate configuration
    /// </summary>
    protected (PgCsConfiguration? Config, bool Success) LoadConfiguration(string configPath, InvocationContext context)
    {
        try
        {
            // Check if file exists
            var (isValid, error) = ConfigurationLoader.ValidateFile(configPath);
            if (!isValid)
            {
                ErrorFormatter.DisplayFileError(
                    configPath,
                    error!,
                    new[]
                    {
                        "Ensure the file exists and is readable",
                        "Use 'pgcs init' to create a new configuration file",
                        "Specify a different file with --config"
                    }
                );
                return (null, false);
            }

            // Load configuration
            var loader = new ConfigurationLoader();
            var config = loader.Load(configPath);

            // Validate configuration
            var validator = new ConfigurationValidator();
            if (!validator.Validate(config))
            {
                ErrorFormatter.DisplayValidationErrors(validator.Errors, validator.Warnings);
                ErrorFormatter.DisplayUsageHint("validate");
                return (null, false);
            }

            // Show warnings if any
            if (validator.Warnings.Count > 0)
            {
                foreach (var warning in validator.Warnings)
                {
                    Writer.Warning(warning);
                }
                Writer.WriteLine();
            }

            return (config, true);
        }
        catch (Exception ex)
        {
            ErrorFormatter.DisplayError(ex, "Failed to load configuration");
            return (null, false);
        }
    }

    /// <summary>
    /// Handle common exceptions
    /// </summary>
    protected int HandleException(Exception ex, string operation)
    {
        ErrorFormatter.DisplayError(ex, $"Failed to {operation}");
        return 1;
    }

    /// <summary>
    /// Check if directory exists, create if requested
    /// </summary>
    protected bool EnsureDirectory(string path, bool create = true)
    {
        if (Directory.Exists(path))
            return true;

        if (!create)
            return false;

        try
        {
            Directory.CreateDirectory(path);
            Writer.Dim($"Created directory: {path}");
            return true;
        }
        catch (Exception ex)
        {
            Writer.Error($"Failed to create directory: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get verbose flag from context
    /// </summary>
    protected static bool GetVerbose(InvocationContext context)
    {
        return context.ParseResult.GetValueForOption(VerboseOption);
    }

    /// <summary>
    /// Get config path from context
    /// </summary>
    protected static string GetConfigPath(InvocationContext context)
    {
        return context.ParseResult.GetValueForOption(ConfigOption) ?? "config.yml";
    }
}
