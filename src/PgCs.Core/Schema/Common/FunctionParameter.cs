namespace PgCs.Core.Schema.Common;

/// <summary>
/// Параметр функции или процедуры PostgreSQL
/// </summary>
public sealed record FunctionParameter
{
    /// <summary>
    /// Имя параметра (может отсутствовать для позиционных параметров)
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Тип данных PostgreSQL параметра (integer, text, RECORD и т.д.)
    /// </summary>
    public required string DataType { get; init; }
    
    /// <summary>
    /// Режим параметра: IN (входной), OUT (выходной), INOUT, VARIADIC
    /// </summary>
    public ParameterMode Mode { get; init; } = ParameterMode.In;
    
    /// <summary>
    /// Значение по умолчанию для параметра (DEFAULT expression)
    /// </summary>
    public string? DefaultValue { get; init; }
    
    /// <summary>
    /// Является ли параметр массивом
    /// </summary>
    public bool IsArray { get; init; }
}