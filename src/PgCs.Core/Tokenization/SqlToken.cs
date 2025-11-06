namespace PgCs.Core.Tokenization;

/// <summary>
/// SQL токен - минимальная синтаксическая единица PostgreSQL кода
/// </summary>
/// <remarks>
/// Использует ReadOnlyMemory для zero-allocation работы со строками.
/// Токен является value type (record struct) для минимизации heap allocations.
/// Поддерживает как trivia токены (whitespace, комментарии), так и значащие токены (keywords, identifiers, literals).
/// </remarks>
public readonly record struct SqlToken
{
    /// <summary>
    /// Тип токена (ключевое слово, идентификатор, оператор и т.д.)
    /// </summary>
    public required TokenType Type { get; init; }

    /// <summary>
    /// Значение токена как ReadOnlyMemory для zero-allocation операций
    /// </summary>
    /// <remarks>
    /// Не создает копию строки, а ссылается на участок исходного текста.
    /// Используйте Value property для материализации строки.
    /// </remarks>
    public required ReadOnlyMemory<char> ValueMemory { get; init; }

    /// <summary>
    /// Значение токена как string
    /// </summary>
    /// <remarks>
    /// Материализует строку из ReadOnlyMemory при каждом обращении.
    /// Для производительности предпочитайте ValueMemory.Span в hot paths.
    /// </remarks>
    public string Value => ValueMemory.ToString();

    /// <summary>
    /// Позиция токена в исходном тексте (начало и конец)
    /// </summary>
    public required TextSpan Span { get; init; }

    /// <summary>
    /// Номер строки в исходном тексте (начинается с 1)
    /// </summary>
    public required int Line { get; init; }

    /// <summary>
    /// Позиция в строке (колонка), начинается с 1
    /// </summary>
    public required int Column { get; init; }

    /// <summary>
    /// Проверяет, является ли токен тривиальным (whitespace или комментарий)
    /// </summary>
    /// <returns>true если токен Whitespace, LineComment или BlockComment</returns>
    /// <remarks>
    /// Trivia токены не влияют на семантику кода, но важны для форматирования и сохранения комментариев.
    /// </remarks>
    public bool IsTrivia => Type is TokenType.Whitespace or TokenType.LineComment or TokenType.BlockComment;
}