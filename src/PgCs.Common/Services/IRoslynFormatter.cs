namespace PgCs.Common.Services;

/// <summary>
/// Форматировщик C# кода с использованием Roslyn
/// </summary>
public interface IRoslynFormatter
{
    /// <summary>
    /// Форматирует C# код согласно стилю
    /// </summary>
    ValueTask<string> FormatAsync(string sourceCode);
}
