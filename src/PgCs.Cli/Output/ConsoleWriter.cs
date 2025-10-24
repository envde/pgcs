namespace PgCs.Cli.Output;

/// <summary>
/// Console output with color support
/// </summary>
public sealed class ConsoleWriter
{
    private bool _enableColors;

    public ConsoleWriter(bool enableColors = true)
    {
        _enableColors = enableColors && !Console.IsOutputRedirected;
    }

    /// <summary>
    /// Set whether colors are enabled
    /// </summary>
    public void SetColorEnabled(bool enabled)
    {
        _enableColors = enabled && !Console.IsOutputRedirected;
    }

    /// <summary>
    /// Write success message
    /// </summary>
    public void Success(string message)
    {
        WriteColored("✓ ", ConsoleColor.Green, message);
    }

    /// <summary>
    /// Write error message
    /// </summary>
    public void Error(string message)
    {
        WriteColored("✗ ", ConsoleColor.Red, message);
    }

    /// <summary>
    /// Write warning message
    /// </summary>
    public void Warning(string message)
    {
        WriteColored("⚠ ", ConsoleColor.Yellow, message);
    }

    /// <summary>
    /// Write info message
    /// </summary>
    public void Info(string message)
    {
        WriteColored("ℹ ", ConsoleColor.Cyan, message);
    }

    /// <summary>
    /// Write step message
    /// </summary>
    public void Step(string message)
    {
        WriteColored("→ ", ConsoleColor.Blue, message);
    }

    /// <summary>
    /// Write dimmed text
    /// </summary>
    public void Dim(string message)
    {
        if (_enableColors)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Write bold text
    /// </summary>
    public void Bold(string message)
    {
        Console.WriteLine(message);
    }

    /// <summary>
    /// Write plain text
    /// </summary>
    public void WriteLine(string message = "")
    {
        Console.WriteLine(message);
    }

    /// <summary>
    /// Write heading
    /// </summary>
    public void Heading(string message)
    {
        Console.WriteLine();
        if (_enableColors)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"═══ {message} ═══");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine($"=== {message} ===");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Write separator
    /// </summary>
    public void Separator()
    {
        if (_enableColors)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('─', 60));
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(new string('-', 60));
        }
    }

    /// <summary>
    /// Write table row
    /// </summary>
    public void TableRow(string key, string value, int keyWidth = 25)
    {
        if (_enableColors)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(key.PadRight(keyWidth));
            Console.ResetColor();
            Console.WriteLine(value);
        }
        else
        {
            Console.WriteLine($"{key.PadRight(keyWidth)}{value}");
        }
    }

    /// <summary>
    /// Ask yes/no question
    /// </summary>
    public bool Confirm(string question, bool defaultValue = false)
    {
        var defaultText = defaultValue ? "Y/n" : "y/N";
        WriteColored("? ", ConsoleColor.Yellow, $"{question} ({defaultText}): ");
        
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        
        if (string.IsNullOrEmpty(response))
            return defaultValue;
        
        return response == "y" || response == "yes";
    }

    /// <summary>
    /// Write colored text
    /// </summary>
    private void WriteColored(string prefix, ConsoleColor color, string message)
    {
        if (_enableColors)
        {
            Console.ForegroundColor = color;
            Console.Write(prefix);
            Console.ResetColor();
            Console.WriteLine(message);
        }
        else
        {
            Console.WriteLine($"{prefix}{message}");
        }
    }
}
