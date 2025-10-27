namespace PgCs.Core.Definitions.Schema.Base;

/// <summary>
/// Волатильность функции (влияет на оптимизацию запросов и кэширование результатов)
/// </summary>
public enum FunctionVolatility
{
    /// <summary>
    /// Volatile - функция может возвращать разные результаты для одинаковых аргументов (например, random(), now())
    /// </summary>
    Volatile,

    /// <summary>
    /// Stable - функция возвращает одинаковые результаты для одинаковых аргументов в пределах одного запроса
    /// </summary>
    Stable,

    /// <summary>
    /// Immutable - функция всегда возвращает одинаковые результаты для одинаковых аргументов (например, математические функции)
    /// </summary>
    Immutable
}