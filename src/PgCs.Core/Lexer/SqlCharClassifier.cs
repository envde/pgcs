namespace PgCs.Core.Lexer;

/// <summary>
/// Классификатор символов SQL
/// Определяет тип символа для токенизации
/// </summary>
public static class SqlCharClassifier
{
    /// <summary>
    /// Проверяет, является ли символ пробельным (whitespace)
    /// </summary>
    public static bool IsWhitespace(char ch) =>
        ch is ' ' or '\t' or '\r' or '\n';

    /// <summary>
    /// Проверяет, может ли символ начинать идентификатор
    /// PostgreSQL: буква или подчеркивание
    /// </summary>
    public static bool IsIdentifierStart(char ch) =>
        char.IsLetter(ch) || ch == '_';

    /// <summary>
    /// Проверяет, может ли символ быть частью идентификатора
    /// PostgreSQL: буква, цифра, подчеркивание, знак доллара (для $1, $2 параметров)
    /// </summary>
    public static bool IsIdentifierPart(char ch) =>
        char.IsLetterOrDigit(ch) || ch is '_' or '$';

    /// <summary>
    /// Проверяет, является ли символ цифрой
    /// </summary>
    public static bool IsDigit(char ch) =>
        char.IsDigit(ch);

    /// <summary>
    /// Проверяет, является ли символ оператором
    /// </summary>
    public static bool IsOperatorChar(char ch) =>
        ch is '+' or '-' or '*' or '/' or '%' or '^'
            or '<' or '>' or '=' or '!' or '|' or '&'
            or '~' or '#';
}