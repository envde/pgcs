using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Core;

/// <summary>
/// Простая реализация валидатора кода
/// </summary>
internal sealed class CodeValidator : ICodeValidator
{
    /// <inheritdoc />
    public ValidationResult Validate(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var warnings = new List<ValidationWarning>();
        var errors = new List<ValidationError>();

        // Базовые проверки
        if (!code.Contains("namespace"))
        {
            warnings.Add(new ValidationWarning
            {
                Code = "VAL001",
                Message = "Отсутствует объявление namespace"
            });
        }

        if (!code.Contains("using"))
        {
            warnings.Add(new ValidationWarning
            {
                Code = "VAL002",
                Message = "Отсутствуют using директивы"
            });
        }

        // Проверка на базовые синтаксические ошибки
        var openBraces = code.Count(c => c == '{');
        var closeBraces = code.Count(c => c == '}');

        if (openBraces != closeBraces)
        {
            errors.Add(new ValidationError
            {
                Code = "ERR001",
                Message = $"Несовпадение фигурных скобок: открывающих {openBraces}, закрывающих {closeBraces}",
                Severity = ErrorSeverity.Error
            });
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
            ValidationTime = TimeSpan.Zero
        };
    }
}
