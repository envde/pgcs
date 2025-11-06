using PgCs.Core.Extraction.Block;
using PgCs.Core.Parsing.Common;
using PgCs.Core.Parsing.CommentParsing;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.BlockParsing;

/// <summary>
/// Парсер SQL блоков на основе токенов
/// Разделяет токены на блоки по точкам с запятой
/// </summary>
public sealed class BlockParser
{
    private readonly IReadOnlyList<SqlToken> _allTokens;
    private readonly string _sourceText;
    private int _position;

    // Компоненты
    private readonly BlockTokenAccumulator _tokenAccumulator = new();
    private readonly HeaderCommentCollector _headerComments = new();
    private readonly BlockBuilder _blockBuilder = new();

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
    /// Парсит все блоки из токенов
    /// </summary>
    public IReadOnlyList<SqlBlock> ParseBlocks()
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
    public SqlBlock? ParseBlock()
    {
        // Собираем header комментарии и пропускаем trivia до начала блока
        CollectHeaderCommentsAndTrivia();

        if (IsAtEnd())
        {
            return null;
        }

        _tokenAccumulator.Clear();

        // Собираем токены до точки с запятой
        while (!IsAtEnd())
        {
            var token = Current();
            Advance();

            _tokenAccumulator.Add(token);

            // Конец блока - точка с запятой (только если это не trivia)
            if (token.IsSignificant() && token.Type == TokenType.Semicolon)
            {
                break;
            }
        }

        if (_tokenAccumulator.IsEmpty)
        {
            return null;
        }

        // Строим блок
        var headerComment = _headerComments.ExtractAndClear();
        return _blockBuilder.Build(_tokenAccumulator.AllTokens, _sourceText, headerComment);
    }

    /// <summary>
    /// Собирает header комментарии и пропускает trivia
    /// </summary>
    public void CollectHeaderCommentsAndTrivia()
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
            if (token.Type == TokenType.Whitespace && TextHelper.ContainsEmptyLine(token.Value))
            {
                _headerComments.MarkEmptyLine();
            }

            // Обработка комментариев
            if (token.Type == TokenType.LineComment)
            {
                var commentText = TextHelper.ExtractCommentText(token.Value);
                _headerComments.AddComment(commentText);
            }

            Advance();
        }
    }

    /// <summary>
    /// Достигнут ли конец токенов
    /// </summary>
    public bool IsAtEnd() => 
        _position >= _allTokens.Count || Current().Type == TokenType.EndOfFile;

    /// <summary>
    /// Получить текущий токен
    /// </summary>
    public SqlToken Current() => _allTokens[_position];

    /// <summary>
    /// Переместиться к следующему токену
    /// </summary>
    public void Advance() => _position++;
}