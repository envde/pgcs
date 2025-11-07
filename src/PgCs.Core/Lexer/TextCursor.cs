namespace PgCs.Core.Lexer;

/// <summary>
/// Снимок позиции курсора для возможности отката
/// </summary>
/// <param name="Position">Позиция в тексте (0-based)</param>
/// <param name="Line">Номер строки (начинается с 1)</param>
/// <param name="Column">Номер колонки (начинается с 1)</param>
public readonly record struct CursorPosition(int Position, int Line, int Column);

/// <summary>
/// Курсор для навигации по тексту
/// Предоставляет удобный API для токенизации
/// </summary>
public sealed class TextCursor
{
    private readonly string _text;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    /// <summary>
    /// Создаёт новый курсор для навигации по тексту
    /// </summary>
    /// <param name="text">Текст для навигации</param>
    public TextCursor(string text)
    {
        _text = text;
    }

    /// <summary>Текущая позиция в тексте (0-based)</summary>
    public int Position => _position;

    /// <summary>Текущая строка (начинается с 1)</summary>
    public int Line => _line;

    /// <summary>Текущая колонка (начинается с 1)</summary>
    public int Column => _column;

    /// <summary>Текущий символ (или '\0' если конец текста)</summary>
    public char Current => IsAtEnd() ? '\0' : _text[_position];

    /// <summary>Следующий символ (не перемещая курсор, или '\0' если конец)</summary>
    public char Peek() => _position + 1 >= _text.Length ? '\0' : _text[_position + 1];

    /// <summary>Достигнут ли конец текста</summary>
    public bool IsAtEnd() => _position >= _text.Length;

    /// <summary>
    /// Перемещает курсор на следующий символ
    /// </summary>
    /// <returns>Символ на предыдущей позиции</returns>
    public char Advance()
    {
        var ch = _text[_position];
        _position++;

        if (ch == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return ch;
    }

    /// <summary>
    /// Проверяет, совпадает ли текущая позиция с последовательностью символов
    /// </summary>
    /// <param name="sequence">Последовательность для проверки</param>
    /// <returns>true если текущая позиция начинается с указанной последовательности</returns>
    public bool MatchSequence(ReadOnlySpan<char> sequence)
    {
        if (_position + sequence.Length > _text.Length)
        {
            return false;
        }

        return _text.AsSpan(_position, sequence.Length).SequenceEqual(sequence);
    }

    /// <summary>
    /// Извлекает подстроку как Span (zero-allocation)
    /// </summary>
    /// <param name="start">Начальная позиция</param>
    /// <param name="length">Длина подстроки</param>
    /// <returns>Span с указанным участком текста</returns>
    public ReadOnlySpan<char> GetTextSpan(int start, int length)
    {
        if (start + length > _text.Length)
        {
            length = _text.Length - start;
        }

        return _text.AsSpan(start, length);
    }

    /// <summary>
    /// Создаёт снимок текущей позиции курсора
    /// </summary>
    /// <returns>Снимок с текущей позицией, строкой и колонкой</returns>
    public CursorPosition CreateSnapshot() => new(_position, _line, _column);

    /// <summary>
    /// Восстанавливает позицию курсора из снимка
    /// </summary>
    /// <param name="snapshot">Снимок позиции для восстановления</param>
    public void RestoreSnapshot(CursorPosition snapshot)
    {
        _position = snapshot.Position;
        _line = snapshot.Line;
        _column = snapshot.Column;
    }
}