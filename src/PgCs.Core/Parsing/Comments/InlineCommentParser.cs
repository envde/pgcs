using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.Comments;

/// <summary>
/// Парсер inline комментариев из токенов
/// Находит комментарии внутри блока и связывает их с ключами
/// </summary>
public sealed class InlineCommentParser
{
    private readonly SqlContentBuilder _contentBuilder = new();

    /// <summary>
    /// Извлекает inline комментарии из токенов блока
    /// </summary>
    /// <param name="allTokens">Все токены (включая trivia)</param>
    /// <returns>Список inline комментариев с позициями</returns>
    public IReadOnlyList<InlineComment> Extract(IReadOnlyList<SqlToken> allTokens)
    {
        var comments = new List<InlineComment>();
        var contentPosition = 0;

        for (var i = 0; i < allTokens.Count; i++)
        {
            var token = allTokens[i];

            // Накапливаем позицию для content (только не-trivia токены)
            if (token.IsSignificant())
            {
                contentPosition += token.Value.Length;

                // Добавляем пробел если нужен между текущим и следующим токеном
                var nextToken = TokenSequenceHelper.FindNextSignificantToken(allTokens, i + 1);
                if (nextToken.HasValue && _contentBuilder.NeedsSpaceBetween(token, nextToken.Value))
                {
                    contentPosition++; // Учитываем пробел
                }
            }

            // Inline комментарий (только LineComment)
            if (token.Type == TokenType.LineComment)
            {
                var commentText = SqlTextHelper.ExtractCommentText(token.Value);
                var key = FindKeyBeforeComment(allTokens, i);

                comments.Add(new InlineComment
                {
                    Key = key,
                    Comment = commentText,
                    Position = contentPosition
                });
            }
        }

        return comments;
    }

    /// <summary>
    /// Находит ключ (идентификатор) перед комментарием
    /// Ищет ближайший идентификатор перед комментарием, игнорируя keywords и литералы
    /// </summary>
    private static string FindKeyBeforeComment(IReadOnlyList<SqlToken> tokens, int commentIndex)
    {
        // Ищем назад первый значащий идентификатор
        for (var i = commentIndex - 1; i >= 0; i--)
        {
            var token = tokens[i];

            // Пропускаем trivia
            if (token.IsTrivia())
            {
                continue;
            }

            // Пропускаем всё кроме идентификаторов
            if (!token.IsIdentifier())
            {
                continue;
            }

            // Нашли идентификатор - возвращаем его
            return SqlTextHelper.UnquoteIdentifier(token.Value);
        }

        return "unknown";
    }
}
