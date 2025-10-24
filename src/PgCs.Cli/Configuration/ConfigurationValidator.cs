namespace PgCs.Cli.Configuration;

/// <summary>
/// Validates PgCs configuration
/// </summary>
public sealed class ConfigurationValidator
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    /// <summary>
    /// Validation errors
    /// </summary>
    public IReadOnlyList<string> Errors => _errors;

    /// <summary>
    /// Validation warnings
    /// </summary>
    public IReadOnlyList<string> Warnings => _warnings;

    /// <summary>
    /// Validate configuration
    /// </summary>
    public bool Validate(PgCsConfiguration config)
    {
        _errors.Clear();
        _warnings.Clear();

        ValidateSchema(config.Schema);
        ValidateQueries(config.Queries);
        ValidateFormatting(config.Formatting);
        ValidateOutput(config.Output);
        ValidateLogging(config.Logging);

        return _errors.Count == 0;
    }

    private void ValidateSchema(SchemaConfiguration? schema)
    {
        if (schema is null)
        {
            _warnings.Add("Schema configuration is not provided - schema generation will be skipped");
            return;
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(schema.Input.File) && string.IsNullOrWhiteSpace(schema.Input.Directory))
        {
            _errors.Add("Schema input: either 'file' or 'directory' must be specified");
        }

        if (!string.IsNullOrWhiteSpace(schema.Input.File) && !string.IsNullOrWhiteSpace(schema.Input.Directory))
        {
            _errors.Add("Schema input: cannot specify both 'file' and 'directory'");
        }

        // Validate output
        if (string.IsNullOrWhiteSpace(schema.Output.Directory))
        {
            _errors.Add("Schema output: 'directory' is required");
        }

        if (string.IsNullOrWhiteSpace(schema.Output.Namespace))
        {
            _errors.Add("Schema output: 'namespace' is required");
        }

        // Validate naming convention
        ValidateNamingConvention(schema.Naming.Convention, "Schema naming");

        // Validate generation options
        if (!schema.Generation.GenerateClasses && !schema.Generation.GenerateRecords)
        {
            _warnings.Add("Schema generation: both 'generateClasses' and 'generateRecords' are disabled - no classes will be generated");
        }
    }

    private void ValidateQueries(QueriesConfiguration? queries)
    {
        if (queries is null)
        {
            _warnings.Add("Queries configuration is not provided - query generation will be skipped");
            return;
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(queries.Input.File) && string.IsNullOrWhiteSpace(queries.Input.Directory))
        {
            _errors.Add("Queries input: either 'file' or 'directory' must be specified");
        }

        if (!string.IsNullOrWhiteSpace(queries.Input.File) && !string.IsNullOrWhiteSpace(queries.Input.Directory))
        {
            _errors.Add("Queries input: cannot specify both 'file' and 'directory'");
        }

        // Validate output
        if (string.IsNullOrWhiteSpace(queries.Output.Directory))
        {
            _errors.Add("Queries output: 'directory' is required");
        }

        if (string.IsNullOrWhiteSpace(queries.Output.Namespace))
        {
            _errors.Add("Queries output: 'namespace' is required");
        }

        // Validate naming convention
        ValidateNamingConvention(queries.Methods.NamingConvention, "Queries method naming");

        // Validate repository options
        if (!queries.Repositories.GenerateInterfaces && !queries.Repositories.GenerateImplementations)
        {
            _warnings.Add("Queries repositories: both 'generateInterfaces' and 'generateImplementations' are disabled - no repositories will be generated");
        }

        // Validate model options
        if (!queries.Models.GenerateRecords && !queries.Models.GenerateClasses)
        {
            _warnings.Add("Queries models: both 'generateRecords' and 'generateClasses' are disabled - no models will be generated");
        }
    }

    private void ValidateFormatting(FormattingConfiguration formatting)
    {
        // Validate indentation
        if (formatting.Indentation != "spaces" && formatting.Indentation != "tabs")
        {
            _errors.Add($"Formatting: invalid indentation type '{formatting.Indentation}' (must be 'spaces' or 'tabs')");
        }

        if (formatting.IndentSize < 1 || formatting.IndentSize > 8)
        {
            _errors.Add($"Formatting: invalid indent size {formatting.IndentSize} (must be between 1 and 8)");
        }

        // Validate line endings
        if (formatting.LineEndings != "lf" && formatting.LineEndings != "crlf")
        {
            _errors.Add($"Formatting: invalid line endings '{formatting.LineEndings}' (must be 'lf' or 'crlf')");
        }
    }

    private void ValidateOutput(OutputConfiguration output)
    {
        if (output.CreateBackups && string.IsNullOrWhiteSpace(output.BackupDirectory))
        {
            _errors.Add("Output: 'backupDirectory' is required when 'createBackups' is enabled");
        }

        if (output.DryRun && output.CreateBackups)
        {
            _warnings.Add("Output: 'createBackups' has no effect when 'dryRun' is enabled");
        }
    }

    private void ValidateLogging(LoggingConfiguration logging)
    {
        var validLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None" };
        if (!validLevels.Contains(logging.Level))
        {
            _errors.Add($"Logging: invalid level '{logging.Level}' (must be one of: {string.Join(", ", validLevels)})");
        }

        if (!logging.Console && string.IsNullOrWhiteSpace(logging.File))
        {
            _warnings.Add("Logging: both console and file logging are disabled - no logs will be produced");
        }
    }

    private void ValidateNamingConvention(string convention, string context)
    {
        var validConventions = new[] { "PascalCase", "camelCase", "snake_case", "kebab-case" };
        if (!validConventions.Contains(convention))
        {
            _errors.Add($"{context}: invalid naming convention '{convention}' (must be one of: {string.Join(", ", validConventions)})");
        }
    }

    /// <summary>
    /// Get formatted validation result
    /// </summary>
    public string GetFormattedResult()
    {
        var result = new List<string>();

        if (_errors.Count > 0)
        {
            result.Add("❌ Configuration Errors:");
            foreach (var error in _errors)
            {
                result.Add($"  • {error}");
            }
        }

        if (_warnings.Count > 0)
        {
            if (result.Count > 0) result.Add("");
            result.Add("⚠️  Configuration Warnings:");
            foreach (var warning in _warnings)
            {
                result.Add($"  • {warning}");
            }
        }

        if (_errors.Count == 0 && _warnings.Count == 0)
        {
            result.Add("✓ Configuration is valid");
        }

        return string.Join(Environment.NewLine, result);
    }
}
