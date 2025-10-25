using PgCs.Cli.Output;
using PgCs.Common.CodeGeneration;
using PgCs.Common.Services;
using PgCs.Common.Utils;

namespace PgCs.Cli.Commands;

/// <summary>
/// Helper методы для работы с validation issues в командах
/// </summary>
public static class ValidationIssueDisplayHelper
{
    /// <summary>
    /// Отображает validation issues в консоли
    /// </summary>
    public static void DisplayValidationIssues(
        ConsoleWriter writer,
        IReadOnlyList<ValidationMessage> issues,
        string contextName)
    {
        if (issues.Count == 0)
            return;

        writer.WriteLine();
        writer.Info($"Found {issues.Count} issue(s) during {contextName}:");
        writer.WriteLine();

        foreach (var issue in issues)
        {
            // Format message
            var message = $"[{issue.Code}] {issue.Message}";

            // Display based on severity
            if (issue.Severity == ValidationSeverity.Error)
            {
                writer.Error($"ERROR: {message}");
            }
            else if (issue.Severity == ValidationSeverity.Warning)
            {
                writer.Warning($"{message}");
            }
            else
            {
                writer.Info($"{message}");
            }

            // Display location if available
            if (!string.IsNullOrEmpty(issue.Location))
            {
                var locationPreview = StringParsingHelpers.Truncate(issue.Location);
                writer.Info($"  → {locationPreview}");
            }
        }
        writer.WriteLine();
    }
}
