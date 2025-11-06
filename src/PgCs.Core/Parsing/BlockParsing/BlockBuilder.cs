using PgCs.Core.Extraction.Block;
using PgCs.Core.Parsing.CommentParsing;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.BlockParsing;

/// <summary>
/// Строитель SqlBlock из токенов
/// Координирует работу всех компонентов для создания блока
/// </summary>
public sealed class BlockBuilder
{
    private readonly ContentBuilder _contentBuilder = new();
    private readonly InlineCommentExtractor _inlineCommentExtractor = new();

    /// <summary>
    /// Строит SqlBlock из токенов
    /// </summary>
    /// <param name="allTokens">Все токены блока (включая trivia)</param>
    /// <param name="sourceText">Исходный SQL текст</param>
    /// <param name="headerComment">Header комментарий (если есть)</param>
    /// <returns>Готовый SqlBlock</returns>
    public SqlBlock Build(
        IReadOnlyList<SqlToken> allTokens,
        string sourceText,
        string? headerComment)
    {
        if (allTokens.Count == 0)
        {
            throw new ArgumentException("Tokens list cannot be empty", nameof(allTokens));
        }

        var startToken = allTokens[0];
        var endToken = allTokens[^1];

        // Строим Content (только значащие токены)
        var content = _contentBuilder.Build(allTokens);

        // Извлекаем RawContent (включая все trivia)
        var rawContent = ExtractRawContent(startToken, endToken, sourceText, headerComment);

        // Извлекаем inline комментарии
        var inlineComments = _inlineCommentExtractor.Extract(allTokens);

        return new SqlBlock
        {
            Content = content,
            RawContent = rawContent,
            HeaderComment = headerComment,
            InlineComments = inlineComments.Count > 0 ? inlineComments : null,
            StartLine = startToken.Line,
            EndLine = endToken.Line
        };
    }

    /// <summary>
    /// Извлекает RawContent из исходного текста
    /// </summary>
    private static string ExtractRawContent(
        SqlToken startToken,
        SqlToken endToken,
        string sourceText,
        string? headerComment)
    {
        var start = startToken.Span.Start;
        var end = endToken.Span.End;

        var blockContent = sourceText[start..end];

        // Добавляем header комментарии если есть
        if (headerComment is not null)
        {
            return $"-- {headerComment}{Environment.NewLine}{blockContent}";
        }

        return blockContent;
    }
}