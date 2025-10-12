namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация о функции/процедуре
/// </summary>
public class FunctionInfo
{
    /// <summary>
    /// Имя функции
    /// </summary>
    public required string FunctionName { get; init; }

    /// <summary>
    /// Схема
    /// </summary>
    public required string SchemaName { get; init; }

    /// <summary>
    /// Тип возвращаемого значения
    /// </summary>
    public string? ReturnType { get; init; }

    /// <summary>
    /// Параметры функции
    /// </summary>
    public required IReadOnlyList<FunctionParameter> Parameters { get; init; }

    /// <summary>
    /// Язык (plpgsql, sql, c)
    /// </summary>
    public required string Language { get; init; }

    /// <summary>
    /// Определение функции
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; init; }
}
