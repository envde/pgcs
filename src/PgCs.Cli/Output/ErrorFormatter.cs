namespace PgCs.Cli.Output;

/// <summary>
/// Formats error messages for user-friendly display
/// </summary>
public sealed class ErrorFormatter
{
    private readonly ConsoleWriter _writer;

    public ErrorFormatter(ConsoleWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Display formatted error
    /// </summary>
    public void DisplayError(Exception exception, string? context = null)
    {
        _writer.WriteLine();
        _writer.Error(context ?? "An error occurred");
        _writer.WriteLine();

        // Display exception type and message
        _writer.Dim($"  Error Type: {exception.GetType().Name}");
        _writer.WriteLine($"  Message: {exception.Message}");

        // Display file context if available
        if (exception is FileNotFoundException fileEx)
        {
            _writer.WriteLine();
            _writer.Warning("File not found - please check the path:");
            _writer.Dim($"  {fileEx.FileName}");
        }
        else if (exception is IOException ioEx)
        {
            _writer.WriteLine();
            _writer.Warning("IO Error - please check file permissions and paths");
        }
        else if (exception is InvalidOperationException)
        {
            _writer.WriteLine();
            _writer.Warning("Invalid operation - please check your configuration");
        }

        // Display inner exception if exists
        if (exception.InnerException is not null)
        {
            _writer.WriteLine();
            _writer.Dim("  Inner Exception:");
            _writer.Dim($"    {exception.InnerException.Message}");
            
            // В debug mode показываем stack trace inner exception
            if (Environment.GetEnvironmentVariable("PGCS_DEBUG") == "1" && exception.InnerException.StackTrace is not null)
            {
                _writer.Dim("    Stack Trace:");
                _writer.Dim($"{exception.InnerException.StackTrace}");
            }
        }

        // Display stack trace in debug mode
        if (Environment.GetEnvironmentVariable("PGCS_DEBUG") == "1")
        {
            _writer.WriteLine();
            _writer.Dim("  Stack Trace:");
            _writer.Dim($"{exception.StackTrace}");
        }
        else
        {
            _writer.WriteLine();
            _writer.Dim("  Tip: Set PGCS_DEBUG=1 environment variable for detailed stack trace");
        }

        _writer.WriteLine();
    }

    /// <summary>
    /// Display validation errors
    /// </summary>
    public void DisplayValidationErrors(IEnumerable<string> errors, IEnumerable<string>? warnings = null)
    {
        _writer.WriteLine();
        _writer.Error("Configuration validation failed");
        _writer.WriteLine();

        foreach (var error in errors)
        {
            _writer.WriteLine($"  ✗ {error}");
        }

        if (warnings?.Any() == true)
        {
            _writer.WriteLine();
            foreach (var warning in warnings)
            {
                _writer.Warning($"  {warning}");
            }
        }

        _writer.WriteLine();
    }

    /// <summary>
    /// Display usage hint
    /// </summary>
    public void DisplayUsageHint(string command)
    {
        _writer.WriteLine();
        _writer.Info($"Run 'pgcs {command} --help' for more information");
        _writer.WriteLine();
    }

    /// <summary>
    /// Display file error with suggestions
    /// </summary>
    public void DisplayFileError(string filePath, string error, IEnumerable<string>? suggestions = null)
    {
        _writer.WriteLine();
        _writer.Error($"File error: {error}");
        _writer.Dim($"  Path: {filePath}");

        if (suggestions?.Any() == true)
        {
            _writer.WriteLine();
            _writer.Info("Suggestions:");
            foreach (var suggestion in suggestions)
            {
                _writer.Dim($"  • {suggestion}");
            }
        }

        _writer.WriteLine();
    }
}
