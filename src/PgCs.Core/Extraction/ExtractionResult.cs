using PgCs.Core.Schema.Common;
using PgCs.Core.Validation;

namespace PgCs.Core.Extraction;

/// <summary>
/// Результат извлечения объекта из sql block
/// </summary>
public sealed record ExtractionResult<T> where T: DefinitionBase
{
    /// <summary>
    /// Содержи извлеченный объект, если операция завершилась успешно.
    /// </summary>
    public T? Definition { get; init; }

    /// <summary>
    /// Список проблем валидации, возникших при извлечении
    /// </summary>
    public IReadOnlyList<ValidationIssue> ValidationIssues { get; init; } = [];

    /// <summary>
    /// Успешно ли завершилось извлечение
    /// </summary>
    public bool IsSuccess => Definition is not null &&
                             ValidationIssues.All(i => i.Severity != ValidationIssue.ValidationSeverity.Error);
    
    /// <summary>
    /// Создает успешный результат извлечения
    /// </summary>
    /// <param name="definition">Объект извлеченный из SQL</param>
    /// <param name="issues">Сообщения в ходе извлечения объекта</param>
    /// <returns></returns>
    public static ExtractionResult<T> Success(T definition, IReadOnlyList<ValidationIssue>? issues = null)
    {
        return new ExtractionResult<T>
        {
            Definition = definition,
            ValidationIssues = issues ?? []
        };
    }


    /// <summary>
    /// Создает результат с ошибкой
    /// </summary>
    /// <param name="issues">Сообщения в ходе извлечения объекта</param>
    /// <returns></returns>
    public static ExtractionResult<T> Failure(IReadOnlyList<ValidationIssue> issues)
    {
        return new ExtractionResult<T>
        {
            Definition = null,
            ValidationIssues = issues
        };
    }

    /// <summary>
    /// Создает пустой результат
    /// </summary>
    public static ExtractionResult<T> NotApplicable()
    {
        return new ExtractionResult<T>
        {
            Definition = null,
            ValidationIssues = []
        };
    }
}