using PgCs.Common.CodeGeneration;

namespace PgCs.Common.QueryGenerator.Models.Results;

/// <summary>
/// Результат генерации отдельного метода запроса
/// </summary>
public sealed record GeneratedMethodResult
{
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Имя сгенерированного метода
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// Сигнатура метода
    /// </summary>
    public required string MethodSignature { get; init; }

    /// <summary>
    /// Полный исходный код метода
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Сгенерированный код (если метод в отдельном файле)
    /// </summary>
    public GeneratedCode? Code { get; init; }

    /// <summary>
    /// Модель результата, связанная с этим методом
    /// </summary>
    public GeneratedModelResult? ResultModel { get; init; }

    /// <summary>
    /// Модель параметров, связанная с этим методом
    /// </summary>
    public GeneratedModelResult? ParameterModel { get; init; }

    /// <summary>
    /// Проблемы валидации и предупреждения
    /// </summary>
    public IReadOnlyList<ValidationIssue>? ValidationIssues { get; init; }

    /// <summary>
    /// Оригинальный SQL запрос
    /// </summary>
    public required string SqlQuery { get; init; }
}
