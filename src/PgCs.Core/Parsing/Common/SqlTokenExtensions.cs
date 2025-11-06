using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.Common;

/// <summary>
/// Методы расширения для работы с SQL токенами
/// </summary>
public static class SqlTokenExtensions
{
    /// <summary>
    /// Проверяет, является ли токен тривиальным (whitespace, comment)
    /// Trivia токены не влияют на синтаксический анализ, но важны для форматирования
    /// </summary>
    public static bool IsTrivia(this SqlToken token) =>
        token.Type is TokenType.Whitespace 
            or TokenType.LineComment 
            or TokenType.BlockComment;

    /// <summary>
    /// Проверяет, является ли токен значащим (не trivia)
    /// </summary>
    public static bool IsSignificant(this SqlToken token) => 
        !token.IsTrivia();

    /// <summary>
    /// Проверяет, является ли токен комментарием (любого типа)
    /// </summary>
    public static bool IsComment(this SqlToken token) =>
        token.Type is TokenType.LineComment or TokenType.BlockComment;

    /// <summary>
    /// Проверяет, является ли токен ключевым словом
    /// </summary>
    public static bool IsKeyword(this SqlToken token) =>
        token.Type == TokenType.Keyword;

    /// <summary>
    /// Проверяет, является ли токен идентификатором (обычным или quoted)
    /// </summary>
    public static bool IsIdentifier(this SqlToken token) =>
        token.Type is TokenType.Identifier or TokenType.QuotedIdentifier;
}
