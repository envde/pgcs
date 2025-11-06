namespace PgCs.Core.Validation;

/// <summary>
/// Методы расширения для работы с коллекциями ValidationIssue
/// </summary>
/// <remarks>
/// Предоставляет удобные методы для фильтрации, группировки и подсчета проблем валидации по severity level.
/// </remarks>
public static class ValidationExtensions
{
    /// <summary>
    /// Проверяет, содержит ли список хотя бы одну ошибку (Error severity)
    /// </summary>
    /// <param name="issues">Список проблем для проверки</param>
    /// <returns>true если найдена хотя бы одна ошибка</returns>
    public static bool HasErrors(this IReadOnlyList<ValidationIssue> issues) =>
        issues.Any(i => i.Severity == ValidationIssue.ValidationSeverity.Error);

    /// <summary>
    /// Проверяет, содержит ли список хотя бы одно предупреждение (Warning severity)
    /// </summary>
    /// <param name="issues">Список проблем для проверки</param>
    /// <returns>true если найдено хотя бы одно предупреждение</returns>
    public static bool HasWarnings(this IReadOnlyList<ValidationIssue> issues) =>
        issues.Any(i => i.Severity == ValidationIssue.ValidationSeverity.Warning);

    /// <summary>
    /// Фильтрует список, возвращая только ошибки (Error severity)
    /// </summary>
    /// <param name="issues">Список проблем для фильтрации</param>
    /// <returns>Последовательность только ошибок</returns>
    public static IEnumerable<ValidationIssue> GetErrors(this IReadOnlyList<ValidationIssue> issues) =>
        issues.Where(i => i.Severity == ValidationIssue.ValidationSeverity.Error);

    /// <summary>
    /// Фильтрует список, возвращая только предупреждения (Warning severity)
    /// </summary>
    /// <param name="issues">Список проблем для фильтрации</param>
    /// <returns>Последовательность только предупреждений</returns>
    public static IEnumerable<ValidationIssue> GetWarnings(this IReadOnlyList<ValidationIssue> issues) =>
        issues.Where(i => i.Severity == ValidationIssue.ValidationSeverity.Warning);

    /// <summary>
    /// Фильтрует список, возвращая только информационные сообщения (Info severity)
    /// </summary>
    /// <param name="issues">Список проблем для фильтрации</param>
    /// <returns>Последовательность только информационных сообщений</returns>
    public static IEnumerable<ValidationIssue> GetInfoMessages(this IReadOnlyList<ValidationIssue> issues) =>
        issues.Where(i => i.Severity == ValidationIssue.ValidationSeverity.Info);

    /// <summary>
    /// Группирует проблемы по типу объекта (Table, View, Function и т.д.)
    /// </summary>
    /// <param name="issues">Список проблем для группировки</param>
    /// <returns>Группированные проблемы по DefinitionType</returns>
    public static IEnumerable<IGrouping<ValidationIssue.ValidationDefinitionType, ValidationIssue>> GroupByDefinitionType(
        this IReadOnlyList<ValidationIssue> issues) =>
        issues.GroupBy(i => i.DefinitionType);

    /// <summary>
    /// Группирует проблемы по уровню серьезности (Info, Warning, Error)
    /// </summary>
    /// <param name="issues">Список проблем для группировки</param>
    /// <returns>Группированные проблемы по Severity</returns>
    public static IEnumerable<IGrouping<ValidationIssue.ValidationSeverity, ValidationIssue>> GroupBySeverity(
        this IReadOnlyList<ValidationIssue> issues) =>
        issues.GroupBy(i => i.Severity);

    /// <summary>
    /// Подсчитывает количество ошибок в списке
    /// </summary>
    /// <param name="issues">Список проблем для подсчета</param>
    /// <returns>Количество ошибок (Error severity)</returns>
    public static int CountErrors(this IReadOnlyList<ValidationIssue> issues) =>
        issues.Count(i => i.Severity == ValidationIssue.ValidationSeverity.Error);

    /// <summary>
    /// Подсчитывает количество предупреждений в списке
    /// </summary>
    /// <param name="issues">Список проблем для подсчета</param>
    /// <returns>Количество предупреждений (Warning severity)</returns>
    public static int CountWarnings(this IReadOnlyList<ValidationIssue> issues) =>
        issues.Count(i => i.Severity == ValidationIssue.ValidationSeverity.Warning);

    /// <summary>
    /// Создает форматированное текстовое представление всех проблем валидации
    /// </summary>
    /// <param name="issues">Список проблем для форматирования</param>
    /// <returns>Многострочный текст с подробным описанием всех проблем, сгруппированных по severity</returns>
    /// <remarks>
    /// Выводит сводку: общее количество проблем, количество ошибок и предупреждений.
    /// Затем группирует проблемы по severity и выводит детали каждой проблемы с локацией.
    /// </remarks>
    public static string ToFormattedString(this IReadOnlyList<ValidationIssue> issues)
    {
        if (issues.Count == 0)
        {
            return "No validation issues found.";
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Validation Issues ({issues.Count} total):");
        sb.AppendLine($"  Errors: {issues.CountErrors()}");
        sb.AppendLine($"  Warnings: {issues.CountWarnings()}");
        sb.AppendLine();

        foreach (var group in issues.GroupBySeverity())
        {
            sb.AppendLine($"{group.Key}:");
            foreach (var issue in group)
            {
                sb.AppendLine($"  [{issue.Code}] {issue.Message}");
                sb.AppendLine($"    Location: {issue.Location.Segment} (Line {issue.Location.Line}, Column {issue.Location.Column})");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}