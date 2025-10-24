namespace PgCs.Cli.Output;

/// <summary>
/// Prints generation results and statistics
/// </summary>
public sealed class ResultPrinter
{
    private readonly ConsoleWriter _writer;

    public ResultPrinter(ConsoleWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Print schema generation result
    /// </summary>
    public void PrintSchemaResult(int tablesGenerated, int enumsGenerated, string outputDirectory, TimeSpan elapsed)
    {
        _writer.WriteLine();
        _writer.Heading("Schema Generation Results");

        _writer.TableRow("Tables Generated:", tablesGenerated.ToString());
        _writer.TableRow("Enums Generated:", enumsGenerated.ToString());
        _writer.TableRow("Output Directory:", outputDirectory);
        _writer.TableRow("Time Elapsed:", FormatElapsedTime(elapsed));

        _writer.WriteLine();
        _writer.Success($"Successfully generated {tablesGenerated} table(s) and {enumsGenerated} enum(s)");
        _writer.WriteLine();
    }

    /// <summary>
    /// Print query generation result
    /// </summary>
    public void PrintQueryResult(int repositoriesGenerated, int methodsGenerated, int modelsGenerated, string outputDirectory, TimeSpan elapsed)
    {
        _writer.WriteLine();
        _writer.Heading("Query Generation Results");

        _writer.TableRow("Repositories Generated:", repositoriesGenerated.ToString());
        _writer.TableRow("Methods Generated:", methodsGenerated.ToString());
        _writer.TableRow("Models Generated:", modelsGenerated.ToString());
        _writer.TableRow("Output Directory:", outputDirectory);
        _writer.TableRow("Time Elapsed:", FormatElapsedTime(elapsed));

        _writer.WriteLine();
        var repoWord = repositoriesGenerated == 1 ? "repository" : "repositories";
        var methodWord = methodsGenerated == 1 ? "method" : "methods";
        var modelWord = modelsGenerated == 1 ? "model" : "models";
        _writer.Success($"Successfully generated {repositoriesGenerated} {repoWord}, {methodsGenerated} {methodWord}, and {modelsGenerated} {modelWord}");
        _writer.WriteLine();
    }

    /// <summary>
    /// Print combined generation result
    /// </summary>
    public void PrintCombinedResult(
        int tablesGenerated, int enumsGenerated,
        int repositoriesGenerated, int methodsGenerated, int modelsGenerated,
        TimeSpan elapsed)
    {
        _writer.WriteLine();
        _writer.Heading("Code Generation Results");

        _writer.WriteLine("Schema:");
        _writer.TableRow("  Tables:", tablesGenerated.ToString());
        _writer.TableRow("  Enums:", enumsGenerated.ToString());

        _writer.WriteLine();
        _writer.WriteLine("Queries:");
        _writer.TableRow("  Repositories:", repositoriesGenerated.ToString());
        _writer.TableRow("  Methods:", methodsGenerated.ToString());
        _writer.TableRow("  Models:", modelsGenerated.ToString());

        _writer.WriteLine();
        _writer.TableRow("Total Time:", FormatElapsedTime(elapsed));

        _writer.WriteLine();
        _writer.Success("Code generation completed successfully!");
        _writer.WriteLine();
    }

    /// <summary>
    /// Print validation result
    /// </summary>
    public void PrintValidationResult(bool isValid, IEnumerable<string> errors, IEnumerable<string> warnings)
    {
        _writer.WriteLine();
        _writer.Heading("Configuration Validation");

        if (isValid)
        {
            _writer.Success("Configuration is valid");
        }
        else
        {
            _writer.Error($"Configuration has {errors.Count()} error(s)");
        }

        if (errors.Any())
        {
            _writer.WriteLine();
            _writer.WriteLine("Errors:");
            foreach (var error in errors)
            {
                _writer.WriteLine($"  ✗ {error}");
            }
        }

        if (warnings.Any())
        {
            _writer.WriteLine();
            _writer.WriteLine("Warnings:");
            foreach (var warning in warnings)
            {
                _writer.Warning($"  {warning}");
            }
        }

        _writer.WriteLine();
    }

    /// <summary>
    /// Print dry run result
    /// </summary>
    public void PrintDryRunResult(int filesWouldBeCreated, int filesWouldBeOverwritten, string outputDirectory)
    {
        _writer.WriteLine();
        _writer.Heading("Dry Run Results");

        _writer.Info("No files were written (dry run mode)");
        _writer.WriteLine();

        _writer.TableRow("Files to create:", filesWouldBeCreated.ToString());
        _writer.TableRow("Files to overwrite:", filesWouldBeOverwritten.ToString());
        _writer.TableRow("Output directory:", outputDirectory);

        _writer.WriteLine();
        _writer.Dim("Run without --dry-run to write files");
        _writer.WriteLine();
    }

    /// <summary>
    /// Print file list
    /// </summary>
    public void PrintFileList(IEnumerable<string> files, string title)
    {
        _writer.WriteLine();
        _writer.WriteLine($"{title}:");
        
        foreach (var file in files)
        {
            _writer.Dim($"  • {file}");
        }
        
        _writer.WriteLine();
    }

    /// <summary>
    /// Format elapsed time
    /// </summary>
    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds < 1)
            return $"{elapsed.TotalMilliseconds:F0}ms";
        
        if (elapsed.TotalMinutes < 1)
            return $"{elapsed.TotalSeconds:F2}s";
        
        return $"{elapsed.Minutes}m {elapsed.Seconds}s";
    }
}
