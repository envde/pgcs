namespace PgCs.Core.Schema.Common;

/// <summary>
/// Параметр функции
/// </summary>
public sealed record FunctionParameter
{
    /// <summary>
    /// Имя параметра
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Тип данных PostgreSQL параметра
    /// </summary>
    public required string DataType { get; init; }
    
    /// <summary>
    /// Режим параметра (In, Out, InOut, Variadic)
    /// </summary>
    public ParameterMode Mode { get; init; } = ParameterMode.In;
    
    /// <summary>
    /// Значение по умолчанию для параметра
    /// </summary>
    public string? DefaultValue { get; init; }
}