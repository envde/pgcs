using PgCs.Core.Parsing.Common;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.BlockParsing;

/// <summary>
/// Аккумулятор токенов для построения SQL блока
/// Собирает токены до точки с запятой
/// </summary>
public sealed class BlockTokenAccumulator
{
    private readonly List<SqlToken> _tokens = [];
    private SqlToken? _startToken;

    /// <summary>
    /// Первый токен блока (для определения начала)
    /// </summary>
    public SqlToken? StartToken => _startToken;

    /// <summary>
    /// Последний токен блока
    /// </summary>
    public SqlToken? EndToken => _tokens.Count > 0 ? _tokens[^1] : null;

    /// <summary>
    /// Все токены блока (включая trivia)
    /// </summary>
    public IReadOnlyList<SqlToken> AllTokens => _tokens;

    /// <summary>
    /// Токены без trivia (только значащие)
    /// </summary>
    public IReadOnlyList<SqlToken> ContentTokens => 
        _tokens.Where(t => t.IsSignificant()).ToList();

    /// <summary>
    /// Количество токенов
    /// </summary>
    public int Count => _tokens.Count;

    /// <summary>
    /// Пуст ли аккумулятор
    /// </summary>
    public bool IsEmpty => _tokens.Count == 0;

    /// <summary>
    /// Добавляет токен в аккумулятор
    /// </summary>
    public void Add(SqlToken token)
    {
        _startToken ??= token;
        _tokens.Add(token);
    }

    /// <summary>
    /// Очищает аккумулятор
    /// </summary>
    public void Clear()
    {
        _tokens.Clear();
        _startToken = null;
    }
}