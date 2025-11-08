using PgCs.Core.Types.Schema;

namespace PgCs.Core.Parser;

/// <summary>
/// Результат операции парсинга
/// Содержит либо успешно распарсенный объект, либо информацию об ошибке
/// </summary>
/// <typeparam name="T">Тип распарсенного объекта</typeparam>
public readonly record struct ParseResult<T> where T : PgObject
{
    /// <summary>
    /// Указывает, был ли парсинг успешным
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Распарсенный объект (null в случае неудачи парсинга)
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Сообщение об ошибке (null в случае успешного парсинга)
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Номер строки, где произошла ошибка (0 если не применимо)
    /// </summary>
    public int ErrorLine { get; init; }

    /// <summary>
    /// Номер колонки, где произошла ошибка (0 если не применимо)
    /// </summary>
    public int ErrorColumn { get; init; }

    /// <summary>
    /// Создаёт успешный результат парсинга
    /// </summary>
    /// <param name="value">Распарсенный объект</param>
    /// <returns>Результат парсинга с успешным статусом</returns>
    public static ParseResult<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    /// <summary>
    /// Создаёт неудачный результат парсинга с информацией об ошибке
    /// </summary>
    /// <param name="error">Сообщение об ошибке</param>
    /// <param name="line">Номер строки ошибки (по умолчанию 0)</param>
    /// <param name="column">Номер колонки ошибки (по умолчанию 0)</param>
    /// <returns>Результат парсинга с информацией об ошибке</returns>
    public static ParseResult<T> Failure(string error, int line = 0, int column = 0) => new()
    {
        IsSuccess = false,
        Error = error,
        ErrorLine = line,
        ErrorColumn = column
    };
}
