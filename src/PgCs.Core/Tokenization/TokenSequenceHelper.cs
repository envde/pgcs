namespace PgCs.Core.Tokenization;

/// <summary>
/// Вспомогательный класс для работы с последовательностями токенов
/// </summary>
public static class TokenSequenceHelper
{
    /// <summary>
    /// Находит индекс первого вхождения токена в списке
    /// </summary>
    /// <param name="tokens">Список токенов</param>
    /// <param name="token">Токен для поиска</param>
    /// <returns>Индекс токена или -1 если не найден</returns>
    public static int IndexOf(IReadOnlyList<SqlToken> tokens, SqlToken token)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Equals(token))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Находит индексы диапазона токенов в списке
    /// </summary>
    /// <param name="allTokens">Полный список токенов</param>
    /// <param name="startToken">Начальный токен</param>
    /// <param name="endToken">Конечный токен</param>
    /// <returns>Кортеж (StartIndex, EndIndex) или (-1, -1) если не найдено</returns>
    public static (int StartIndex, int EndIndex) GetTokenRange(
        IReadOnlyList<SqlToken> allTokens,
        SqlToken startToken,
        SqlToken endToken)
    {
        var startIndex = IndexOf(allTokens, startToken);
        if (startIndex == -1)
        {
            return (-1, -1);
        }

        var endIndex = IndexOf(allTokens, endToken);
        if (endIndex == -1 || endIndex < startIndex)
        {
            return (-1, -1);
        }

        return (startIndex, endIndex);
    }

    /// <summary>
    /// Извлекает подпоследовательность токенов из диапазона
    /// </summary>
    /// <param name="tokens">Исходный список токенов</param>
    /// <param name="startIndex">Начальный индекс (включительно)</param>
    /// <param name="endIndex">Конечный индекс (включительно)</param>
    /// <returns>Список токенов в диапазоне</returns>
    public static IReadOnlyList<SqlToken> GetTokensInRange(
        IReadOnlyList<SqlToken> tokens,
        int startIndex,
        int endIndex)
    {
        if (startIndex < 0 || endIndex < 0 || startIndex > endIndex || endIndex >= tokens.Count)
        {
            return Array.Empty<SqlToken>();
        }

        var result = new List<SqlToken>(endIndex - startIndex + 1);
        for (var i = startIndex; i <= endIndex; i++)
        {
            result.Add(tokens[i]);
        }

        return result;
    }

    /// <summary>
    /// Фильтрует токены, оставляя только значащие (не trivia)
    /// </summary>
    public static IReadOnlyList<SqlToken> GetSignificantTokens(IReadOnlyList<SqlToken> tokens)
    {
        return tokens.Where(t => t.IsSignificant()).ToList();
    }

    /// <summary>
    /// Фильтрует токены, оставляя только trivia
    /// </summary>
    public static IReadOnlyList<SqlToken> GetTriviaTokens(IReadOnlyList<SqlToken> tokens)
    {
        return tokens.Where(t => t.IsTrivia()).ToList();
    }

    /// <summary>
    /// Находит следующий значащий токен после указанного индекса
    /// </summary>
    public static SqlToken? FindNextSignificantToken(IReadOnlyList<SqlToken> tokens, int startIndex)
    {
        for (var i = startIndex; i < tokens.Count; i++)
        {
            if (tokens[i].IsSignificant())
            {
                return tokens[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Находит предыдущий значащий токен до указанного индекса
    /// </summary>
    public static SqlToken? FindPreviousSignificantToken(IReadOnlyList<SqlToken> tokens, int startIndex)
    {
        for (var i = startIndex - 1; i >= 0; i--)
        {
            if (tokens[i].IsSignificant())
            {
                return tokens[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Подсчитывает количество значащих токенов в списке
    /// </summary>
    public static int CountSignificantTokens(IReadOnlyList<SqlToken> tokens)
    {
        return tokens.Count(t => t.IsSignificant());
    }

    /// <summary>
    /// Проверяет, все ли токены в списке являются trivia
    /// </summary>
    public static bool AreAllTrivia(IReadOnlyList<SqlToken> tokens)
    {
        return tokens.All(t => t.IsTrivia());
    }

    /// <summary>
    /// Проверяет, содержит ли последовательность токен определённого типа
    /// </summary>
    public static bool ContainsTokenType(IReadOnlyList<SqlToken> tokens, TokenType type)
    {
        return tokens.Any(t => t.Type == type);
    }
}

