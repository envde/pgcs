namespace PgCs.Core.Tokenization;

/// <summary>
/// Типы SQL токенов PostgreSQL
/// </summary>
public enum TokenType
{
    // === Trivia (не влияют на парсинг) ===
    /// <summary>Пробелы, табы, переносы строк</summary>
    Whitespace,

    /// <summary>Однострочный комментарий: -- comment</summary>
    LineComment,

    /// <summary>Многострочный комментарий: /* comment */</summary>
    BlockComment,

    // === Keywords ===
    /// <summary>Ключевое слово SQL (CREATE, SELECT, TABLE, etc.)</summary>
    Keyword,

    // === Identifiers ===
    /// <summary>Обычный идентификатор: table_name, column_name</summary>
    Identifier,

    /// <summary>Quoted идентификатор: "Table Name"</summary>
    QuotedIdentifier,

    // === Literals ===
    /// <summary>Строковый литерал: 'text'</summary>
    StringLiteral,

    /// <summary>Dollar-quoted строка: $$text$$ или $tag$text$tag$</summary>
    DollarQuotedString,

    /// <summary>Числовой литерал: 123, 45.67, 1e10, 0x1A</summary>
    NumericLiteral,

    // === Operators & Punctuation ===
    /// <summary>Оператор: =, +, -, *, /, ||, etc.</summary>
    Operator,

    /// <summary>Открывающая круглая скобка: (</summary>
    OpenParen,

    /// <summary>Закрывающая круглая скобка: )</summary>
    CloseParen,

    /// <summary>Открывающая квадратная скобка: [ (для массивов)</summary>
    OpenBracket,

    /// <summary>Закрывающая квадратная скобка: ]</summary>
    CloseBracket,

    /// <summary>Точка с запятой: ;</summary>
    Semicolon,

    /// <summary>Запятая: ,</summary>
    Comma,

    /// <summary>Точка: .</summary>
    Dot,

    // === Special ===
    /// <summary>Конец файла</summary>
    EndOfFile,

    /// <summary>Неизвестный токен (ошибка)</summary>
    Unknown
}