namespace PgCs.Common.SchemaAnalyzer.Models.Functions;

/// <summary>
/// Волатильность функции (влияет на оптимизацию)
/// </summary>
public enum FunctionVolatility
{
    Volatile,
    Stable,
    Immutable
}