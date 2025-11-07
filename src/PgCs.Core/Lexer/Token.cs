namespace PgCs.Core.Lexer;

/// <summary>
/// SQL токен - минимальная синтаксическая единица PostgreSQL кода
/// </summary>
/// <remarks>
/// Использует ReadOnlyMemory для zero-allocation работы со строками.
/// Токен является value type (record struct) для минимизации heap allocations.
/// Поддерживает как trivia токены (whitespace, комментарии), так и значащие токены (keywords, identifiers, literals).
/// </remarks>
public readonly record struct Token
{
    /// <summary>
    /// Тип токена (ключевое слово, идентификатор, оператор и т.д.)
    /// </summary>
    public required TokenKind Kind { get; init; }

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

    // === Методы для проверки типа токена ===

    /// <summary>
    /// Проверяет, является ли токен тривиальным (whitespace или комментарий)
    /// </summary>
    /// <returns>true если токен Whitespace, LineComment или BlockComment</returns>
    /// <remarks>
    /// Trivia токены не влияют на семантику кода, но важны для форматирования и сохранения комментариев.
    /// </remarks>
    public bool IsTrivia => Kind is TokenKind.Whitespace or TokenKind.LineComment or TokenKind.BlockComment;

    /// <summary>
    /// Проверяет, является ли токен значащим (не trivia)
    /// </summary>
    /// <returns>true если токен не является trivia</returns>
    public bool IsSignificant => !IsTrivia;

    /// <summary>
    /// Проверяет, является ли токен ключевым словом PostgreSQL
    /// </summary>
    /// <returns>true если тип токена = Keyword</returns>
    public bool IsKeyword => Kind == TokenKind.Keyword;

    /// <summary>
    /// Проверяет, является ли токен идентификатором (обычным или quoted)
    /// </summary>
    /// <returns>true если тип токена = Identifier или QuotedIdentifier</returns>
    public bool IsIdentifier => Kind is TokenKind.Identifier or TokenKind.QuotedIdentifier;

    /// <summary>
    /// Проверяет, является ли токен литералом (строка, число)
    /// </summary>
    /// <returns>true если тип токена = StringLiteral, DollarQuotedString или NumericLiteral</returns>
    public bool IsLiteral => Kind is TokenKind.StringLiteral or TokenKind.DollarQuotedString or TokenKind.NumericLiteral;

    /// <summary>
    /// Проверяет, является ли токен оператором
    /// </summary>
    /// <returns>true если тип токена = Operator</returns>
    public bool IsOperator => Kind == TokenKind.Operator;

    /// <summary>
    /// Диапазон текста с начальной позицией и длиной
    /// </summary>
    /// <param name="Start">Начальная позиция в тексте (0-based)</param>
    /// <param name="Length">Длина диапазона в символах</param>
    public readonly record struct TextSpan(int Start, int Length)
    {
        /// <summary>
        /// Конечная позиция диапазона (эксклюзивная)
        /// </summary>
        public int End => Start + Length;
    }
}