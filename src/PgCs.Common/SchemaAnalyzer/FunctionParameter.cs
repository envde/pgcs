namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Параметр функции
/// </summary>
public class FunctionParameter
{
    /// <summary>
    /// Имя параметра
    /// </summary>
    public required string ParameterName { get; init; }

    /// <summary>
    /// PostgreSQL тип
    /// </summary>
    public required string PostgresType { get; init; }

    /// <summary>
    /// Режим (IN, OUT, INOUT)
    /// </summary>
    public string Mode { get; init; } = "IN";

    /// <summary>
    /// Значение по умолчанию
    /// </summary>
    public string? DefaultValue { get; init; }
}
