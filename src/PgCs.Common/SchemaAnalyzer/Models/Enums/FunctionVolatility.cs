namespace PgCs.Common.SchemaAnalyzer.Models.Enums;

/// <summary>
/// Волатильность функции (влияет на оптимизацию)
/// </summary>
public enum FunctionVolatility
{
    Volatile,
    Stable,
    Immutable
}