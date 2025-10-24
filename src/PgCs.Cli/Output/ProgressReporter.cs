using System.Diagnostics;

namespace PgCs.Cli.Output;

/// <summary>
/// Progress reporter for long-running operations
/// </summary>
public sealed class ProgressReporter
{
    private readonly ConsoleWriter _writer;
    private readonly Stopwatch _stopwatch = new();
    private string? _currentOperation;
    private int _totalSteps;
    private int _completedSteps;

    public ProgressReporter(ConsoleWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Start progress tracking
    /// </summary>
    public void Start(string operation, int totalSteps = 0)
    {
        _currentOperation = operation;
        _totalSteps = totalSteps;
        _completedSteps = 0;
        _stopwatch.Restart();
        
        _writer.Step($"Starting: {operation}");
    }

    /// <summary>
    /// Report progress step
    /// </summary>
    public void Step(string stepName)
    {
        _completedSteps++;
        
        if (_totalSteps > 0)
        {
            var percentage = (int)((double)_completedSteps / _totalSteps * 100);
            _writer.Dim($"  [{_completedSteps}/{_totalSteps}] {percentage}% - {stepName}");
        }
        else
        {
            _writer.Dim($"  • {stepName}");
        }
    }

    /// <summary>
    /// Report completion
    /// </summary>
    public void Complete(string? message = null)
    {
        _stopwatch.Stop();
        var elapsed = _stopwatch.Elapsed;
        
        var completionMessage = message ?? _currentOperation ?? "Operation";
        var timeInfo = FormatElapsedTime(elapsed);
        
        _writer.Success($"{completionMessage} completed in {timeInfo}");
    }

    /// <summary>
    /// Report failure
    /// </summary>
    public void Fail(string error)
    {
        _stopwatch.Stop();
        _writer.Error($"{_currentOperation ?? "Operation"} failed: {error}");
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
        
        return $"{elapsed.TotalMinutes:F2}m";
    }
}
