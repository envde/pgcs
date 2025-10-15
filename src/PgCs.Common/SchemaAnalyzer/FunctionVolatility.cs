namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Волатильность функции (влияет на оптимизацию)
/// </summary>
public enum FunctionVolatility
{
    Volatile,
    Stable,
    Immutable
}