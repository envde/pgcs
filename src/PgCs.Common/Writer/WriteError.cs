namespace PgCs.Common.Writer;

/// <summary>
/// Ошибка записи файла
/// </summary>
public sealed record WriteError
{
    /// <summary>
    /// Путь к файлу, при записи которого произошла ошибка
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Тип ошибки
    /// </summary>
    public WriteErrorType ErrorType { get; init; }

    /// <summary>
    /// Исходное исключение (если есть)
    /// </summary>
    public Exception? Exception { get; init; }
}
