namespace PgCs.Common.QueryGenerator.Models.Options;

/// <summary>
/// Стратегия обработки NULL значений
/// </summary>
public enum NullHandlingStrategy
{
    /// <summary>
    /// Использовать nullable типы (int?, string?)
    /// </summary>
    Nullable,

    /// <summary>
    /// Использовать default значения для value types
    /// </summary>
    DefaultValues,

    /// <summary>
    /// Генерировать исключение при NULL значении
    /// </summary>
    ThrowException
}
