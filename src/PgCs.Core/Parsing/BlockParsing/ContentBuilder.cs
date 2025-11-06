using System.Text;
using PgCs.Core.Parsing.Common;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.BlockParsing;

/// <summary>
/// Строитель Content из токенов
/// Собирает SQL код из токенов с правильными пробелами
/// </summary>
public sealed class ContentBuilder
{
    /// <summary>
    /// Строит Content из токенов (без trivia)
    /// Добавляет пробелы между токенами где необходимо
    /// </summary>
    public string Build(IReadOnlyList<SqlToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        SqlToken? prevToken = null;

        foreach (var token in tokens)
        {
            // Пропускаем trivia
            if (token.IsTrivia())
            {
                continue;
            }

            // Добавляем пробел между токенами если нужно
            if (prevToken.HasValue && NeedsSpaceBetween(prevToken.Value, token))
            {
                sb.Append(' ');
            }

            sb.Append(token.Value);
            prevToken = token;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Определяет, нужен ли пробел между двумя токенами
    /// </summary>
    public bool NeedsSpaceBetween(SqlToken prev, SqlToken current)
    {
        // Не нужен пробел между ключевым словом и открывающей скобкой: CREATE TABLE(
        if (prev.IsKeyword() && current.Type == TokenType.OpenParen)
        {
            return false;
        }

        // Не нужен пробел перед пунктуацией
        if (current.Type is TokenType.Comma or TokenType.Semicolon 
            or TokenType.CloseParen or TokenType.Dot)
        {
            return false;
        }

        // Не нужен пробел после открывающих скобок
        if (prev.Type == TokenType.OpenParen)
        {
            return false;
        }

        // Не нужен пробел перед и после точки: schema.table
        if (prev.Type == TokenType.Dot || current.Type == TokenType.Dot)
        {
            return false;
        }

        // Не нужен пробел с оператором :: (PostgreSQL cast)
        if (prev.Type == TokenType.Operator && prev.Value == "::")
        {
            return false;
        }

        if (current.Type == TokenType.Operator && current.Value == "::")
        {
            return false;
        }

        // Не нужен пробел после @ в операторах @>, <@, etc.
        if (prev.Type == TokenType.Operator && prev.Value.StartsWith('@'))
        {
            return false;
        }

        // В остальных случаях нужен пробел
        return true;
    }
}