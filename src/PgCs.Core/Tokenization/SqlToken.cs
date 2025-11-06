namespace PgCs.Core.Tokenization;

/// <summary>
/// SQL токен - минимальная синтаксическая единица
/// </summary>
public readonly record struct SqlToken
{
    /// <summary>Тип токена</summary>
    public required TokenType Type { get; init; }
    
    /// <summary>Значение токена (исходный текст)</summary>
    public required string Value { get; init; }
    
    /// <summary>Позиция в исходном тексте</summary>
    public required TextSpan Span { get; init; }
    
    /// <summary>Номер строки (с 1)</summary>
    public required int Line { get; init; }
    
    /// <summary>Позиция в строке (с 1)</summary>
    public required int Column { get; init; }

    /// <summary>Является ли токен тривиальным (whitespace, comment)</summary>
    public bool IsTrivia => Type is TokenType.Whitespace or TokenType.LineComment or TokenType.BlockComment;
}