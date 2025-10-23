namespace PgCs.Common.SchemaAnalyzer.Models.Functions;

/// <summary>
/// Режим параметра функции (направление передачи данных)
/// </summary>
public enum ParameterMode
{
    /// <summary>
    /// IN - входной параметр (передается в функцию)
    /// </summary>
    In,

    /// <summary>
    /// OUT - выходной параметр (возвращается из функции)
    /// </summary>
    Out,

    /// <summary>
    /// INOUT - параметр работает и как входной, и как выходной
    /// </summary>
    InOut,

    /// <summary>
    /// VARIADIC - переменное количество параметров одного типа
    /// </summary>
    Variadic
}