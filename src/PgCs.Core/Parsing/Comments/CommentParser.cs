using System.Text.RegularExpressions;
using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.Comments;

/// <summary>
/// Универсальный парсер комментариев для PostgreSQL кода
/// </summary>
/// <remarks>
/// Объединяет функциональность парсинга header комментариев, inline комментариев и метаданных.
/// Поддерживает два формата служебных полей в комментариях:
/// 1. Формат с двоеточием: field: value
/// 2. Формат со скобками: field(value)
/// 
/// Примеры:
/// <code>
/// -- comment: User identifier; to_type: BIGINT; to_name: user_id
/// -- comment(User identifier); to_type(BIGINT); to_name(user_id)
/// </code>
/// </remarks>
public sealed partial class CommentParser
{
    private readonly SqlContentBuilder _contentBuilder = new();

    // Regex для парсинга служебных полей
    [GeneratedRegex(@"(\w+)\s*:\s*([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColonFormatRegex();

    [GeneratedRegex(@"(\w+)\s*\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ParenFormatRegex();

    // ============================================================
    // Header комментарии
    // ============================================================

    /// <summary>
    /// Извлекает header комментарии из токенов до начала блока
    /// </summary>
    /// <param name="tokens">Токены для анализа</param>
    /// <returns>Объединенный header комментарий или null</returns>
    public string? ExtractHeaderComment(IReadOnlyList<SqlToken> tokens)
    {
        var comments = new List<string>();
        var hasEmptyLineAfterComment = false;

        foreach (var token in tokens)
        {
            if (token.IsSignificant())
            {
                // Достигли не-trivia токена - заканчиваем сбор
                break;
            }

            // Обработка пустых строк
            if (token.Type == TokenType.Whitespace && SqlTextHelper.ContainsEmptyLine(token.ValueMemory.Span))
            {
                hasEmptyLineAfterComment = true;
            }

            // Обработка комментариев
            if (token.Type == TokenType.LineComment)
            {
                if (hasEmptyLineAfterComment)
                {
                    // Пустая строка перед комментарием - сбрасываем старые
                    comments.Clear();
                    hasEmptyLineAfterComment = false;
                }

                var commentText = SqlTextHelper.ExtractCommentText(token.ValueMemory.Span);
                comments.Add(commentText.ToString()); // Материализуем в string для хранения
            }
        }

        return comments.Count > 0 ? string.Join(" ", comments) : null;
    }

    // ============================================================
    // Inline комментарии
    // ============================================================

    /// <summary>
    /// Извлекает inline комментарии из токенов блока
    /// </summary>
    /// <param name="allTokens">Все токены (включая trivia)</param>
    /// <returns>Список inline комментариев с позициями</returns>
    public IReadOnlyList<InlineComment> ExtractInlineComments(IReadOnlyList<SqlToken> allTokens)
    {
        var comments = new List<InlineComment>();
        var contentPosition = 0;

        for (var i = 0; i < allTokens.Count; i++)
        {
            var token = allTokens[i];

            // Накапливаем позицию для content (только не-trivia токены)
            if (token.IsSignificant())
            {
                contentPosition += token.ValueMemory.Length;

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
                var commentText = SqlTextHelper.ExtractCommentText(token.ValueMemory.Span);
                var key = FindKeyBeforeComment(allTokens, i);

                comments.Add(new InlineComment
                {
                    Key = key,
                    Comment = commentText.ToString(), // Материализуем в string
                    Position = contentPosition
                });
            }
        }

        return comments;
    }

    /// <summary>
    /// Находит ключ (идентификатор) перед комментарием
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
            return SqlTextHelper.UnquoteIdentifier(token.ValueMemory.Span).ToString();
        }

        return "unknown";
    }

    // ============================================================
    // Метаданные комментариев
    // ============================================================

    /// <summary>
    /// Парсит inline-комментарий и извлекает метаданные
    /// </summary>
    /// <param name="commentText">Текст комментария (может содержать префикс --)</param>
    /// <returns>Извлеченные метаданные или Empty если комментарий пустой</returns>
    public CommentMetadata ParseMetadata(string? commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
        {
            return CommentMetadata.Empty;
        }

        // Убираем префикс -- если он есть
        var cleanText = commentText.AsSpan().TrimStart('-').Trim();

        if (cleanText.IsEmpty)
        {
            return CommentMetadata.Empty;
        }

        var cleanString = cleanText.ToString();

        // Проверяем, есть ли служебные поля
        if (!HasFields(cleanString))
        {
            // Нет служебных полей - весь текст является комментарием
            return new CommentMetadata
            {
                Comment = cleanString
            };
        }

        // Извлекаем служебные поля
        TryExtractField(cleanString, "comment", out var comment);
        TryExtractField(cleanString, "to_type", out var toDataType);
        TryExtractField(cleanString, "to_name", out var toName);

        // Если поле comment не найдено, но есть другие поля - используем весь текст
        if (string.IsNullOrWhiteSpace(comment))
        {
            comment = cleanString;
        }

        return new CommentMetadata
        {
            Comment = comment ?? string.Empty,
            ToDataType = toDataType,
            ToName = toName
        };
    }

    /// <summary>
    /// Пытается извлечь значение конкретного поля из текста
    /// </summary>
    public bool TryExtractField(string text, string fieldName, out string? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        // Пробуем формат с двоеточием: field: value
        var colonMatches = ColonFormatRegex().Matches(text);
        foreach (Match match in colonMatches)
        {
            if (match.Groups[1].Value.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                value = match.Groups[2].Value.Trim();
                return !string.IsNullOrEmpty(value);
            }
        }

        // Пробуем формат со скобками: field(value)
        var parenMatches = ParenFormatRegex().Matches(text);
        foreach (Match match in parenMatches)
        {
            if (match.Groups[1].Value.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                value = match.Groups[2].Value.Trim();
                return !string.IsNullOrEmpty(value);
            }
        }

        return false;
    }

    /// <summary>
    /// <summary>
    /// Проверяет, содержит ли текст служебные поля
    /// </summary>
    public bool HasFields(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return ColonFormatRegex().IsMatch(text) || ParenFormatRegex().IsMatch(text);
    }
}
