using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.QueryGenerator.Services;

/// <summary>
/// Валидатор метаданных запросов перед генерацией
/// </summary>
public sealed class QueryValidator : IQueryValidator
{
    public IReadOnlyList<ValidationIssue> Validate(IReadOnlyList<QueryMetadata> queries)
    {
        var issues = new List<ValidationIssue>();

        // Проверка на пустой список запросов
        if (!queries.Any())
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Code = "EMPTY_QUERIES",
                Message = "No queries to generate"
            });
            return issues;
        }

        // Проверка каждого запроса
        var methodNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var query in queries)
        {
            ValidateQuery(query, methodNames, issues);
        }

        return issues;
    }

    private static void ValidateQuery(
        QueryMetadata query,
        HashSet<string> methodNames,
        List<ValidationIssue> issues)
    {
        // Проверка имени метода
        if (string.IsNullOrWhiteSpace(query.MethodName))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "EMPTY_METHOD_NAME",
                Message = "Query has empty method name"
            });
            return;
        }

        // Проверка на дублирование имен методов
        if (!methodNames.Add(query.MethodName))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "DUPLICATE_METHOD_NAME",
                Message = $"Duplicate method name '{query.MethodName}'",
                Location = query.MethodName
            });
        }

        // Проверка SQL запроса
        if (string.IsNullOrWhiteSpace(query.SqlQuery))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "EMPTY_SQL",
                Message = $"Query '{query.MethodName}' has empty SQL",
                Location = query.MethodName
            });
        }

        // Проверка параметров
        var paramNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var param in query.Parameters)
        {
            if (string.IsNullOrWhiteSpace(param.Name))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "EMPTY_PARAMETER_NAME",
                    Message = $"Query '{query.MethodName}' has parameter with empty name",
                    Location = query.MethodName
                });
                continue;
            }

            if (!paramNames.Add(param.Name))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Code = "DUPLICATE_PARAMETER",
                    Message = $"Query '{query.MethodName}' has duplicate parameter '{param.Name}'",
                    Location = query.MethodName
                });
            }

            if (string.IsNullOrWhiteSpace(param.PostgresType))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Code = "MISSING_PARAMETER_TYPE",
                    Message = $"Query '{query.MethodName}' parameter '{param.Name}' has no PostgreSQL type",
                    Location = query.MethodName
                });
            }
        }

        // Проверка возвращаемого типа для SELECT запросов
        if (query.QueryType == QueryType.Select)
        {
            if (query.ReturnCardinality != ReturnCardinality.Exec && query.ReturnType == null)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "MISSING_RETURN_TYPE",
                    Message = $"SELECT query '{query.MethodName}' has no return type information",
                    Location = query.MethodName
                });
            }

            if (query.ReturnType != null && !query.ReturnType.Columns.Any())
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Code = "NO_RETURN_COLUMNS",
                    Message = $"Query '{query.MethodName}' has no return columns",
                    Location = query.MethodName
                });
            }
        }

        // Проверка колонок результата
        if (query.ReturnType != null)
        {
            var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in query.ReturnType.Columns)
            {
                if (string.IsNullOrWhiteSpace(column.Name))
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Code = "EMPTY_COLUMN_NAME",
                        Message = $"Query '{query.MethodName}' has column with empty name",
                        Location = query.MethodName
                    });
                    continue;
                }

                if (!columnNames.Add(column.Name))
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Code = "DUPLICATE_COLUMN",
                        Message = $"Query '{query.MethodName}' has duplicate column '{column.Name}'",
                        Location = query.MethodName
                    });
                }
            }
        }
    }
}
