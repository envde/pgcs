using PgCs.Core.Parsing.Comments;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.Blocks;

/// <summary>
/// Парсер SQL блоков из токенизированного PostgreSQL кода
/// </summary>
/// <remarks>
/// Разбивает SQL код на логические блоки, разделенные точкой с запятой.
/// Каждый блок содержит:
/// - Content: нормализованный SQL код (без trivia)
/// - RawContent: исходный SQL код (с whitespace и комментариями)
/// - HeaderComment: комментарий перед блоком
/// - InlineComments: комментарии внутри блока
/// 
/// Использует двухфазную обработку:
/// 1. Токенизация (SqlTokenizer)
/// 2. Группировка токенов в блоки (BlockParser)
/// 
/// Оптимизирован для .NET 9 с использованием ArrayPool и capacity hints.
/// </remarks>
public sealed class BlockParser
{
    private readonly IReadOnlyList<SqlToken> _allTokens;
    private readonly string _sourceText;
    private int _position;

    // Компоненты для парсинга и построения блоков
    private readonly List<SqlToken> _blockTokens = [];
    private readonly List<SqlToken> _headerTokens = [];
    private readonly SqlContentBuilder _contentBuilder = new();
    private readonly CommentParser _commentParser = new();

    /// <summary>
    /// Создает новый парсер блоков из токенов
    /// </summary>
    /// <param name="tokens">Список токенов из SqlTokenizer</param>
    /// <param name="sourceText">Исходный SQL текст</param>
    public BlockParser(IReadOnlyList<SqlToken> tokens, string sourceText)
    {
        _allTokens = tokens;
        _sourceText = sourceText;
    }

    /// <summary>
    /// Парсит SQL текст в список блоков (высокоуровневый статический API)
    /// </summary>
    /// <param name="sql">SQL текст для парсинга</param>
    /// <returns>Список SQL блоков, разделенных точками с запятой</returns>
    /// <exception cref="ArgumentException">Если sql null или пустой</exception>
    /// <remarks>
    /// Это удобный метод, который выполняет токенизацию и парсинг в один вызов.
    /// Для большего контроля используйте SqlTokenizer и BlockParser напрямую.
    /// </remarks>
    public static IReadOnlyList<SqlBlock> Parse(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        // Фаза 1: Токенизация SQL
        var tokenizer = new SqlTokenizer(sql);
        var tokens = tokenizer.Tokenize(sql);

        // Фаза 2: Парсинг блоков из токенов
        var parser = new BlockParser(tokens, sql);
        return parser.ParseBlocks();
    }

    /// <summary>
    /// Парсит все блоки из токенов (с оптимизацией памяти через capacity hint)
    /// </summary>
    private IReadOnlyList<SqlBlock> ParseBlocks()
    {
        // Capacity hint: считаем точки с запятой для уменьшения реаллокаций
        var estimatedBlockCount = EstimateBlockCount(_allTokens);
        var blocks = new List<SqlBlock>(estimatedBlockCount);

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
    /// Оценивает количество блоков по точкам с запятой
    /// </summary>
    private static int EstimateBlockCount(IReadOnlyList<SqlToken> tokens)
    {
        var count = 0;

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.IsSignificant() && token.Type == TokenType.Semicolon)
            {
                count++;
            }
        }

        // Минимум 1 блок, даже если нет точек с запятой
        return Math.Max(count, 1);
    }

    /// <summary>
    /// Парсит один SQL блок
    /// </summary>
    private SqlBlock? ParseBlock()
    {
        // Собираем header комментарии и пропускаем trivia до начала блока
        CollectHeaderTokens();

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
        return BuildBlock(_blockTokens, _headerTokens);
    }

    /// <summary>
    /// Строит SqlBlock из токенов
    /// </summary>
    private SqlBlock BuildBlock(IReadOnlyList<SqlToken> blockTokens, IReadOnlyList<SqlToken> headerTokens)
    {
        if (blockTokens.Count == 0)
        {
            throw new ArgumentException("Tokens list cannot be empty", nameof(blockTokens));
        }

        var startToken = blockTokens[0];
        var endToken = blockTokens[^1];

        // Строим Content (только значащие токены)
        var content = _contentBuilder.Build(blockTokens);

        // Извлекаем header комментарий
        var headerComment = _commentParser.ExtractHeaderComment(headerTokens);

        // Извлекаем RawContent (включая все trivia)
        var rawContent = ExtractRawContent(startToken, endToken, headerComment);

        // Извлекаем inline комментарии
        var inlineComments = _commentParser.ExtractInlineComments(blockTokens);

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
    /// Извлекает RawContent из исходного текста (zero allocation с string.Create)
    /// </summary>
    private string ExtractRawContent(SqlToken startToken, SqlToken endToken, string? headerComment)
    {
        var start = startToken.Span.Start;
        var end = endToken.Span.End;
        var blockSpan = _sourceText.AsSpan(start, end - start);

        // Без header - просто возвращаем blockContent
        if (headerComment is null)
        {
            return blockSpan.ToString();
        }

        // С header - собираем через string.Create (одна аллокация вместо 4+)
        const string prefix = "-- ";
        var newLine = Environment.NewLine;

        // Материализуем blockSpan в строку (нужна для closure)
        var blockContent = blockSpan.ToString();
        var totalLength = prefix.Length + headerComment.Length + newLine.Length + blockContent.Length;

        return string.Create(totalLength, (prefix, headerComment, newLine, blockContent), static (span, state) =>
        {
            var (prefix, header, newLine, block) = state;
            var position = 0;

            // Копируем "-- "
            prefix.AsSpan().CopyTo(span[position..]);
            position += prefix.Length;

            // Копируем headerComment
            header.AsSpan().CopyTo(span[position..]);
            position += header.Length;

            // Копируем Environment.NewLine
            newLine.AsSpan().CopyTo(span[position..]);
            position += newLine.Length;

            // Копируем blockContent
            block.AsSpan().CopyTo(span[position..]);
        });
    }

    /// <summary>
    /// Собирает header токены (комментарии и trivia перед блоком)
    /// </summary>
    private void CollectHeaderTokens()
    {
        _headerTokens.Clear();

        while (!IsAtEnd())
        {
            var token = Current();

            if (token.IsSignificant())
            {
                // Достигли не-trivia токена - заканчиваем сбор
                break;
            }

            _headerTokens.Add(token);
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