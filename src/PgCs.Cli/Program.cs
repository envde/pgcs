using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using PgCs.Cli.Commands;

namespace PgCs.Cli;

/// <summary>
/// PgCs CLI entry point
/// </summary>
internal static class Program
{
    /// <summary>
    /// Application version
    /// </summary>
    private const string Version = "1.0.0";

    /// <summary>
    /// Main entry point
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        // Handle version flag manually (before parsing)
        if (args.Contains("--version") || args.Contains("-V"))
        {
            PrintVersion();
            return 0;
        }

        // Handle no arguments - show banner and help
        if (args.Length == 0)
        {
            PrintBanner();
            args = new[] { "--help" };
        }

        // Create root command
        var rootCommand = new RootCommand("PgCs - PostgreSQL C# Code Generator")
        {
            Description = """
                CLI tool for generating C# code from PostgreSQL schemas and queries.
                
                Features:
                  • Generate C# classes from PostgreSQL schema DDL
                  • Generate repository pattern code from annotated SQL queries
                  • Type-safe query parameters and result models
                  • Configurable code generation via YAML
                  • Fast startup and efficient processing
                
                Documentation: https://github.com/yourusername/pgcs
                """
        };

        // Create "generate" command with subcommands
        var generateCommand = new GenerateCommand();
        generateCommand.AddCommand(new GenerateSchemaCommand());
        generateCommand.AddCommand(new GenerateQueriesCommand());

        // Add commands to root
        rootCommand.AddCommand(generateCommand);
        rootCommand.AddCommand(new ValidateCommand());
        rootCommand.AddCommand(new InitCommand());

        // Build parser with middleware
        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseExceptionHandler((exception, context) =>
            {
                // Global exception handler
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"✗ Fatal error: {exception.Message}");
                Console.ResetColor();

                if (Environment.GetEnvironmentVariable("PGCS_DEBUG") == "1")
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Stack trace:");
                    Console.Error.WriteLine(exception.StackTrace);
                }
                else
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Set PGCS_DEBUG=1 for detailed stack trace");
                }

                context.ExitCode = 1;
            })
            .Build();

        // Parse and execute
        return await parser.InvokeAsync(args);
    }

    /// <summary>
    /// Print application version
    /// </summary>
    private static void PrintVersion()
    {
        Console.WriteLine($"PgCs CLI v{Version}");
        Console.WriteLine($".NET {Environment.Version}");
        Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}");
    }

    /// <summary>
    /// Print application banner
    /// </summary>
    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("""
              ____        ____
             |  _ \ __ _ / ___|___
             | |_) / _` | |   / __|
             |  __/ (_| | |___\__ \
             |_|   \__, |\____|___/
                   |___/            
                   
            """);
        Console.ResetColor();
        Console.WriteLine($"PostgreSQL C# Code Generator v{Version}");
        Console.WriteLine();
    }
}

