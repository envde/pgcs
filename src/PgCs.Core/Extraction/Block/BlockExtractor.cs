using PgCs.Core.Parsing.BlockParsing;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Extraction.Block;

/// <summary>
/// Извлекатель SQL блоков на основе токенизации
/// Использует SqlTokenizer и BlockParser для корректного парсинга
/// </summary>
/// <remarks>
/// Преимущества перед построчным парсером:
/// - Корректная обработка строковых литералов ('text with -- fake comment')
/// - Правильная обработка dollar-quoted strings ($$body$$, $tag$body$tag$)
/// - Понимание вложенных комментариев /* /* nested */ */
/// - Точная балансировка скобок
/// - Поддержка всех операторов PostgreSQL (||, @>, <@, ->, etc.)
/// </remarks>
public sealed class BlockExtractor : IBlockExtractor
{
    private readonly SqlTokenizer _tokenizer = new();

    /// <inheritdoc />
    public IReadOnlyList<SqlBlock> Extract(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        // Фаза 1: Токенизация SQL
        var tokens = _tokenizer.Tokenize(sql);

        // Фаза 2: Парсинг блоков из токенов
        var parser = new BlockParser(tokens, sql);
        return parser.ParseBlocks();
    }
}