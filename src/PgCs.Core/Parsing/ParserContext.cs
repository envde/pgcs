using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing;

/// <summary>
/// Контекст для парсинга SQL токенов
/// Обеспечивает навигацию по токенам и управление состоянием
/// </summary>
public sealed class ParserContext
{
    private readonly IReadOnlyList<SqlToken> _tokens;
    private int _position;

    /// <summary>
    /// Текущая позиция токена (начинается с 0)
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// Общее количество токенов
    /// </summary>
    public int TokenCount => _tokens.Count;

    /// <summary>
    /// Указывает, были ли все токены обработаны
    /// </summary>
    public bool IsAtEnd => _position >= _tokens.Count;

    /// <summary>
    /// Текущий токен (или default если достигнут конец)
    /// </summary>
    public SqlToken Current => IsAtEnd ? default : _tokens[_position];

    /// <summary>
    /// Исходный SQL текст
    /// </summary>
    public required ReadOnlyMemory<char> Source { get; init; }

    /// <summary>
    /// Инициализирует новый экземпляр контекста парсера
    /// </summary>
    /// <param name="tokens">Список SQL токенов для обработки</param>
    public ParserContext(IReadOnlyList<SqlToken> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    /// <summary>
    /// Переходит к следующему токену
    /// </summary>
    /// <returns>Предыдущий текущий токен</returns>
    public SqlToken Advance()
    {
        var current = Current;
        if (!IsAtEnd)
            _position++;
        return current;
    }

    /// <summary>
    /// Просматривает следующий токен без перехода к нему
    /// </summary>
    /// <param name="offset">Смещение от текущей позиции (по умолчанию 1)</param>
    /// <returns>Токен на указанном смещении или default если выход за границы</returns>
    public SqlToken Peek(int offset = 1)
    {
        var index = _position + offset;
        return index >= 0 && index < _tokens.Count ? _tokens[index] : default;
    }

    /// <summary>
    /// Проверяет, соответствует ли текущий токен ожидаемому типу
    /// </summary>
    /// <param name="type">Ожидаемый тип токена</param>
    /// <returns>true если текущий токен соответствует типу, иначе false</returns>
    public bool Check(TokenType type) => !IsAtEnd && Current.Type == type;

    /// <summary>
    /// Потребляет текущий токен, если он соответствует ожидаемому типу
    /// </summary>
    /// <param name="type">Ожидаемый тип токена</param>
    /// <returns>true если токен был потреблён, иначе false</returns>
    public bool Match(TokenType type)
    {
        if (!Check(type))
            return false;
        Advance();
        return true;
    }

    /// <summary>
    /// Потребляет текущий токен, если он соответствует любому из ожидаемых типов
    /// </summary>
    /// <param name="types">Массив ожидаемых типов токенов</param>
    /// <returns>true если токен был потреблён, иначе false</returns>
    public bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Получает текстовое содержимое текущего токена из исходного текста
    /// </summary>
    /// <returns>Текст текущего токена или пустой span если достигнут конец</returns>
    public ReadOnlySpan<char> GetCurrentText() =>
        IsAtEnd ? ReadOnlySpan<char>.Empty : Source.Span.Slice(Current.Span.Start, Current.Span.Length);

    /// <summary>
    /// Получает текстовое содержимое указанного токена из исходного текста
    /// </summary>
    /// <param name="token">Токен, текст которого требуется получить</param>
    /// <returns>Текст указанного токена</returns>
    public ReadOnlySpan<char> GetTokenText(SqlToken token) =>
        Source.Span.Slice(token.Span.Start, token.Span.Length);

    /// <summary>
    /// Сохраняет текущую позицию для возможного отката
    /// </summary>
    /// <returns>Сохранённая позиция</returns>
    public int SavePosition() => _position;

    /// <summary>
    /// Восстанавливает ранее сохранённую позицию
    /// </summary>
    /// <param name="position">Позиция для восстановления</param>
    public void RestorePosition(int position) => _position = position;
}
