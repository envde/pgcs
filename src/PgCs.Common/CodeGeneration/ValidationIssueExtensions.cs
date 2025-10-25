namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Extension методы для работы с коллекциями ValidationIssue
/// </summary>
public static class ValidationIssueExtensions
{
    /// <summary>
    /// Подсчитывает количество ошибок в коллекции
    /// </summary>
    public static int CountErrors(this IEnumerable<ValidationIssue> issues)
    {
        return issues.Count(i => i.Severity == ValidationSeverity.Error);
    }

    /// <summary>
    /// Подсчитывает количество предупреждений в коллекции
    /// </summary>
    public static int CountWarnings(this IEnumerable<ValidationIssue> issues)
    {
        return issues.Count(i => i.Severity == ValidationSeverity.Warning);
    }

    /// <summary>
    /// Подсчитывает количество информационных сообщений в коллекции
    /// </summary>
    public static int CountInfo(this IEnumerable<ValidationIssue> issues)
    {
        return issues.Count(i => i.Severity == ValidationSeverity.Info);
    }

    /// <summary>
    /// Проверяет наличие ошибок в коллекции
    /// </summary>
    public static bool HasErrors(this IEnumerable<ValidationIssue> issues)
    {
        return issues.Any(i => i.Severity == ValidationSeverity.Error);
    }

    /// <summary>
    /// Проверяет наличие предупреждений в коллекции
    /// </summary>
    public static bool HasWarnings(this IEnumerable<ValidationIssue> issues)
    {
        return issues.Any(i => i.Severity == ValidationSeverity.Warning);
    }

    /// <summary>
    /// Фильтрует коллекцию по уровню серьёзности
    /// </summary>
    public static IEnumerable<ValidationIssue> WithSeverity(this IEnumerable<ValidationIssue> issues, ValidationSeverity severity)
    {
        return issues.Where(i => i.Severity == severity);
    }

    /// <summary>
    /// Фильтрует коллекцию по минимальному уровню серьёзности
    /// </summary>
    public static IEnumerable<ValidationIssue> WithMinimumSeverity(this IEnumerable<ValidationIssue> issues, ValidationSeverity minimumSeverity)
    {
        return issues.Where(i => i.Severity >= minimumSeverity);
    }
}
