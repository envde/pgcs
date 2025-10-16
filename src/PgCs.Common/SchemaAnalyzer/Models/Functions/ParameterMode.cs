namespace PgCs.Common.SchemaAnalyzer.Models.Functions;

/// <summary>
/// Режим параметра функции
/// </summary>
public enum ParameterMode
{
    In,
    Out,
    InOut,
    Variadic
}