using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Fluent API для комплексной валидации схемы и запросов
/// </summary>
public class ValidationPipeline
{
    private readonly List<ValidationIssue> _issues = new();
    private readonly List<Func<IEnumerable<ValidationIssue>>> _validators = new();
    private SchemaMetadata? _schema;
    private IReadOnlyList<QueryMetadata>? _queries;
    private ValidationSeverity _minimumSeverity = ValidationSeverity.Warning;
    private bool _stopOnFirstError = false;
    private Action<ValidationIssue>? _onIssue;

    private ValidationPipeline() { }

    /// <summary>
    /// Создать новый validation pipeline
    /// </summary>
    public static ValidationPipeline Create() => new();

    /// <summary>
    /// Валидировать схему базы данных
    /// </summary>
    public ValidationPipeline ForSchema(SchemaMetadata schema)
    {
        _schema = schema;
        return this;
    }

    /// <summary>
    /// Валидировать SQL запросы
    /// </summary>
    public ValidationPipeline ForQueries(IReadOnlyList<QueryMetadata> queries)
    {
        _queries = queries;
        return this;
    }

    /// <summary>
    /// Проверить всё (таблицы, колонки, типы, индексы, запросы)
    /// </summary>
    public ValidationPipeline ValidateAll()
    {
        return CheckSchema()
            .CheckQueries()
            .CheckParameters()
            .CheckReturnTypes();
    }

    /// <summary>
    /// Проверить схему (таблицы, колонки, типы)
    /// </summary>
    public ValidationPipeline CheckSchema()
    {
        if (_schema == null) return this;

        _validators.Add(() =>
        {
            var issues = new List<ValidationIssue>();

            // Проверка наличия таблиц
            if (_schema.Tables.Count == 0)
            {
                issues.Add(CreateIssue(
                    ValidationSeverity.Warning,
                    "NO_TABLES",
                    "Схема не содержит таблиц",
                    "Schema"));
            }

            // Проверка дубликатов таблиц
            var duplicateTables = _schema.Tables
                .GroupBy(t => t.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateTables.Any())
            {
                issues.Add(CreateIssue(
                    ValidationSeverity.Error,
                    "DUPLICATE_TABLES",
                    $"Найдены дублирующиеся таблицы: {string.Join(", ", duplicateTables)}",
                    "Schema.Tables"));
            }

            // Проверка таблиц без колонок
            var tablesWithoutColumns = _schema.Tables
                .Where(t => t.Columns.Count == 0)
                .Select(t => t.Name)
                .ToList();

            if (tablesWithoutColumns.Any())
            {
                issues.Add(CreateIssue(
                    ValidationSeverity.Error,
                    "NO_COLUMNS",
                    $"Таблицы без колонок: {string.Join(", ", tablesWithoutColumns)}",
                    "Schema.Tables"));
            }

            // Проверка таблиц без PRIMARY KEY
            var tablesWithoutPK = _schema.Tables
                .Where(t => !t.Columns.Any(c => c.IsPrimaryKey))
                .Select(t => t.Name)
                .ToList();

            if (tablesWithoutPK.Any())
            {
                issues.Add(CreateIssue(
                    ValidationSeverity.Warning,
                    "NO_PRIMARY_KEY",
                    $"Таблицы без PRIMARY KEY ({tablesWithoutPK.Count}): {string.Join(", ", tablesWithoutPK.Take(5))}{(tablesWithoutPK.Count > 5 ? "..." : "")}",
                    "Schema.Tables"));
            }

            return issues;
        });

        return this;
    }

    /// <summary>
    /// Проверить запросы на корректность
    /// </summary>
    public ValidationPipeline CheckQueries()
    {
        if (_queries == null) return this;

        _validators.Add(() =>
        {
            var issues = new List<ValidationIssue>();

            // Проверка дубликатов имен методов
            var duplicateNames = _queries
                .GroupBy(q => q.MethodName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNames.Any())
            {
                issues.Add(CreateIssue(
                    ValidationSeverity.Error,
                    "DUPLICATE_METHOD_NAMES",
                    $"Дублирующиеся имена методов: {string.Join(", ", duplicateNames)}",
                    "Queries"));
            }

            // Проверка пустых запросов
            var emptyQueries = _queries
                .Where(q => string.IsNullOrWhiteSpace(q.SqlQuery))
                .Select(q => q.MethodName)
                .ToList();

            if (emptyQueries.Any())
            {
                issues.Add(CreateIssue(
                    ValidationSeverity.Error,
                    "EMPTY_SQL",
                    $"Пустые SQL запросы: {string.Join(", ", emptyQueries)}",
                    "Queries"));
            }

            return issues;
        });

        return this;
    }

    /// <summary>
    /// Проверить параметры запросов
    /// </summary>
    public ValidationPipeline CheckParameters()
    {
        if (_queries == null) return this;

        _validators.Add(() =>
        {
            var issues = new List<ValidationIssue>();

            var queriesWithManyParams = _queries
                .Where(q => q.Parameters.Count > 10)
                .Select(q => new { q.MethodName, q.Parameters.Count })
                .ToList();

            if (queriesWithManyParams.Any())
            {
                foreach (var q in queriesWithManyParams)
                {
                    issues.Add(CreateIssue(
                        ValidationSeverity.Warning,
                        "TOO_MANY_PARAMETERS",
                        $"Запрос '{q.MethodName}' имеет {q.Count} параметров (рекомендуется Parameter Model)",
                        $"Queries.{q.MethodName}"));
                }
            }

            return issues;
        });

        return this;
    }

    /// <summary>
    /// Проверить возвращаемые типы
    /// </summary>
    public ValidationPipeline CheckReturnTypes()
    {
        if (_queries == null) return this;

        _validators.Add(() =>
        {
            var issues = new List<ValidationIssue>();

            var queriesWithoutReturnType = _queries
                .Where(q => q.ReturnType == null && q.ReturnCardinality != ReturnCardinality.Exec)
                .Select(q => q.MethodName)
                .ToList();

            if (queriesWithoutReturnType.Any())
            {
                issues.Add(CreateIssue(
                    ValidationSeverity.Warning,
                    "NO_RETURN_TYPE",
                    $"Запросы без определённого типа возврата: {string.Join(", ", queriesWithoutReturnType)}",
                    "Queries"));
            }

            return issues;
        });

        return this;
    }

    /// <summary>
    /// Установить минимальный уровень серьёзности
    /// </summary>
    public ValidationPipeline WithMinimumSeverity(ValidationSeverity severity)
    {
        _minimumSeverity = severity;
        return this;
    }

    /// <summary>
    /// Остановить при первой ошибке
    /// </summary>
    public ValidationPipeline StopOnFirstError()
    {
        _stopOnFirstError = true;
        return this;
    }

    /// <summary>
    /// Обработчик каждой проблемы
    /// </summary>
    public ValidationPipeline OnIssue(Action<ValidationIssue> handler)
    {
        _onIssue = handler;
        return this;
    }

    /// <summary>
    /// Выполнить валидацию
    /// </summary>
    public ValidationResult Validate()
    {
        _issues.Clear();

        foreach (var validator in _validators)
        {
            var validatorIssues = validator();
            foreach (var issue in validatorIssues.Where(i => i.Severity >= _minimumSeverity))
            {
                _issues.Add(issue);
                _onIssue?.Invoke(issue);

                if (_stopOnFirstError && issue.Severity == ValidationSeverity.Error)
                {
                    goto EndValidation;
                }
            }
        }

    EndValidation:
        var hasErrors = _issues.HasErrors();

        return new ValidationResult
        {
            IsValid = !hasErrors,
            Issues = _issues,
            ErrorCount = _issues.CountErrors(),
            WarningCount = _issues.CountWarnings(),
            InfoCount = _issues.CountInfo()
        };
    }

    /// <summary>
    /// Выполнить валидацию и бросить исключение при ошибках
    /// </summary>
    public ValidationResult ValidateOrThrow()
    {
        var result = Validate();
        if (!result.IsValid)
        {
            var errorMessages = result.Issues
                .Where(i => i.Severity == ValidationSeverity.Error)
                .Select(i => $"[{i.Code}] {i.Message}");

            throw new ValidationException(
                $"Обнаружены ошибки валидации:\n{string.Join("\n", errorMessages)}",
                result.Issues);
        }
        return result;
    }

    private static ValidationIssue CreateIssue(
        ValidationSeverity severity,
        string code,
        string message,
        string? location = null)
    {
        return new ValidationIssue
        {
            Severity = severity,
            Code = code,
            Message = message,
            Location = location
        };
    }
}

/// <summary>
/// Результат валидации
/// </summary>
public record ValidationResult
{
    public required bool IsValid { get; init; }
    public required IReadOnlyList<ValidationIssue> Issues { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int InfoCount { get; init; }

    public string GetReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Validation Report:");
        sb.AppendLine($"  Valid: {IsValid}");
        sb.AppendLine($"  Errors: {ErrorCount}");
        sb.AppendLine($"  Warnings: {WarningCount}");
        sb.AppendLine($"  Info: {InfoCount}");

        if (Issues.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Issues:");
            foreach (var issue in Issues)
            {
                sb.AppendLine($"  [{issue.Severity}] [{issue.Code}] {issue.Location}: {issue.Message}");
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Исключение валидации
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyList<ValidationIssue> Issues { get; }

    public ValidationException(string message, IReadOnlyList<ValidationIssue> issues)
        : base(message)
    {
        Issues = issues;
    }
}
