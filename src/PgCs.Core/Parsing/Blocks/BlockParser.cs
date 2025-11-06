using PgCs.Core.Parsing.Comments;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.Blocks;

/// <summary>
/// Парсер SQL блоков на основе токенов.
/// Использует SqlTokenizer для токенизации и разделяет блоки по точкам с запятой.
/// </summary>
/// <remarks>
/// Преимущества перед построчным парсером:
/// - Корректная обработка строковых литералов ('text with -- fake comment')
/// - Правильная обработка dollar-quoted strings ($$body$$, $tag$body$tag$)
/// - Понимание вложенных комментариев /* /* nested */ */
/// - Точная балансировка скобок
/// - Поддержка всех операторов PostgreSQL (||, @>, <@, ->, etc.)
/// </remarks>
public sealed class BlockParser
{
    private readonly IReadOnlyList<SqlToken> _allTokens;
    private readonly string _sourceText;
    private int _position;

    // Компоненты для парсинга и построения блоков
    private readonly List<SqlToken> _blockTokens = [];
    private readonly HeaderCommentParser _headerComments = new();
    private readonly SqlContentBuilder _contentBuilder = new();
    private readonly InlineCommentParser _inlineCommentParser = new();

    /// <summary>
    /// Создает парсер блоков
    /// </summary>
    /// <param name="tokens">Все токены из токенизатора</param>
    /// <param name="sourceText">Исходный SQL текст</param>
    public BlockParser(IReadOnlyList<SqlToken> tokens, string sourceText)
    {
        _allTokens = tokens;
        _sourceText = sourceText;
    }

    /// <summary>
    /// Парсит SQL текст в список блоков (высокоуровневый API).
    /// </summary>
    /// <param name="sql">SQL текст для парсинга</param>
    /// <returns>Список SQL блоков</returns>
    /// <exception cref="ArgumentException">Если SQL текст пустой или состоит из пробелов</exception>
    public static IReadOnlyList<SqlBlock> Parse(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        // Фаза 1: Токенизация SQL
        var tokenizer = new SqlTokenizer();
        var tokens = tokenizer.Tokenize(sql);

        // Фаза 2: Парсинг блоков из токенов
        var parser = new BlockParser(tokens, sql);
        return parser.ParseBlocks();
    }

    /// <summary>
    /// Парсит все блоки из токенов
    /// </summary>
    private IReadOnlyList<SqlBlock> ParseBlocks()
    {
        var blocks = new List<SqlBlock>();

        while (!IsAtEnd())
        {
            var block = ParseBlock();
            if (block is not null)
            {
                blocks.Add(block);
            }
        }

        return blocks;
    }

    /// <summary>
    /// Парсит один SQL блок
    /// </summary>
    private SqlBlock? ParseBlock()
    {
        // Собираем header комментарии и пропускаем trivia до начала блока
        CollectHeaderCommentsAndTrivia();

        if (IsAtEnd())
        {
            return null;
        }

        _blockTokens.Clear();

        // Собираем токены до точки с запятой
        while (!IsAtEnd())
        {
            var token = Current();
            Advance();

            _blockTokens.Add(token);

            // Конец блока - точка с запятой (только если это не trivia)
            if (token.IsSignificant() && token.Type == TokenType.Semicolon)
            {
                break;
            }
        }

        if (_blockTokens.Count == 0)
        {
            return null;
        }

        // Строим блок
        var headerComment = _headerComments.ExtractAndClear();
        return BuildBlock(_blockTokens, headerComment);
    }

    /// <summary>
    /// Строит SqlBlock из токенов
    /// </summary>
    /// <param name="allTokens">Все токены блока (включая trivia)</param>
    /// <param name="headerComment">Header комментарий (если есть)</param>
    /// <returns>Готовый SqlBlock</returns>
    private SqlBlock BuildBlock(IReadOnlyList<SqlToken> allTokens, string? headerComment)
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
        var rawContent = ExtractRawContent(startToken, endToken, headerComment);

        // Извлекаем inline комментарии
        var inlineComments = _inlineCommentParser.Extract(allTokens);

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
    /// <param name="startToken">Первый токен блока</param>
    /// <param name="endToken">Последний токен блока</param>
    /// <param name="headerComment">Header комментарий (если есть)</param>
    /// <returns>Полный исходный текст блока с комментариями</returns>
    private string ExtractRawContent(SqlToken startToken, SqlToken endToken, string? headerComment)
    {
        var start = startToken.Span.Start;
        var end = endToken.Span.End;

        var blockContent = _sourceText[start..end];

        // Добавляем header комментарии если есть
        if (headerComment is not null)
        {
            return $"-- {headerComment}{Environment.NewLine}{blockContent}";
        }

        return blockContent;
    }

    /// <summary>
    /// Собирает header комментарии и пропускает trivia
    /// </summary>
    private void CollectHeaderCommentsAndTrivia()
    {
        while (!IsAtEnd())
        {
            var token = Current();

            if (token.IsSignificant())
            {
                // Достигли не-trivia токена - заканчиваем сбор header comments
                break;
            }

            // Обработка пустых строк (в whitespace токене)
            if (token.Type == TokenType.Whitespace && SqlTextHelper.ContainsEmptyLine(token.Value))
            {
                _headerComments.MarkEmptyLine();
            }

            // Обработка комментариев
            if (token.Type == TokenType.LineComment)
            {
                var commentText = SqlTextHelper.ExtractCommentText(token.Value);
                _headerComments.AddComment(commentText);
            }

            Advance();
        }
    }

    /// <summary>
    /// Достигнут ли конец токенов
    /// </summary>
    private bool IsAtEnd() => _position >= _allTokens.Count || Current().Type == TokenType.EndOfFile;

    /// <summary>
    /// Получить текущий токен
    /// </summary>
    private SqlToken Current() => _allTokens[_position];

    /// <summary>
    /// Переместиться к следующему токену
    /// </summary>
    private void Advance() => _position++;
}